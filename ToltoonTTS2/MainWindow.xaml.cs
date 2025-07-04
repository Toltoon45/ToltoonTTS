using System.Windows;
using ToltoonTTS2.Goodgame.Connection;
using ToltoonTTS2.Scripts.BlackList;
using ToltoonTTS2.Scripts.EnsureFolderAndFileExist;
using ToltoonTTS2.Scripts.SaveLoadSettings;
using ToltoonTTS2.TTS;
using ToltoonTTS2.Twitch.Connection;

namespace ToltoonTTS2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ITwitchGetID twitchGetId = new TwitchGetId();
            ITwitchConnectToChat twitchConnectToChat = new TwitchConnectToChat();
            IGoodgameConnection goodgameConnectionToChat = new GoodgameConnectionToChat();
            ITts TtsService = new TtsSAPI();
            ISettings SettingsService = new SettingsService();
            IDirectoryService DirectoryService = new DirectoryService();
            ILoadAvailableVoices LoadAvailableVoices = new GetInstalledVoice();
            ILoadProfilesList LoadProfiles = new GetAvailableProfiles();
            IBlackListServices BlackListService = new BlackListServices();
            IWordreplace WordReplaceService = new WordReplace();
            ITtsMessageProcessing MessageProcessing = new TtsMessageProcessing();
            DataContext = new MainViewModel(twitchGetId, 
                twitchConnectToChat, 
                TtsService, SettingsService, 
                DirectoryService, LoadAvailableVoices,
                LoadProfiles,
                BlackListService,
                WordReplaceService,
                MessageProcessing,
                goodgameConnectionToChat);
            //при первом изменении itemsource он становится пустым. Нужна заглушка чтобы этого не было
            //BlackListService.AddToBlackList("1");
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SaveSettings();
            }
            Application.Current.Shutdown();
        }

        private void ListBoxWhatToReplace_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {

        }

        private void PasswordboxTwitchApi_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}