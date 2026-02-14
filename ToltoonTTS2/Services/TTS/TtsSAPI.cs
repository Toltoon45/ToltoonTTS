using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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

        private IEnumerable<int> _dynamicSpeedSettings;

        private WaveOutEvent? _waveOut;

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
                var ready = await PrepareMessageAsync(msg);
                _audioQueue.Enqueue(ready);

                StartPlayback();
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

                int volume = (int)(Math.Clamp(msg.VoiceVolume, 0f, 1f) * 100);
                SetVolume(volume);

                int adjust = GetDynamicRateAdjustment(text.Length);
                int rate = (int)Math.Clamp(msg.VoiceSpeed + adjust, -10f, 10f);
                SetRate(rate);

                wav = GenerateSpeech(text);
            }
            else
            {
                wav = await PiperSharpTTS.GenerateVoice(
                    msg.VoiceName,
                    text,
                    msg.VoiceSpeed
                );
            }

            return new ReadyTtsMessage
            {
                Wav = wav,
                Original = msg
            };
        }

        private MemoryStream GenerateSpeech(string text)
        {
            var stream = new MemoryStream();

            _synth.SetOutputToWaveStream(stream);
            _synth.Speak(text);

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

            var reader = new WaveFileReader(ready.Wav);
            string msg = ready.Original.Text;
            var sampleProvider = reader.ToSampleProvider();
            
            //робовойс_тест
            ISampleProvider chain = reader.ToSampleProvider();
            chain = new VibratoSampleProvider(chain, 6f, 15f);
            float gain = CalculateNormalizationGain(sampleProvider);
            chain = new VolumeSampleProvider(chain)
            {
                Volume = gain
            };
            
            // Нормализация

            reader.Position = 0;
            sampleProvider = reader.ToSampleProvider();
            
            //var effect = panning

            _waveOut = new WaveOutEvent();
            _waveOut.Init(chain);

            _waveOut.PlaybackStopped += (s, e) =>
            {
                reader.Dispose();
                ready.Wav.Dispose();
                _waveOut.Dispose();
                _waveOut = null;

                PlayNext(); // без паузы
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

        public static float CalculateNormalizationGain(ISampleProvider provider, float targetPeak = 0.99f)
        {
            float max = 0f;
            float[] buffer = new float[4096];
            int read;

            while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    float abs = Math.Abs(buffer[i]);
                    if (abs > max)
                        max = abs;
                }
            }

            return max == 0 ? 1f : targetPeak / max;
        }
    }

    public class ReadyTtsMessage
    {
        public MemoryStream Wav { get; set; }
        public ProcessedTtsMessage Original { get; set; }
    }
}
