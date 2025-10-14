using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Windows.Input;
using ToltoonTTS2.Services.BlackList;
using ToltoonTTS2.Services.EnsureFolderAndFileExist;
using ToltoonTTS2.Services.Goodgame.Connection;
using ToltoonTTS2.Services.SaveLoadSettings;
using ToltoonTTS2.Services.TTS;
using ToltoonTTS2.Services.Twitch.Connection;
using ToltoonTTS2.Voices;

namespace ToltoonTTS2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SQLiteConnection _LoadIndividualVoicesSettingsDb; //индивидуальные голоса
        private readonly SQLiteConnection _individualVoicesGoodgameDb; //голоса для людей с гудгейма
        private readonly SQLiteConnection _individualVoicesTwitchDb; //голоса для людей с твича

        ObservableCollection<VoiceItem> _voiceItems;          // для работы с БД
        ObservableCollection<PlaceVoicesInfoInWPF> _uiItems;  // для биндинга к UI

        private readonly string _LoadIndividualVoicesSettingsDbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "IndividualVoices.db");
        private readonly string _individualVoicesTwitchDbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "TwitchIndividualVoices.db");
        private readonly string _individualVoicesGoodgameDbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "GoodgameIndividualVoices.db");
        public ObservableCollection<PlaceVoicesInfoInWPF> ItemSourceAllVoices { get; set; } = new ObservableCollection<PlaceVoicesInfoInWPF>();
        public List<string> DynamicSpeedLabels => new List<string> { "100", "200", "300", "400", "500" };
        public AdditionalSettingsViewModel AdditionalSettings { get; } = new();


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
        private string _labelTtsSpeedValue;
        private string _labelTtsVolumeValue;
        private string _doNotTtsIfStartWith;
        private string _skipMessage;
        private string _skipMessageAll;
        private ObservableCollection<string> _blackList;
        private ObservableCollection<string> _wordToReplace;
        private ObservableCollection<string> _wordToReplaceWith;
        private ObservableCollection<string> _dynamicSpeedMessage;
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
        private TwitchConnectionState _twitchConnectionState;
        private GoodgameConnectionState _goodgameConnectionState;
        private IndividualVoicesWindow IndividualVoicesWin;
        private bool _individualVoicesEnabled;
        private readonly ITwitchGetID _twitchGetId;
        private readonly ITwitchConnectToChat _twitchConnectToChat;
        private readonly IGoodgameConnection _goodgameConnectionToChat;
        private readonly ITts _ttsService;
        private readonly ISettings _serviceSettings;
        private readonly IDirectoryService _directoryService;
        private readonly ILoadAvailableVoices _loadAvailableVoices;
        private readonly ILoadProfilesList _loadProfiles;
        private readonly IBlackListServices _blackListServices;
        private readonly IWordreplace _wordReplaceService;
        private readonly ITtsMessageProcessing _messageProcessing;
        private string _voiceTestErrorMessage;

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
            ITtsMessageProcessing messageProcessing,
            IGoodgameConnection goodgameConnectionToChat,
            AdditionalSettingsViewModel additionalSettings)
        {
            AdditionalSettings = additionalSettings;
            _serviceSettings = settingsService;
            _twitchGetId = twitchGetId;
            _twitchConnectToChat = twitchConnectToChat;
            _goodgameConnectionToChat = goodgameConnectionToChat;
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
            IndividualVoicesWin = new IndividualVoicesWindow();

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
            OpenIndividualVoicesTwitchWindow = new RelayCommand(() => OpenIndividualVoices("Twitch"));
            OpenIndividualVoicesGoodgameWindow = new RelayCommand(() => OpenIndividualVoices("Goodgame"));
            buttonMakeEnabledVoicesLouder = new RelayCommand(() => ChangeEnabledVoicesVolume(1));
            buttonMakeEnabledVoicesQuiet = new RelayCommand(() => ChangeEnabledVoicesVolume(-1));

            DynamicSpeedValues.Clear();

            var raw = Properties.Settings.Default.DynamicSpeed;
            var parts = raw?.Split(',') ?? Array.Empty<string>();

            for (int i = 0; i < 5; i++)
            {
                int value = 0;
                if (i < parts.Length && int.TryParse(parts[i], out var parsed))
                    value = parsed;

                var item = new BindableInt { Value = value }; //молодец

                item.PropertyChanged += (_, __) => SaveToSettingsDynamicTtsSpeed();

                DynamicSpeedValues.Add(item);
                
            }
            var values = DynamicSpeedValues.Select(x => x.Value);
            _ttsService.SetDynamicSpeed(values);




            _directoryService = directoryService;
            _directoryService.EnsureAppStructureExists();

            LoadSettings();

            _LoadIndividualVoicesSettingsDb = new SQLiteConnection(_LoadIndividualVoicesSettingsDbPath);
            _LoadIndividualVoicesSettingsDb.CreateTable<VoiceItem>();

            _individualVoicesTwitchDb = new SQLiteConnection(_individualVoicesTwitchDbPath);  // ← создаём подключение к нужной БД
            _individualVoicesTwitchDb.CreateTable<PlatformsIndividualVoices>();
            _individualVoicesGoodgameDb = new SQLiteConnection(_individualVoicesGoodgameDbPath);
            _individualVoicesGoodgameDb.CreateTable<PlatformsIndividualVoices>();

            LoadInstalledVoices();
            _messageProcessing.SetDatabase(_individualVoicesTwitchDb, _LoadIndividualVoicesSettingsDb, _individualVoicesGoodgameDb);
            _loadProfiles.GetListOfAvailableProfiles(AvailableProfiles);
            _loadAvailableVoices.GetListOfAvailableVoices(AvailableVoices);

            //получение сообщений
            _twitchConnectToChat.MessageReceived += OnTwitchMessageReceived;
            _goodgameConnectionToChat.MessageReceived += (sender, e) =>
            {
                // обработка сообщения
                if ((e.UserName.ToLower() == "toltoon45" || e.UserName == "s1llyc4k3") && e.Message.StartsWith("!разбанить пидораса"))
                {
                    var words = e.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        var lastWord = words[^1];
                        BlackListMembers.Remove(lastWord);
                    }
                }
                var message = _messageProcessing.ProcessIncomingMessage(e.UserName, e.Message, "goodgame");
                if (message != null)
                    _ttsService.Speak(message);
            };
        }

        private void ChangeEnabledVoicesVolume(int v)
        {
            foreach(var item in ItemSourceAllVoices)
            {
                if (item.IsEnabled)
                {
                   int a = Convert.ToInt32(item.TextBoxVolume);
                    int result = a + v;
                   item.TextBoxVolume = Convert.ToString(result); 
                }
                
            }
        }

        private void OpenIndividualVoices(string platform)
        {

            var twitchDb = new SQLiteConnection(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "TwitchIndividualVoices.db"));
            var goodgameDb = new SQLiteConnection(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "GoodgameIndividualVoices.db"));
            var voicesDb = new SQLiteConnection(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "IndividualVoices.db"));

            // Выбор нужной базы в зависимости от платформы
            var selectedDb = platform.Equals("Twitch", StringComparison.OrdinalIgnoreCase)
                ? twitchDb //у тебя всё сделано для твич, надо сделать общим
                : goodgameDb;

            var viewModel = new TwitchVoiceBindingsViewModel(selectedDb, voicesDb);


            var window = new IndividualVoicesWindow
            {
                DataContext = viewModel
            };

            window.ShowDialog();
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

            var result = _messageProcessing.ProcessIncomingMessage(username, message, "twitch");

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                _ttsService.Speak(result);
            }
        }

        public TwitchConnectionState TwitchConnectionState
        {
            get => _twitchConnectionState;
            set
            {
                _twitchConnectionState = value;
                OnPropertyChanged();
                TwitchConnectionStatus = $"Twitch: {value}";
            }
        }

        public GoodgameConnectionState GoodgameConnectionState
        {
            get => _goodgameConnectionState;
            set
            {
                _goodgameConnectionState = value;
                OnPropertyChanged();
                GoodgameConnectionStatus = $"Гудгейм: {value}";
            }
        }

        private async Task ConnectToStreamingChats()
        {
            if (ConnectToTwitch)
            {
                _twitchConnectToChat.ConnectionStateChanged += (s, state) =>
                {
                    TwitchConnectionState = state;
                };

                await _twitchConnectToChat.ConnectToChat(TwitchApi, TwitchNickname);
            }
            if (ConnectToGoodgame)
            {
                _goodgameConnectionToChat.ConnectionStateChanged += (s, state) =>
                {
                    GoodgameConnectionState = state;
                };
                await _goodgameConnectionToChat.Connection(GoodgameNickname);
            }
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
        public ICommand OpenIndividualVoicesTwitchWindow { get; }
        public ICommand OpenIndividualVoicesGoodgameWindow { get; }
        public ICommand buttonMakeEnabledVoicesLouder { get; }
        public ICommand buttonMakeEnabledVoicesQuiet { get; }

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

        public string GoodgameConnectionStatus
        {
            get => _goodgameConnectionStatus;
            set { _goodgameConnectionStatus = value; OnPropertyChanged(); }
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

        public bool IndividualVoicesEnabled
        {
            get => _individualVoicesEnabled;
            set { _individualVoicesEnabled = value; OnPropertyChanged();
                _messageProcessing.SetIndividualVoicesEnabled(value);
            }
            
        }

        public bool ConnectToGoodgame
        {
            get => _connectToGoodgame;
            set { _connectToGoodgame = value; OnPropertyChanged(); }
        }

        public bool RemoveEmoji
        {
            get => _removeEmoji;
            set { _removeEmoji = value; OnPropertyChanged();
                _messageProcessing.SetRemoveEmoji(value);
            }
        }

        public string SelectedVoice
        {
            get => _selectedVoice;
            set { _selectedVoice = value; OnPropertyChanged();
                _ttsService?.SetVoice(value);
                _messageProcessing.SetStandartVoiceName(value);
            }
        }

        public int TtsSpeedValue
        {
            get => _ttsSpeedValue;
            set { _ttsSpeedValue = value; OnPropertyChanged();
                _ttsService?.SetRate(value);
                LabelTtsSpeed = Convert.ToString(value);
                _messageProcessing.SetStandartVoiceSpeed(value);
            }
        }
        
        public string LabelTtsSpeed
        {
            get => _labelTtsSpeedValue;
            set 
            { 
                _labelTtsSpeedValue = value; OnPropertyChanged(); 
            }
        }

        public int TtsVolumeValue
        {
            get => _ttsVolumeValue;
            set
            {
                _ttsVolumeValue = value; OnPropertyChanged();
                _ttsService?.SetVolume(value);
                LabelTtsVolume = Convert.ToString(value);
                _messageProcessing.SetStandardVoiceVolume(value);
            }
        }

        public string LabelTtsVolume
        {
            get => _labelTtsVolumeValue;
            set
            {
                _labelTtsVolumeValue = value; OnPropertyChanged();
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
            set { _doNotTtsIfStartWith = value; OnPropertyChanged();
                _messageProcessing.SetDoNotTtsIfStartWith(value);
            }
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
            set { _skipMessageAll = value; OnPropertyChanged();
                _messageProcessing.SetSkipAllMessages(value);
            }
        }

        public ObservableCollection<string> BlackListMembers
        {
            get => _blackList;
            set { _blackList = value; OnPropertyChanged();
                _messageProcessing.SetBlackList(_blackList);
            }
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

        //public ObservableCollection< ItemSourceAllVoices
        //{
        //    get = _itemSourceAllVoices;
        //    set { _itemSourceAllVoices = value; OnPropertyChanged(); }
        //}

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
            set { _wordToReplaceWith = value; OnPropertyChanged();
                _messageProcessing.WordToReplaceWith(value);
            }
        }

        public ObservableCollection<string> DynamicSpeedMessage
        {
            get => _dynamicSpeedMessage;
            set
            {
                _dynamicSpeedMessage = value; OnPropertyChanged(); //передавать данные для обработчика сообщений
            }
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
        
        public string VoiceTestErrorMessage
        {
            get => _voiceTestErrorMessage;
            set
            {
                _voiceTestErrorMessage = value;
                OnPropertyChanged(nameof(VoiceTestErrorMessage));
            }
        }

        private void DisconnectTwitch()
        {
            // Логика отключения от Twitch
        }

        private void LoadSettings()
        {


            var settings = _serviceSettings.LoadSettings();

            TwitchApi = settings.TwitchApi;
            if (TwitchApi == "")
            {
                TwitchApi = "1"; //без этой строки TwitchApi всегда будет ""
            }

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

            IndividualVoicesEnabled = settings.IndividualVoicesEnabled;
            
            foreach (var item in settings.BlackListMembers)
            {
                BlackListMembers.Add(item);
                _messageProcessing.SetBlackList(BlackListMembers);
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
        private void LoadInstalledVoices()
        {
            // Загружаем из базы
            var voicesInDb = _LoadIndividualVoicesSettingsDb.Table<VoiceItem>().ToList();

            using (var synth = new SpeechSynthesizer())
            {
                foreach (InstalledVoice installedVoice in synth.GetInstalledVoices())
                {
                    var info = installedVoice.VoiceInfo;
                    var existing = voicesInDb.FirstOrDefault(v => v.VoiceName == info.Name);

                    if (existing != null)
                    {
                        // Если голос уже есть в базе — добавляем из базы
                        ItemSourceAllVoices.Add(new PlaceVoicesInfoInWPF
                        {
                            Id = existing.Id,
                            Name = existing.VoiceName,
                            TextBoxVolume = existing.Volume,
                            TextBoxSpeed = existing.Speed,
                            IsEnabled = existing.IsEnabled,
                            Db = _LoadIndividualVoicesSettingsDb,
                            StatusReporter = this
                        });
                    }
                    else
                    {
                        // Если нет — создаём новый, добавляем в БД и UI
                        var newVoice = new VoiceItem
                        {
                            VoiceName = info.Name,
                            Volume = "50",
                            Speed = "0",
                            IsEnabled = false
                        };
                        _LoadIndividualVoicesSettingsDb.Insert(newVoice);

                        ItemSourceAllVoices.Add(new PlaceVoicesInfoInWPF
                        {
                            Id = newVoice.Id,
                            Name = newVoice.VoiceName,
                            TextBoxVolume = newVoice.Volume,
                            TextBoxSpeed = newVoice.Speed,
                            IsEnabled = newVoice.IsEnabled,
                            Db = _LoadIndividualVoicesSettingsDb
                        });
                    }
                }
            }
        }

        public ObservableCollection<BindableInt> DynamicSpeedValues { get; } = new()
{
    new BindableInt { Value = 0 },
    new BindableInt { Value = 0 },
    new BindableInt { Value = 0 },
    new BindableInt { Value = 0 },
    new BindableInt { Value = 0 }
};

        private void SaveToSettingsDynamicTtsSpeed()
        {
            var values = DynamicSpeedValues.Select(x => x.Value);
            _ttsService.SetDynamicSpeed(values);
            var joined = string.Join(",", values);
            Properties.Settings.Default.DynamicSpeed = joined;
            Properties.Settings.Default.Save();
        }


        //сохранение при закрытии программы
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
                WhatToReplaceWith = WordWhatToReplaceWith.ToList(),
                IndividualVoicesEnabled = IndividualVoicesEnabled,

            };
            foreach (var uiItem in ItemSourceAllVoices)
            {
                var dbItem = new VoiceItem
                {
                    Id = uiItem.Id,
                    VoiceName = uiItem.Name,
                    Volume = uiItem.TextBoxVolume,
                    Speed = uiItem.TextBoxSpeed,
                    IsEnabled = uiItem.IsEnabled
                };

                _LoadIndividualVoicesSettingsDb.Update(dbItem);
            }
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
                _messageProcessing.SetBlackList(BlackListMembers);
            }
        }

        private void DeleteFromBlackList()
        {
            _blackListServices.RemoveFromBlackList(BlackListSelectedItem);
            _messageProcessing.SetBlackList(BlackListMembers);
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