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
            LoadContainers.LoadJsonFileIndividualVoicesUserList("twitch", StackPanelUserIndividualVoicesList);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            SaveContainers.JsonIndividualVoicesListClosing(StackPanelUserIndividualVoicesList,"twitch");
            UpdateVoices.LoadVoicesOnProgramStart(true, "twitch");
            StackPanelUserIndividualVoicesList.Children.Clear();
            UpdateUserList();
        }
    }
}
