using System.Windows;
using ToltoonTTS2.Services.BlackList;
using ToltoonTTS2.Services.SaveLoadSettings;
using ToltoonTTS2.Services.EnsureFolderAndFileExist;
using ToltoonTTS2.Services.Goodgame.Connection;
using ToltoonTTS2.Services.TTS;
using ToltoonTTS2.Services.Twitch.Connection;
using ToltoonTTS2.ViewModels;
using ToltoonTTS2.Services.Youtube;

namespace ToltoonTTS2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            var additionalSettings = new AdditionalSettingsViewModel();
 
            ITwitchConnectToChat twitchConnectToChat = new TwitchConnectToChat(additionalSettings);
            ITwitchGetID twitchGetId = new TwitchGetId();
            IGoodgameConnection goodgameConnectionToChat = new GoodgameConnectionToChat();
            IYoutubeConnection youtubeConnectionToChat = new YoutubeConnection();
            ITts TtsService = new TtsSAPI();
            ISettings SettingsService = new SettingsService();
            IDirectoryService DirectoryService = new DirectoryService();
            ILoadAvailableVoices LoadAvailableVoices = new GetInstalledVoice();
            ILoadProfilesList LoadProfiles = new GetAvailableProfiles();
            IBlackListServices BlackListService = new BlackListServices();
            IWordreplace WordReplaceService = new WordReplace();
            ITtsMessageProcessing MessageProcessing = new TtsMessageProcessing();

            InitializeComponent();
            DataContext = new MainViewModel(twitchGetId,
                twitchConnectToChat,
                TtsService, SettingsService,
                DirectoryService, LoadAvailableVoices,
                LoadProfiles,
                BlackListService,
                WordReplaceService,
                MessageProcessing,
                goodgameConnectionToChat,
                youtubeConnectionToChat,
                additionalSettings);

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

        private void TabControlMainWindow_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}