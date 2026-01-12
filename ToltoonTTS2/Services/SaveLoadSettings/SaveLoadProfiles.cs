using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ToltoonTTS2.ViewModels;

namespace ToltoonTTS2.Services.SaveLoadSettings
{
    public static class SaveLoadProfiles
    {
        private static readonly string ProfilesDirectory = "DataForProgram/Profiles";
        public static void SaveProfile(MainViewModel viewModel)
        {
            Directory.CreateDirectory(ProfilesDirectory);
            var jsonFileData = new
            {
                viewModel.TwitchApi,
                viewModel.TwitchClientId,
                viewModel.TwitchNickname,
                viewModel.ConnectToTwitch,
                viewModel.SelectedVoice,
                viewModel.TtsSpeedValue,
                viewModel.TtsVolumeValue,
                viewModel.GoodgameNickname,
                viewModel.IndividualVoicesEnabled,
                viewModel.TtsForChannelPoints,
                viewModel.NameOfRewardTtsForChannelPoints

            };
            string fileName = viewModel.NameToSaveProfile;
            string filePath = Path.Combine(ProfilesDirectory, $"{fileName}.json");

            try
            {
                string jsonData = JsonSerializer.Serialize(jsonFileData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);

                if (!viewModel.AvailableProfiles.Contains($"{fileName}.json"))
                {
                    viewModel.AvailableProfiles.Add($"{fileName}.json");
                }
            }
            catch { }
        }
        public static void LoadProfile(MainViewModel viewModel)
        {

            string filePath = Path.Combine(ProfilesDirectory, $"{viewModel.SelectedProfile}");
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                var loadedData = JsonSerializer.Deserialize<ProfileData>(jsonData);
                viewModel.TwitchApi = loadedData.TwitchApi;
                viewModel.TwitchClientId = loadedData.TwitchClientId;
                viewModel.TwitchNickname = loadedData.TwitchNickname;
                viewModel.GoodgameNickname = loadedData.GoodgameNickname;
                viewModel.ConnectToTwitch = loadedData.ConnectToTwitch;
                viewModel.SelectedVoice = loadedData.SelectedVoice;
                viewModel.TtsSpeedValue = loadedData.TtsSpeedValue;
                viewModel.TtsVolumeValue = loadedData.TtsVolumeValue;
                viewModel.IndividualVoicesEnabled = loadedData.IndividualVoicesEnabled;
                viewModel.TtsForChannelPoints = loadedData.TtsForChannelPoints;
                viewModel.NameOfRewardTtsForChannelPoints = loadedData.NameOfRewardTtsForChannelPoints;
            }
        }

        internal static void DeleteProfile(MainViewModel mainViewModel)
        {
            File.Delete($@"DataForProgram/Profiles//{mainViewModel.SelectedProfile}");
            mainViewModel.AvailableProfiles.Remove(mainViewModel.SelectedProfile);
        }
    }
    public class ProfileData
    {
        public string TwitchApi { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchNickname { get; set; }
        public bool ConnectToTwitch { get; set; }
        public string SelectedVoice { get; set; }
        public int TtsSpeedValue { get; set; }
        public int TtsVolumeValue { get; set; }
        public string GoodgameNickname { get; set; }
        public List<string> BlackList { get; set; }
        public bool IndividualVoicesEnabled { get; set; }
        public bool TtsForChannelPoints { get; set; }
        public string NameOfRewardTtsForChannelPoints { get; set; } 
    }
    //для сохранения ускорения сообщения в зависимости от длины
    public class BindableInt : INotifyPropertyChanged
    {
        private int _value;

        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }


}
