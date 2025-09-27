using System.Windows;
using ToltoonTTS2.Services.BlackList;
using ToltoonTTS2.Services.SaveLoadSettings;
using ToltoonTTS2.Services.BlackList;
using ToltoonTTS2.Services.EnsureFolderAndFileExist;
using ToltoonTTS2.Services.Goodgame.Connection;
using ToltoonTTS2.Services.SaveLoadSettings;
using ToltoonTTS2.Services.TTS;
using ToltoonTTS2.Services.Twitch.Connection;
using ToltoonTTS2.Services.TTS;
using ToltoonTTS2.Services.Twitch.Connection;
using ToltoonTTS2.ViewModels;

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