using System.Windows;
using System.Windows.Controls;
using ToltoonTTS.Scripts;
using ToltoonTTS.Scripts.GoodGame;
using ToltoonTTS.Scripts.IndividualVoices;
using ToltoonTTS.Scripts.Twitch;

namespace ToltoonTTS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class IndividualVoices : Window
    {
        TwitchIndividualVoicesList twitchIndividualVoicesList;
        GoodgameIndividualVoiceList goodgameIndividualVoiceList;
        public IndividualVoices()
        {
            InitializeComponent();
            LoadContainers.LoadJsonFileIndividualVoices(StackPanelAddedVoices);
            twitchIndividualVoicesList = new TwitchIndividualVoicesList();
            goodgameIndividualVoiceList = new GoodgameIndividualVoiceList();
        }

        private void ButtonAddNewVoiceToPool_Click(object sender, RoutedEventArgs e)
        {
            //добавить слот голоса
            // Добавить слот голоса, если AddNewVoice возвращает не null
            var newVoiceSlot = AddVoiceToPool.AddNewVoice(StackPanelAddedVoices);

            if (newVoiceSlot != null)
            {
                StackPanelAddedVoices.Children.Add(newVoiceSlot);
            }
            else
            {
                // Выполнить альтернативный код
                MessageBox.Show("Все голоса заняты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void IndividualVoicesWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            IndividualVoicesWindow.Hide();
            SaveContainers.SaveJsonFileIndividualVoices(StackPanelAddedVoices);
            //раньше было var availableVoices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var availableVoices = new List<string>();
            var availableVoicesChangedNames = new List<string>();
            //перебор всех СтакПанелей и перебор всех КомбоБокс внутри СтакПанелей
            foreach (var childStackPanel in StackPanelAddedVoices.Children.OfType<StackPanel>())
            {
                foreach (var comboBox in childStackPanel.Children.OfType<ComboBox>())
                {
                    if (!string.IsNullOrEmpty(comboBox.Text) && !availableVoices.Contains(comboBox.Text))
                        availableVoices.Add(comboBox.Text);
                }
                foreach (var textBox in childStackPanel.Children.OfType<TextBox>())
                {
                    if (textBox.Name.StartsWith("textBoxYourVoiceName"))
                        availableVoicesChangedNames.Add(textBox.Text);
                }
            }

            //Обновление списка доступных голосов
            TextToSpeech.availableRandomVoices = availableVoices;

            //Обновление списка изменённых голосов
            TextToSpeech.individualVoicesRenamed = availableVoicesChangedNames;
        }

        public static void DeleteVoiceStackPanel(int serialNumber)
        {

        }

        private void ButtonIndividualVoicesResetToPreviousSettings_Click(object sender, RoutedEventArgs e)
        {
            StackPanelAddedVoices.Children.Clear();
            LoadContainers.LoadJsonFileIndividualVoices(StackPanelAddedVoices);
        }

        private void ButtonIndividualVoicesShowTwitchVoicesList_Click(object sender, RoutedEventArgs e)
        {
            twitchIndividualVoicesList.UpdateUserList();
            twitchIndividualVoicesList.Show();
        }

        private void ButtonIndividualVoicesShowGoodgameVoicesList_Click(object sender, RoutedEventArgs e)
        {
            goodgameIndividualVoiceList.UpdateUserList();
            goodgameIndividualVoiceList.Show();
        }
    }
}