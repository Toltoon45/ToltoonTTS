using System.Windows;
using ToltoonTTS.Scripts.IndividualVoices;

namespace ToltoonTTS.Scripts.GoodGame
{
    /// <summary>
    /// Interaction logic for GoodgameIndividualVoiceList.xaml
    /// </summary>
    public partial class GoodgameIndividualVoiceList : Window
    {
        public GoodgameIndividualVoiceList()
        {
            InitializeComponent();
            LoadContainers.LoadJsonFileIndividualVoicesUserList("goodgame", StackPanelUserIndividualVoicesList);
        }

        internal void UpdateUserList()
        {
            StackPanelUserIndividualVoicesList.Children.Clear();
            LoadContainers.LoadJsonFileIndividualVoicesUserList("goodgame", StackPanelUserIndividualVoicesList);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            SaveContainers.JsonIndividualVoicesListClosing(StackPanelUserIndividualVoicesList, "goodgame");
            UpdateVoices.LoadVoicesOnProgramStart(true, "goodgame"); //это конечно не на старте но работает ж))))))))
        }
    }
}
