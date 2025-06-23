using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ToltoonTTS2.TTS
{
    class TtsSAPI : ITts, IDisposable
    {
        private readonly SpeechSynthesizer _synth;
        private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
        private bool _isSpeaking = false;
        private readonly object _lock = new object();

        public string SkipCommandOne { get; set; } = "пропуск1";
        public string SkipCommandAll { get; set; } = "пропуск1все";

        public TtsSAPI()
        {
            _synth = new SpeechSynthesizer();
            _synth.SetOutputToDefaultAudioDevice();
            _synth.SpeakCompleted += Synth_SpeakCompleted;
        }

        public void Speak(ProcessedTtsMessage result)
        {
            if (string.IsNullOrWhiteSpace(result.Text)) return;

            // Обработка команд — выполняется мгновенно, не попадает в очередь
            if (result.Text.Contains(SkipCommandAll, StringComparison.OrdinalIgnoreCase))
            {
                ClearQueue();
                _synth.SpeakAsyncCancelAll();
                return;
            }

            if (result.Text.Contains(SkipCommandOne, StringComparison.OrdinalIgnoreCase))
            {
                var current = _synth.GetCurrentlySpokenPrompt();
                if (current != null)
                    _synth.SpeakAsyncCancel(current);
                return;
            }

            // Устанавливаем голос
            if (!string.IsNullOrEmpty(result.VoiceName))
            {
                SetVoice(result.VoiceName);
            }

            // Устанавливаем громкость (приводим float [0.0–1.0] к int [0–100])
            int volume = (int)(Math.Clamp(result.VoiceVolume, 0f, 1f) * 100);
            SetVolume(volume);

            // Устанавливаем скорость (приводим float к int [-10 – 10])
            int rate = (int)Math.Clamp(result.VoiceSpeed, -10f, 10f);
            SetRate(rate);

            // Очередь обычных сообщений
            _messageQueue.Enqueue(result.Text);
            lock (_lock)
            {
                if (!_isSpeaking)
                {
                    ProcessQueue();
                }
            }
        }


        private void ProcessQueue()
        {
            if (_messageQueue.TryDequeue(out var message))
            {
                _isSpeaking = true;

                // Обработка текста (упрощённая)
                message = PreprocessMessage(message);

                try
                {
                    _synth.SpeakAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка синтеза речи: {ex.Message}");
                    ClearQueue();
                    _synth.SpeakAsyncCancelAll();
                    _isSpeaking = false;
                }
            }
            else
            {
                _isSpeaking = false;
            }
        }

        private void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            lock (_lock)
            {
                _isSpeaking = false;
                ProcessQueue();
            }
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

        private void ClearQueue()
        {
            while (_messageQueue.TryDequeue(out _)) { }
            _isSpeaking = false;
        }

        public void Dispose()
        {
            _synth?.Dispose();
        }
    }
}
