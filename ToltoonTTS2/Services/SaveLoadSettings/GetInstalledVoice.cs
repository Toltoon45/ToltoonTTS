using System.Collections.ObjectModel;
using System.IO;
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
                var cwd = Directory.GetCurrentDirectory();
                var ruFolderNames = Directory.GetDirectories($"{cwd}/models", "ru_*")
                                             .Select(Path.GetFileName);

                foreach (var folder in ruFolderNames)
                {
                    comboBoxAvailableVoices.Add(folder);
                }
            }
        }
    }
}
