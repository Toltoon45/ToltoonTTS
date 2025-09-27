using System.Collections.ObjectModel;
using System.Speech.Synthesis;

namespace ToltoonTTS2.Services.SaveLoadSettings
{
    class GetInstalledVoice : ILoadAvailableVoices
    {
        public void GetListOfAvailableVoices(ObservableCollection<string> comboBoxAvailableVoices)
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                foreach(InstalledVoice voice in synth.GetInstalledVoices())
                {
                    comboBoxAvailableVoices.Add(voice.VoiceInfo.Name);
                }
            }
        }
    }
}
