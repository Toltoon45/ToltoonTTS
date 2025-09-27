

using System.Collections.ObjectModel;
using System.IO;

namespace ToltoonTTS2.Services.SaveLoadSettings
{
    class GetAvailableProfiles : ILoadProfilesList
    {
        public void GetListOfAvailableProfiles(ObservableCollection<string> comboBoxAvailableProfiles)
        {
            foreach (var file in Directory.GetFiles("DataForProgram/Profiles"))
            {
                comboBoxAvailableProfiles.Add(Path.GetFileName(file));
            }
        }
    }
}
