using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using ToltoonTTS2.Scripts.BlackList;
using ToltoonTTS2.Scripts.Database;
using ToltoonTTS2.Scripts.EnsureFolderAndFileExist;
using ToltoonTTS2.Scripts.SaveLoadSettings;
using ToltoonTTS2.TTS;
using ToltoonTTS2.Twitch.Connection;
using SQLite;
using System.IO;

namespace ToltoonTTS2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SQLiteConnection _db;
        ObservableCollection<VoiceItem> _voiceItems;          // для работы с БД
        ObservableCollection<PlaceVoicesInfoInWPF> _uiItems;  // для биндинга к UI
        private readonly string _dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\Voices\", "IndividualVoices.db");
        public ObservableCollection<PlaceVoicesInfoInWPF> ItemSourceAllVoices { get; set; } = new ObservableCollection<PlaceVoicesInfoInWPF>();

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

            _db = new SQLiteConnection(_dbPath);
            _db.CreateTable<VoiceItem>();
            LoadInstalledVoices();

            _loadProfiles.GetListOfAvailableProfiles(AvailableProfiles);
            _loadAvailableVoices.GetListOfAvailableVoices(AvailableVoices);

            _twitchConnectToChat.MessageReceived += OnTwitchMessageReceived;
        }
        // Метод преобразования VoiceItem -> PlaceVoicesInfoInWPF
        private PlaceVoicesInfoInWPF ToPlaceVoicesInfo(VoiceItem item) => new PlaceVoicesInfoInWPF
        {
            Name = item.VoiceName,
            TextBoxVolume = item.Volume,
            TextBoxSpeed = item.Speed
        };

        // Метод преобразования PlaceVoicesInfoInWPF -> VoiceItem
        private VoiceItem ToVoiceItem(PlaceVoicesInfoInWPF place, VoiceItem original)
        {
            original.Volume = place.TextBoxVolume;
            original.Speed = place.TextBoxSpeed;
            return original;
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
            string ProcessedMessage = _messageProcessing.ProcessIncomingMessage(username, message);
            if (!string.IsNullOrEmpty(ProcessedMessage))
                _ttsService.Speak(ProcessedMessage);
        }


        //private async Task ConnectToStreamingChats()
        //{
        //    if (ConnectToTwitch)
        //    {
        //        //твич выключил pubsub. поулчать id - пока что не для чего
        //        //string userId = await _twitchGetId.GetTwitchUserId(TwitchApi, TwitchClientId, TwitchNickname);
        //        //await _twitchConnectToChat.ConnectToChat(TwitchApi, TwitchClientId, TwitchNickname);
        //        await _twitchConnectToChat.ConnectToChat(TwitchApi, TwitchNickname);
        //        TwitchConnectionStatus = "Twitch подключился";
        //    }
        //    //GoodGame
        //}

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
            set { _removeEmoji = value; OnPropertyChanged();
                _messageProcessing.SetRemoveEmoji(value);
            }
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
                LabelTtsSpeed = Convert.ToString(value);
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
        private void LoadInstalledVoices()
        {
            // Загружаем из базы
            var voicesInDb = _db.Table<VoiceItem>().ToList();

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
                            IsEnabled = existing.IsEnabled
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
                        _db.Insert(newVoice);

                        ItemSourceAllVoices.Add(new PlaceVoicesInfoInWPF
                        {
                            Id = newVoice.Id,
                            Name = newVoice.VoiceName,
                            TextBoxVolume = newVoice.Volume,
                            TextBoxSpeed = newVoice.Speed,
                            IsEnabled = newVoice.IsEnabled
                        });
                    }
                }
            }
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
                WhatToReplaceWith = WordWhatToReplaceWith.ToList()
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

                _db.Update(dbItem);
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