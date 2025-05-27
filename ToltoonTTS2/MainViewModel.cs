using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToltoonTTS2.Scripts.BlackList;
using ToltoonTTS2.Scripts.EnsureFolderAndFileExist;
using ToltoonTTS2.Scripts.SaveLoadSettings;
using ToltoonTTS2.TTS;
using ToltoonTTS2.Twitch.Connection;

namespace ToltoonTTS2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Приватные поля
        private string _twitchApi;
        private string _twitchClientId;
        private string _twitchNickname;
        private bool _connectToTwitch;
        private string _goodgamenickname;
        private bool _connectToGoodgame;
        private bool _removeEmoji;
        private string _selectedVoice;
        private int _ttsSpeedValue;
        private int _ttsVolumeValue;
        private string _doNotTtsIfStartWith;
        private string _skipMessage;
        private string _skipMessageAll;
        private ObservableCollection<string> _blackList;
        private ObservableCollection<string> _wordToReplace;
        private ObservableCollection<string> _wordToReplaceWith;
        private string _wordToReplaceInput;
        private string _wordReplaceToWith;
        private string _blackListInput;
        private string _blackListSelectedItem;
        private string _profileSelected;
        private int _wordReplaceSelectedIndex;
        private ObservableCollection<string> _availableVoices;
        private ObservableCollection<string> _profiles;
        private string _twitchConnectionStatus;
        private string _goodgameConnectionStatus;
        private string _nameToSaveProfile;
        private string _nameToLoadProfile;
        private ObservableCollection<string> _allProfiles;


        private readonly ITwitchGetID _twitchGetId;
        private readonly ITwitchConnectToChat _twitchConnectToChat;
        private readonly ITts _ttsService;
        private readonly ISettings _serviceSettings;
        private readonly IDirectoryService _directoryService;
        private readonly ILoadAvailableVoices _loadAvailableVoices;
        private readonly ILoadProfilesList _loadProfiles;
        private readonly IBlackListServices _blackListServices;
        private readonly IWordreplace _wordReplaceService;
        private readonly ITtsMessageProcessing _messageProcessing;

        public MainViewModel
            (ITwitchGetID twitchGetId, 
            ITwitchConnectToChat twitchConnectToChat, 
            ITts ttsService, 
            ISettings settingsService,
            IDirectoryService directoryService,
            ILoadAvailableVoices getInstalledVoices,
            ILoadProfilesList loadProfiles,
            IBlackListServices blackListSerives,
            IWordreplace wordReplaceService,
            ITtsMessageProcessing messageProcessing)
        {
            _serviceSettings = settingsService;
            _twitchGetId = twitchGetId;
            _twitchConnectToChat = twitchConnectToChat;
            _ttsService = ttsService;
            _loadAvailableVoices = getInstalledVoices;
            _loadProfiles = loadProfiles;
            _blackListServices = blackListSerives;
            _wordReplaceService = wordReplaceService;
            _messageProcessing = messageProcessing;

            BlackListMembers = _blackListServices.BlackList;
            WordToReplace = _wordReplaceService.WordToReplace;
            WordWhatToReplaceWith = _wordReplaceService.WordWhatToReplaceWith;
            AvailableVoices = new ObservableCollection<string>();
            AvailableProfiles = new ObservableCollection<string>();

            DisconnectTwitchCommand = new RelayCommand(DisconnectTwitch);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            AddToBlackListCommand = new RelayCommand(AddToBlackList);
            DeleteFromBlackListCommand = new RelayCommand(DeleteFromBlackList);
            ConnectToChats = new RelayCommand(async () => await ConnectToStreamingChats());
            SaveProfile = new RelayCommand(() => SaveLoadProfiles.SaveProfile(this));
            DeleteProfile = new RelayCommand(() => SaveLoadProfiles.DeleteProfile(this));
            LoadProfile = new RelayCommand(() => SaveLoadProfiles.LoadProfile(this));
            OpenDataDirectory = new RelayCommand(OpenProgramData);
            AddWordReplace = new RelayCommand(AddWordToReplace);
            DeleteWordReplace = new RelayCommand(DeleteWordFromReplace);
            _directoryService = directoryService;
            _directoryService.EnsureAppStructureExists();

            LoadSettings();
            _loadProfiles.GetListOfAvailableProfiles(AvailableProfiles);
            _loadAvailableVoices.GetListOfAvailableVoices(AvailableVoices);

            _twitchConnectToChat.MessageReceived += OnTwitchMessageReceived;
        }

        private void DeleteWordFromReplace()
        {
            _wordReplaceService.DeleteWordFromReplace(_wordReplaceSelectedIndex);
        }

        private void OpenProgramData()
        {
            Process.Start("explorer.exe", "DataForProgram");
        }

        private void OnTwitchMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            string message = e.ChatMessage.Message;
            string username = e.ChatMessage.Username;

            if (BlackListMembers.Any(badWord => username.Contains(badWord, StringComparison.OrdinalIgnoreCase)))
                return;
                _ttsService.Speak($"{message}");
        }


        private async Task ConnectToStreamingChats()
        {
            if (ConnectToTwitch)
            {
                //твич выключил pubsub. поулчать id - пока что не для чего
                //string userId = await _twitchGetId.GetTwitchUserId(TwitchApi, TwitchClientId, TwitchNickname);
                    //await _twitchConnectToChat.ConnectToChat(TwitchApi, TwitchClientId, TwitchNickname);
                    await _twitchConnectToChat.ConnectToChat(TwitchApi, TwitchNickname);
                TwitchConnectionStatus = "Twitch подключился";
            }

            // Аналогично для GoodGame (если нужно)
        }

        // Команды
        public ICommand ConnectionToTwitchCommand { get; }
        public ICommand DisconnectTwitchCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand AddToBlackListCommand { get; }
        public ICommand DeleteFromBlackListCommand { get; set; }
        public ICommand ConnectToChats { get; set; }
        public ICommand OpenDataDirectory { get; }
        public ICommand SaveProfile { get; }
        public ICommand LoadProfile { get; }
        public ICommand AddWordReplace { get; }
        public ICommand DeleteWordReplace { get; }
        public ICommand DeleteProfile { get; set; }

        // Свойства
        public string TwitchApi
        {
            get => _twitchApi;
            set { _twitchApi = value; OnPropertyChanged(); }
        }

        public string TwitchClientId
        {
            get => _twitchClientId;
            set { _twitchClientId = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableProfiles
        {
            get => _allProfiles;
            set { _allProfiles = value; OnPropertyChanged(); }
        }

        public string TwitchNickname
        {
            get => _twitchNickname;
            set { _twitchNickname = value; OnPropertyChanged(); }
        }

        public string TwitchConnectionStatus
        {
            get => _twitchConnectionStatus;
            set { _twitchConnectionStatus = value; OnPropertyChanged(); }
        }

        public bool ConnectToTwitch
        {
            get => _connectToTwitch;
            set { _connectToTwitch = value; OnPropertyChanged(); }
        }

        public string GoodgameNickname
        {
            get => _goodgamenickname;
            set { _goodgamenickname = value; OnPropertyChanged(); }
        }

        public bool ConnectToGoodgame
        {
            get => _connectToGoodgame;
            set { _connectToGoodgame = value; OnPropertyChanged(); }
        }

        public bool RemoveEmoji
        {
            get => _removeEmoji;
            set { _removeEmoji = value; OnPropertyChanged(); }
        }

        public string SelectedVoice
        {
            get => _selectedVoice;
            set { _selectedVoice = value; OnPropertyChanged();
                _ttsService?.SetVoice(value);
            }
        }

        public int TtsSpeedValue
        {
            get => _ttsSpeedValue;
            set { _ttsSpeedValue = value; OnPropertyChanged();
                _ttsService?.SetRate(value);
            }
        }

        public int TtsVolumeValue
        {
            get => _ttsVolumeValue;
            set
            {
                _ttsVolumeValue = value; OnPropertyChanged();
                _ttsService?.SetVolume(value);
            }
        }

        public string NameToSaveProfile
        {
            get => _nameToSaveProfile;
            set
            {
                _nameToSaveProfile = value; OnPropertyChanged();
            }
        }

        public string DoNotTtsIfStartWith
        {
            get => _doNotTtsIfStartWith;
            set { _doNotTtsIfStartWith = value; OnPropertyChanged(); }
        }

        public string SkipMessage
        {
            get => _skipMessage;
            set { _skipMessage = value; OnPropertyChanged();
                _messageProcessing.SetSkipMessage(value);
                }
        }

        public string SkipMessageAll
        {
            get => _skipMessageAll;
            set { _skipMessageAll = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> BlackListMembers
        {
            get => _blackList;
            set { _blackList = value; OnPropertyChanged(); }
        }

        public string BlackListInput
        {
            get => _blackListInput;
            set { _blackListInput = value; OnPropertyChanged(); }
        }
        
        public string SelectedProfile
        {
            get => _profileSelected;
            set { _profileSelected = value; OnPropertyChanged(); }
        }

        public string BlackListSelectedItem
        {
            get => _blackListSelectedItem;
            set { _blackListSelectedItem = value; OnPropertyChanged(); }
        }

        public int WordReplaceSelectedIndex
        {
            get => _wordReplaceSelectedIndex;
            set { _wordReplaceSelectedIndex = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> WordToReplace
        {
            get => _wordToReplace;
            set { _wordToReplace = value; OnPropertyChanged();
                _messageProcessing.WordToReplace(value);
            }
        }

        public ObservableCollection<string> WordWhatToReplaceWith
        {
            get => _wordToReplaceWith;
            set { _wordToReplaceWith = value; OnPropertyChanged(); }
        }

        public string WordToReplaceInput
        {
            get => _wordToReplaceInput;
            set { _wordToReplaceInput = value; OnPropertyChanged(); }
        }

        public string WordReplaceToWithInput
        {
            get => _wordReplaceToWith;
            set { _wordReplaceToWith = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableVoices
        {
            get => _availableVoices;
            set { _availableVoices = value; OnPropertyChanged(); }
        }

        private void DisconnectTwitch()
        {
            // Логика отключения от Twitch
        }

        private void LoadSettings()
        {
            var settings = _serviceSettings.LoadSettings();
            
            TwitchApi = settings.TwitchApi;
            TwitchClientId = settings.TwitchClientId;
            TwitchNickname = settings.TwitchNickname;
            ConnectToTwitch = settings.ConnectToTwitch;
            GoodgameNickname = settings.GoodgameNickname;
            ConnectToGoodgame = settings.ConnectToGoodgame;
            RemoveEmoji = settings.RemoveEmoji;
            SelectedVoice = settings.SelectedVoice;
            TtsSpeedValue = settings.TtsSpeedValue;
            TtsVolumeValue = settings.TtsVolumeValue;
            DoNotTtsIfStartWith = settings.DoNotTtsIfStartWith;
            SkipMessage = settings.SkipMessage;
            SkipMessageAll = settings.SkipMessageAll;
            foreach(var item in settings.BlackListMembers)
            {
                BlackListMembers.Add(item);
            }
            foreach (var item in settings.WhatToReplace)
            {
                WordToReplace.Add(item);
            }
            foreach (var item in settings.WhatToReplaceWith)
            {
                _wordToReplaceWith.Add(item);
            }
        }

        public void SaveSettings()
        {
            var settings = new AppSettings
            {
                TwitchApi = TwitchApi,
                TwitchClientId = TwitchClientId,
                TwitchNickname = TwitchNickname,
                ConnectToTwitch = ConnectToTwitch,
                GoodgameNickname = GoodgameNickname,
                ConnectToGoodgame = ConnectToGoodgame,
                RemoveEmoji = RemoveEmoji,
                SelectedVoice = SelectedVoice,
                TtsSpeedValue = TtsSpeedValue,
                TtsVolumeValue = TtsVolumeValue,
                DoNotTtsIfStartWith = DoNotTtsIfStartWith,
                SkipMessage = SkipMessage,
                SkipMessageAll = SkipMessageAll,
                BlackListMembers = BlackListMembers.ToList(),
                WhatToReplace = WordToReplace.ToList(),
                WhatToReplaceWith = WordWhatToReplaceWith.ToList()
            };
            _serviceSettings.SaveSettings(settings);
        }

        private void AddWordToReplace()
        {
            _wordReplaceService.AddToWordReplace(WordToReplaceInput,WordReplaceToWithInput);
        }

        private void AddToBlackList()
        {
            if (!string.IsNullOrWhiteSpace(BlackListInput))
            {
                _blackListServices.AddToBlackList(BlackListInput);
            }
        }

        private void DeleteFromBlackList()
        {
            _blackListServices.RemoveFromBlackList(BlackListSelectedItem);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private object v;

        public RelayCommand(Action execute) => _execute = execute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
    }
}