using System.Collections.ObjectModel;

namespace ToltoonTTS2.Services.SaveLoadSettings
{
    public interface ISettings
    {
        // Загрузить настройки
        AppSettings LoadSettings();

        // Сохранить настройки
        void SaveSettings(AppSettings settings);
    }

    public interface ILoadAvailableVoices
    {
        void GetListOfAvailableVoices(ObservableCollection<string> comboBoxAvailableVoices);
    }
    
    public interface ILoadProfilesList
    {
        void GetListOfAvailableProfiles(ObservableCollection<string> comboBoxAvailableProfiles);
    }
}
