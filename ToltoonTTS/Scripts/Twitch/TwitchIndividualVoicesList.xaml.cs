using DocumentFormat.OpenXml.Bibliography;
using System.Windows;
using System.Windows.Controls;
using ToltoonTTS.Scripts.IndividualVoices;

namespace ToltoonTTS.Scripts.Twitch
{
    /// <summary>
    /// Interaction logic for TwitchIndividualVoicesList.xaml
    /// </summary>
    public partial class TwitchIndividualVoicesList : Window
    {
        public static StackPanel NewStackPanelForNewUser { get; set; }
        public TwitchIndividualVoicesList()
        {
            InitializeComponent();
            UpdateUserList();
            ref StackPanel NewStackPanelForNewUser = ref StackPanelUserIndividualVoicesList;
        }
        //обновить список всех пользователей в TwitchIndividualVoices
        public void UpdateUserList()
        {
            StackPanelUserIndividualVoicesList.Children.Clear();
            LoadContainers.LoadJsonFileIndividualVoicesUserList("twitch", StackPanelUserIndividualVoicesList);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            TwitchConnection.UpdateTwitchNicknameSet();
            this.Visibility = Visibility.Hidden;
            StackPanelUserIndividualVoicesList.Children.Clear();
            if (StackPanelUserIndividualVoicesList.Children.Count > 0)
            {
                SaveContainers.JsonIndividualVoicesListClosing(StackPanelUserIndividualVoicesList, "twitch");
                UpdateVoices.LoadVoicesOnProgramStart(true, "twitch");
            }
            UpdateUserList();
        }
        //я хочу добавлять новые элементы, когда пишет новый пользователь, но пока что не могу придумать как
        //public  void TwitchAddNewUserToStackPanel()
        //{
        //    StackPanel newUserStackPanel = new StackPanel();
        //    {
        //        newUserStackPanel.Orientation = Orientation.Horizontal;
        //    }
        //    Label nicknameLabel = new Label
        //    {
        //        Content = TwitchConnection.twitchUserMessageUserName,
        //        Margin = new System.Windows.Thickness(10),
        //        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
        //        Width = 100
        //    };
        //    ComboBox voiceCombobox = new ComboBox();
        //    {
        //        voiceCombobox.ItemsSource = TextToSpeech.availableRandomVoices;
        //        voiceCombobox.SelectedItem = TwitchConnection.twitchUserMessageInput;
        //        voiceCombobox.Height = 25;
        //        voiceCombobox.SelectionChanged += (sender, e) =>
        //        {
        //            //if (platform == "twitch")
        //            //{

        //            //}
        //        };
        //    };
        //    newUserStackPanel.Children.Add(nicknameLabel);
        //    newUserStackPanel.Children.Add(voiceCombobox);
        //    StackPanelUserIndividualVoicesList.Children.Add(newUserStackPanel);
        //}
    }
}
