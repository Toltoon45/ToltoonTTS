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
        
        public TwitchIndividualVoicesList()
        {
            InitializeComponent();
            UpdateUserList();
        }
        
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
    }
}
