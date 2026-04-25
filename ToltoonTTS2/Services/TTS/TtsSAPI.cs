using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NickBuhro.Translit;
using System.Collections.Concurrent;
using System.IO;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace ToltoonTTS2.Services.TTS
{
    class TtsSAPI : ITts, IDisposable
    {
        private readonly SpeechSynthesizer _synth;

        // Очередь уже готовых WAV
        private readonly ConcurrentQueue<ReadyTtsMessage> _audioQueue = new();

        private bool _isPlaying = false;
        private readonly object _lock = new object();

        private bool _translitToRussian = true;

        private IEnumerable<int> _dynamicSpeedSettings;

        private WaveOutEvent? _waveOut;
        private AudioEffectSettings _audioEffectsSettings = new();
        private readonly Random _random = new();

        public string SkipCommandOne { get; set; } = "пропуск1";
        public string SkipCommandAll { get; set; } = "пропуск1все";

        public TtsSAPI()
        {
            _synth = new SpeechSynthesizer();
        }

        private bool IsPiperVoice(string voiceName)
        {
            return Regex.IsMatch(voiceName, @"^[a-z]{2}_[A-Z]{2}-");
        }

        public void Speak(ProcessedTtsMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Text))
                return;

            // Команды пропуска
            if (msg.Text.Contains(SkipCommandAll, StringComparison.OrdinalIgnoreCase))
            {
                ClearQueue();
                _waveOut?.Stop();
                return;
            }

            if (msg.Text.Contains(SkipCommandOne, StringComparison.OrdinalIgnoreCase))
            {
                _waveOut?.Stop();
                return;
            }

            // Асинхронная подготовка сообщения
            Task.Run(async () =>
            {
                try
                {
                    var ready = await PrepareMessageAsync(msg);
                    if (ready?.Wav == null || ready.Wav.Length == 0)
                        return;

                    _audioQueue.Enqueue(ready);
                    StartPlayback();
                }
                catch
                {
                    // Проглатываем ошибку подготовки сообщения, чтобы не ронять поток озвучки.
                }
            });
        }

        private async Task<ReadyTtsMessage> PrepareMessageAsync(ProcessedTtsMessage msg)
        {
            string text = PreprocessMessage(msg.Text);

            bool isPiper = IsPiperVoice(msg.VoiceName);
            MemoryStream wav;

            if (!isPiper)
            {
                SetVoice(msg.VoiceName);

                //int volume = (int)(Math.Clamp(msg.VoiceVolume, 0f, 1f) * 100);
                int volume = msg.VoiceVolume;
                SetVolume(volume);

                int adjust = GetDynamicRateAdjustment(text.Length);
                int rate = (int)Math.Clamp(msg.VoiceSpeed + adjust, -10f, 10f);
                SetRate(rate);

                wav = GenerateSpeech(
    text,
    msg.VoiceName,
    volume,
    rate);
            }
            else
            {
                wav = await PiperSharpTTS.GenerateVoice(
                    msg.VoiceName,
                    msg.VoiceVolume,
                    text,
                    msg.VoiceSpeed,
                    _translitToRussian
                );
            }

            return new ReadyTtsMessage
            {
                Wav = wav,
                Original = msg
            };
        }
        private MemoryStream GenerateSpeech(string text, string voice,
                                             int volume, int rate)
        {
            using var synth = new SpeechSynthesizer();

            synth.SelectVoice(voice);
            synth.Volume = volume;
            synth.Rate = rate;

            var stream = new MemoryStream();

            synth.SetOutputToWaveStream(stream);
            synth.Speak(text);

            stream.Position = 0;

            return stream;
        }

        private void StartPlayback()
        {
            lock (_lock)
            {
                if (_isPlaying) return;
                _isPlaying = true;
            }

            PlayNext();
        }

        private void PlayNext()
        {
            if (!_audioQueue.TryDequeue(out var ready))
            {
                _isPlaying = false;
                return;
            }

            WaveFileReader reader;
            try
            {
                reader = new WaveFileReader(ready.Wav);
            }
            catch
            {
                ready.Wav?.Dispose();
                PlayNext();
                return;
            }

            float normalizationGain = 1f;
            if (_audioEffectsSettings.Normalization?.Enabled == true)
            {
                if (_audioEffectsSettings.Normalization?.Enabled == true)
                {
                    var analysisProvider = reader.ToSampleProvider();
                    float targetRms = ResolveNormalizationTargetRms(_audioEffectsSettings.Normalization);
                    normalizationGain = VibratoSampleProvider.CalculateRmsNormalizationGain(analysisProvider, targetRms);
                    reader.Position = 0;
                }
            }

            ISampleProvider chain = reader.ToSampleProvider();

            chain = ApplySingleAudioEffect(chain);
            chain = ApplyNormalizationIfEnabled(chain, normalizationGain);

            //Воспроизведение
            _waveOut = new WaveOutEvent();
            _waveOut.Init(chain);

            _waveOut.PlaybackStopped += (s, e) =>
            {
                reader.Dispose();
                ready.Wav.Dispose();
                _waveOut.Dispose();
                _waveOut = null;

                PlayNext();
            };

            _waveOut.Play();
        }


        public void SetVoice(string voiceName)
        {
            if (_synth.GetInstalledVoices().Any(v => v.VoiceInfo.Name == voiceName))
                _synth.SelectVoice(voiceName);
        }

        public void SetRate(int rate) => _synth.Rate = rate;
        public void SetVolume(int volume) => _synth.Volume = volume;

        private string PreprocessMessage(string message)
        {
            message = Regex.Replace(message, @"\.{2,}", " . ");

            message = Regex.Replace(message,
                @"(?:https?:\/\/)?(?:www\.)?(?:x\.com|twitter\.com)[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]*|(?:https?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+",
                "ссылка");

            string emojiPattern = @"(?:[\u203C-\u3299\u00A9\u00AE\u2000-\u3300\uF000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF])";
            message = Regex.Replace(message, emojiPattern, string.Empty);

            return message;
        }

        private int GetDynamicRateAdjustment(int textLength)
        {
            int[] thresholds = { 100, 200, 300, 400, 500 };
            var settings = _dynamicSpeedSettings?.ToList();

            if (settings == null) return 0;

            for (int i = thresholds.Length - 1; i >= 0; i--)
            {
                if (textLength >= thresholds[i])
                    return settings[i];
            }

            return 0;
        }

        private void ClearQueue()
        {
            while (_audioQueue.TryDequeue(out _)) { }
            _isPlaying = false;
        }

        public void Dispose()
        {
            _synth?.Dispose();
        }

        public void SetDynamicSpeed(IEnumerable<int> settings)
        {
            _dynamicSpeedSettings = settings;
        }

        public void SetTranslitVoiceToRussian(bool v)
        {
            _translitToRussian = v;
        }

        public void SetAudioEffectsSettings(AudioEffectSettings settings)
        {
            _audioEffectsSettings = settings ?? new AudioEffectSettings();
        }

        private ISampleProvider ApplySingleAudioEffect(ISampleProvider chain)
        {
            var available = new List<Func<ISampleProvider, ISampleProvider>>();

            if (CanApply(_audioEffectsSettings.Vibrato))
            {
                available.Add(sp =>
                {
                    float strength = NormalizeStrength(_audioEffectsSettings.Vibrato.Strength);
                    float rate = 1f + strength * 11f;
                    float depth = 1f + strength * 19f;
                    return new VibratoSampleProvider(sp, rate, depth);
                });
            }

            if (CanApply(_audioEffectsSettings.Robot))
            {
                available.Add(sp =>
                {
                    float strength = NormalizeStrength(_audioEffectsSettings.Robot.Strength);
                    float frequency = 10f + strength * 110f;
                    float mix = 0.2f + strength * 0.8f;
                    return new RobotSampleProvider(sp, frequency, mix);
                });
            }

            //if (CanApply(_audioEffectsSettings.Delay))
            //{
            //    available.Add(sp =>
            //    {
            //        float strength = NormalizeStrength(_audioEffectsSettings.Delay.Strength);
            //        int delayMs = (int)(80 + strength * 720);
            //        float feedback = 0.1f + strength * 0.75f;
            //        float mix = 0.15f + strength * 0.75f;
            //        return new DelaySampleProvider(sp, delayMs, feedback, mix);
            //    });
            //}

            if (CanApply(_audioEffectsSettings.Distortion))
            {
                available.Add(sp =>
                {
                    float strength = NormalizeStrength(_audioEffectsSettings.Distortion.Strength);
                    float gain = 1f + strength * 19f;
                    float mix = 0.15f + strength * 0.85f;
                    return new DistortionSampleProvider(sp, gain, mix);
                });
            }

            if (available.Count == 0)
                return chain;

            int index = _random.Next(available.Count);
            return available[index](chain);
        }

        private ISampleProvider ApplyNormalizationIfEnabled(ISampleProvider chain, float gain)
        {
            if (_audioEffectsSettings.Normalization?.Enabled != true)
                return chain;

            return new VolumeSampleProvider(chain)
            {
                Volume = Math.Clamp(gain, 0f, 10f)
            };
        }

        private static float ResolveNormalizationTargetRms(EffectSetting? normalizationSettings)
        {
            if (normalizationSettings == null)
                return 0.15f;

            float normalizedVolume = Math.Clamp(normalizationSettings.Strength, 0, 100) / 100f;
            return 0.05f + (normalizedVolume * 0.25f);
        }

        private bool CanApply(EffectSetting effect)
        {
            if (effect == null || !effect.Enabled)
                return false;

            int chance = Math.Clamp(effect.Chance, 0, 100);
            return _random.Next(0, 100) < chance;
        }

        private static float NormalizeStrength(int value)
        {
            return Math.Clamp(value, 0, 100) / 100f;
        }

    }

    public class ReadyTtsMessage
    {
        public MemoryStream Wav { get; set; }
        public ProcessedTtsMessage Original { get; set; }
    }
}
