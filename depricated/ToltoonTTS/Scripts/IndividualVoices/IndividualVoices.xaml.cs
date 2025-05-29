using DocumentFormat.OpenXml.Bibliography;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using ToltoonTTS.Scripts;
using ToltoonTTS.Scripts.GoodGame;
using ToltoonTTS.Scripts.IndividualVoices;
using ToltoonTTS.Scripts.Twitch;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace ToltoonTTS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class IndividualVoices : Window
    {
        TwitchIndividualVoicesList twitchIndividualVoicesList;
        GoodgameIndividualVoiceList goodgameIndividualVoiceList;
        List<string> SourceToStopDoubleIndividualVoices = new List<string>();
        SpeechSynthesizer Synth = new SpeechSynthesizer();
        public IndividualVoices()
        {
            InitializeComponent();
            LoadContainers.LoadJsonFileIndividualVoices(StackPanelAddedVoices);
            this.IsVisibleChanged += OnVisibleChanged;
            UpdateSourceForInstalledVoicesCombobox();
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIndividualVoicePoolTextForCopy();
        }

        private void ButtonAddNewVoiceToPool_Click(object sender, RoutedEventArgs e)
        {
            //добавить слот голоса
            // Добавить слот голоса, если AddNewVoice возвращает не null
            var newVoiceSlot = AddVoiceToPool.AddNewVoice(StackPanelAddedVoices);

            if (newVoiceSlot != null)
            {
                StackPanelAddedVoices.Children.Add(newVoiceSlot);
                SaveAllVoices();
                UpdateIndividualVoicePoolTextForCopy();
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
            SaveAllVoices();

        }

        private void SaveAllVoices()
        {
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
            UpdateIndividualVoicePoolTextForCopy();
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

        private void IndividualVoicesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateIndividualVoicePoolTextForCopy();
            foreach (StackPanel firstChild in StackPanelAddedVoices.Children)
            {
                foreach(var item in firstChild.Children)
                {
                    string asd = item.GetType().Name;
                    if (item is Button button)
                    {
                        if (button.Name == "buttonDelete")
                        {
                            button.Click += (sender, e) => UpdateIndividualVoicePoolTextForCopy();
                        }
                    }
                    else if (item is ComboBox comboBox)
                    {
                        //Чтобы элемент числился в combobox он уже должен существовать. Добавляем его а потом убираем
                        SourceToStopDoubleIndividualVoices.Add(comboBox.SelectedItem as string);
                        //Установка ItemSource сбрасывает значение, его надо сохранить
                        string saveName = comboBox.Text;
                        int a = comboBox.Items.Count;
                        comboBox.ItemsSource = SourceToStopDoubleIndividualVoices;
                        if (comboBox.Items.Contains(saveName))
                        {
                            comboBox.SelectedItem = saveName;
                        }
                        SourceToStopDoubleIndividualVoices.Remove(saveName);

                        comboBox.DropDownClosed += (sender, e) =>
                        {
                            ComboBox cb = sender as ComboBox;
                            comboDeletePreviewStringClick(cb);
                            UpdateIndividualVoicePoolTextForCopy();
                        };
                        comboBox.PreviewMouseLeftButtonDown += (sender, e) =>
                        {//ComboBox при открытии кеширует данные. Открыть комбобокс -> обновить source -> комбобокс НЕ ОБНОВИТ source. Надо делать вручную
                            var abc = comboBox.SelectedItem;
                            var bbb = comboBox.Text;
                            comboBox.Items.Refresh();
                            SourceToStopDoubleIndividualVoices.Add(bbb);
                            comboBox.SelectedItem = bbb;
                            var previousSelectedItem = e.Source as ComboBox;
                            comboBoxMouseClick(previousSelectedItem);
                            //UpdateIndividualVoicePoolTextForCopy();
                        };
                        comboBox.SelectionChanged += (sender, e) =>
                        {
                            var selectedComboBox = sender as ComboBox; // Приводим sender к типу ComboBox

                            // Получаем новый выбранный элемент (первый элемент в AddedItems)
                            var newSelectedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;

                            // Получаем предыдущий выбранный элемент (первый элемент в RemovedItems)
                            var previousSelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;

                            // Передаем комбобокс, новый и предыдущий элементы в метод
                            UpdateAvailableVoices(selectedComboBox, newSelectedItem, previousSelectedItem);
                            UpdateSourceForInstalledVoicesCombobox();
                        };
                        comboBox.SelectionChanged += (sender, e) => UpdateIndividualVoicePoolTextForCopy();
                    }
                }

            }
                twitchIndividualVoicesList = new TwitchIndividualVoicesList();
                goodgameIndividualVoiceList = new GoodgameIndividualVoiceList();
        }

        private void comboDeletePreviewStringClick(ComboBox cb)
        {
            if (SourceToStopDoubleIndividualVoices.Contains(cb.Text))
                SourceToStopDoubleIndividualVoices.Remove(cb.Text);
        }

        private void comboBoxMouseClick(ComboBox combobox)
        {
            if (!SourceToStopDoubleIndividualVoices.Contains(combobox.Text))
                SourceToStopDoubleIndividualVoices.Add(combobox.Text);
        }

        private void UpdateAvailableVoices(ComboBox combobox, object newSelectedItem, object previousSelectedItem)
        {
            foreach (StackPanel children in StackPanelAddedVoices.Children)
            {
                // Находим ComboBox внутри StackPanel
                ComboBox comboBox = children.Children.OfType<ComboBox>().FirstOrDefault();

                if (comboBox != null)
                {

                    TextBoxIndividualVoicesForCopy.Text += comboBox.Text + Environment.NewLine;
                }
            }
        }

        private void UpdateIndividualVoicePoolTextForCopy()
        {
            TextBoxIndividualVoicesForCopy.Clear();
            SourceToStopDoubleIndividualVoices.Clear();
            foreach (InstalledVoice voice in Synth.GetInstalledVoices())
            {
                SourceToStopDoubleIndividualVoices.Add(voice.VoiceInfo.Name);
            }

            foreach (StackPanel stackPanel in StackPanelAddedVoices.Children)
            {
                foreach (var item in stackPanel.Children)
                {
                    if (item is ComboBox comboBox)
                    {
                        SourceToStopDoubleIndividualVoices.Remove(comboBox.Text);
                        TextBoxIndividualVoicesForCopy.Text += comboBox.Text + Environment.NewLine;
                    }
                }
            }
        }

        private void UpdateSourceForInstalledVoicesCombobox()
        {
            SourceToStopDoubleIndividualVoices.Clear();
            TextBoxIndividualVoicesForCopy.Clear();
            foreach (InstalledVoice voice in Synth.GetInstalledVoices())
            {
                SourceToStopDoubleIndividualVoices.Add(voice.VoiceInfo.Name);
            }
            foreach (StackPanel firstChild in StackPanelAddedVoices.Children)
            {
                foreach (var item in firstChild.Children)
                {
                    if (item is ComboBox comboBox)
                    {
                        SourceToStopDoubleIndividualVoices.Remove(comboBox.Text);

                        //Чтобы элемент числился в combobox он уже должен существовать. Добавляем его а потом убираем
                        SourceToStopDoubleIndividualVoices.Add(comboBox.SelectedItem as string);
                        //Установка ItemSource сбрасывает значение, его надо сохранить
                        string saveName = comboBox.Text;
                        int a = comboBox.Items.Count;
                        comboBox.ItemsSource = SourceToStopDoubleIndividualVoices;
                        if (comboBox.Items.Contains(saveName))
                        {
                            comboBox.SelectedItem = saveName;
                        }
                        SourceToStopDoubleIndividualVoices.Remove(saveName);
                    }
                }
            }
        }
    }
}