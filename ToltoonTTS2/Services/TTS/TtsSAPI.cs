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
        private readonly ConcurrentQueue<ProcessedTtsMessage> _messageQueue = new();

        private IEnumerable<int> _dynamicSpeedSettings;

        private bool _isSpeaking = false;
        private readonly object _lock = new object();

        private string[] _pipeVoices =
{
    "ru_RU-denis-medium",
    "ru_RU-dmitri-medium",
    "ru_RU-irina-medium",
    "ru_RU-ruslan-medium"
};

        private WaveOutEvent? _waveOut;

        public string SkipCommandOne { get; set; } = "пропуск1";
        public string SkipCommandAll { get; set; } = "пропуск1все";

        public TtsSAPI()
        {
            _synth = new SpeechSynthesizer();
            //_synth.SetOutputToDefaultAudioDevice();
            MemoryStream audioStream = new MemoryStream();
            _synth.SetOutputToWaveStream(audioStream);

        }

        private bool IsPiperVoice(string voiceName)
        {
            if (voiceName.Contains("_") || voiceName.Contains("-")) return true;
            else return false;
        }

        public void Speak(ProcessedTtsMessage result)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(result.Text)) return;

                // Обработка команд — выполняется мгновенно, не попадает в очередь
                if (result.Text.Contains(SkipCommandAll, StringComparison.OrdinalIgnoreCase))
                {
                    ClearQueue();
                    _waveOut.Stop();
                    return;
                }

                if (result.Text.Contains(SkipCommandOne, StringComparison.OrdinalIgnoreCase))
                {
                    if (_waveOut != null)
                        _waveOut.Stop();

                    return;
                }

                // Устанавливаем голос
                bool _isPiperVoice = IsPiperVoice(result.VoiceName);
                int AdjustSpeed = GetDynamicRateAdjustment(result.Text.Length);
                if (!_isPiperVoice)
                {
                    SetVoice(result.VoiceName);
                    // Устанавливаем громкость (приводим float [0.0–1.0] к int [0–100])
                    int volume = (int)(Math.Clamp(result.VoiceVolume, 0f, 1f) * 100);
                    SetVolume(volume);
                    // Устанавливаем скорость (приводим float к int [-10 – 10])
                    int rate = (int)Math.Clamp(result.VoiceSpeed + AdjustSpeed, -10f, 10f);
                    SetRate(rate);
                }
                else
                {

                }

                    // Очередь обычных сообщений
                    _messageQueue.Enqueue(result);
                lock (_lock)
                {
                    if (!_isSpeaking)
                    {
                        ProcessQueue(_isPiperVoice);
                    }
                }
            }
            catch { }
            
        }

        private MemoryStream GenerateSpeech(string text)
        {
            //проверка, является ли голос piper

            var stream = new MemoryStream();
            _synth.SelectVoice(_synth.Voice.Name);
            _synth.Volume = _synth.Volume;
            _synth.Rate = _synth.Rate;

            _synth.SetOutputToWaveStream(stream);
            _synth.Speak(text);

            stream.Position = 0;
            return stream;
        }

        private async void ProcessQueue(bool _isPiperVoice)
        {
            MemoryStream wavMessage = new MemoryStream();
            if (!_messageQueue.TryDequeue(out var result))
            {
                _isSpeaking = false;
                return;
            }

            _isSpeaking = true;

            var message = PreprocessMessage(result.Text);

            if (!_isPiperVoice)
            {
                wavMessage = GenerateSpeech(message);
            }
            else if (_isPiperVoice)
            {
                wavMessage = await PiperSharpTTS.GenerateVoice(result.VoiceName, result.Text, result.VoiceSpeed);

            }


            var reader = new WaveFileReader(wavMessage);

            // 1. Первый проход — ищем пик
            var sampleProvider = reader.ToSampleProvider();
            float gain = CalculateNormalizationGain(sampleProvider);

            // 2. Сброс
            reader.Position = 0;
            sampleProvider = reader.ToSampleProvider();

            // 3. Нормализация
            var volumeProvider = new VolumeSampleProvider(sampleProvider)
            {
                Volume = gain + 1
            };

            // (опционально) рация
             //var distorted = new DistortionSampleProvider(volumeProvider) { Drive = 10f };

            _waveOut = new WaveOutEvent();

            _waveOut.PlaybackStopped += (s, e) =>
            {
                _waveOut.Dispose();
                reader.Dispose();
                wavMessage.Dispose();

                lock (_lock)
                {
                    _isSpeaking = false;
                    ProcessQueue(_isPiperVoice);
                }
            };
            //volumeProvider = PiperSharpTTS.GenerateVoice();
            _waveOut.Init(volumeProvider);
            _waveOut.Play();
        }



        public void PlayStream(MemoryStream wavStream)
        {
            wavStream.Position = 0;

            // Для WAV используем WaveFileReader
            // Остановить предыдущее проигрывание
            _waveOut?.Stop();
            _waveOut?.Dispose();

            var reader = new WaveFileReader(wavStream);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(reader);

            _waveOut.Play();

            // Когда звук доиграет → переход к следующему
            _waveOut.PlaybackStopped += (s, e) =>
            {
                reader.Dispose();
                _waveOut.Dispose();
                _waveOut = null;

                lock (_lock)
                {
                    _isSpeaking = false;
                    ProcessQueue(false);
                }
            };
            wavStream.Position = 0;
        }

        public void SetVoice(string voiceName)
        {
            if (_synth.GetInstalledVoices().Any(v => v.VoiceInfo.Name == voiceName))
            {
                _synth.SelectVoice(voiceName);
            }
        }

        public void SetRate(int rate) => _synth.Rate = rate;
        public void SetVolume(int volume) => _synth.Volume = volume;

        private string PreprocessMessage(string message)
        {
            // Удаление повторяющихся точек
            message = Regex.Replace(message, @"\.{2,}", " . ");

            // Пример удаления ссылок
            message = Regex.Replace(message,
                @"(?:https?:\/\/)?(?:www\.)?(?:x\.com|twitter\.com)[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]*|(?:https?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+",
                "ссылка");

            // Удаление эмодзи (по желанию можно добавить флаг и сделать опционально)
            string emojiPattern = @"(?:[\u203C-\u3299\u00A9\u00AE\u2000-\u3300\uF000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF])";
            message = Regex.Replace(message, emojiPattern, string.Empty);

            return message;
        }
        private int GetDynamicRateAdjustment(int textLength)
        {
            int[] thresholds = { 100, 200, 300, 400, 500 };
            var settings = _dynamicSpeedSettings?.ToList();

            for (int i = thresholds.Length - 1; i >= 0; i--)
            {
                if (textLength >= thresholds[i])
                    return settings[i]; // теперь settings поддерживает индекс
            }

            return 0;
        }

        private void ClearQueue()
        {
            while (_messageQueue.TryDequeue(out _)) { }
            _isSpeaking = false;
        }

        public void Dispose()
        {
            _synth?.Dispose();
        }

        public void SetDynamicSpeed(IEnumerable<int> settings)
        {
            _dynamicSpeedSettings = settings;
        }

        public void SetTtsForChannelPointsEnabled(bool enabled)
        {
            _isSpeaking = enabled;
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

    public class DistortionSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        public float Drive { get; set; } = 5f; // 1..20

        public DistortionSampleProvider(ISampleProvider source)
        {
            _source = source;
            WaveFormat = source.WaveFormat;
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = _source.Read(buffer, offset, count);

            for (int i = 0; i < read; i++)
            {
                float sample = buffer[offset + i] * Drive;

                // ЖЁСТКИЙ КЛИППИНГ
                if (sample > 1f) sample = 1f;
                if (sample < -1f) sample = -1f;

                buffer[offset + i] = sample;
            }

            return read;
        }
    }


}
