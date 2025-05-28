using System.Speech.Synthesis;

namespace ToltoonTTS2.TTS
{
    class TtsSAPI : ITts, IDisposable
    {
        private readonly SpeechSynthesizer _synth;

        public TtsSAPI()
        {
            _synth = new SpeechSynthesizer();
            _synth.SetOutputToDefaultAudioDevice();
        }

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            _synth.SpeakAsync(text);
        }
        public void SetVoice(string voiceName)
        {
            if (_synth.GetInstalledVoices().Any(v => v.VoiceInfo.Name == voiceName))
                _synth.SelectVoice(voiceName);
        }

        public void SetRate(int rate) => _synth.Rate = rate;
        public void SetVolume(int volume) => _synth.Volume = volume;

        public void Dispose() => _synth?.Dispose();
    }
}
