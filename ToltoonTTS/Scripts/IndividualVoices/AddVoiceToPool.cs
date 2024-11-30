using DocumentFormat.OpenXml.Bibliography;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;


namespace ToltoonTTS.Scripts.IndividualVoices
{
    public static class AddVoiceToPool
    {
        public static List<string>? InstalledVoices;

        public static StackPanel AddNewVoice(StackPanel stackPanelAddedVoices)
        {
            var newPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            string getRandomVoice = GetRandomFreeVoice(stackPanelAddedVoices);
            if (getRandomVoice == null)
                return null;
            var comboBox = new ComboBox
            {
                Name = $"comboBoxInstalledVoice",
                Width = 100,
                Margin = new Thickness(10, 10, 10, 0),
                ToolTip = "Выбор голоса",
                ItemsSource = InstalledVoices,
                SelectedValue = GetRandomFreeVoice(stackPanelAddedVoices),
            };
            var textBoxVolume = CreateTextBox($"textBoxVoice", "Громкость этого голоса", 50);
            var textBoxSpeed = CreateTextBox($"textBoxSpeed", "Скорость голоса", 3);

            Button buttonDelete = new Button();
            {
                buttonDelete.Name = $"buttonDelete";
                buttonDelete.Content = "Удалить";
                buttonDelete.Width = 75;
                buttonDelete.Margin = new Thickness(10, 10, 10, 0);
                buttonDelete.ToolTip = "Удалить";

                // Добавляем обработчик клика на кнопку удаления
                buttonDelete.Click += (sender, e) =>
                {
                    var currentButton = sender as Button;
                    var parentStackPanel = currentButton.Parent as StackPanel;
                    stackPanelAddedVoices.Children.Remove(parentStackPanel);
                };
            }
            TextBox textBoxYourVoiceName = new TextBox();
            {
                textBoxYourVoiceName.Name = $"textBoxYourVoiceName";
                textBoxYourVoiceName.ToolTip = "Для баллов канала";
                textBoxYourVoiceName.Margin = new Thickness(10, 10, 10, 0);
                textBoxYourVoiceName.Width = 150;
                textBoxYourVoiceName.ToolTip = "В разработке";
            }
            newPanel.Children.Add(comboBox);
            newPanel.Children.Add(textBoxVolume);
            newPanel.Children.Add(textBoxSpeed);
            newPanel.Children.Add(buttonDelete);
            //newPanel.Children.Add(textBoxYourVoiceName);

            return newPanel;
        }

        private static TextBox CreateTextBox(string name, string toolTip, int voiceOrVolume)
        {
            return new TextBox
            {
                Name = name,
                Width = 25,
                Margin = new Thickness(10, 10, 10, 0),
                ToolTip = toolTip,
                Text = Convert.ToString(voiceOrVolume)
            };
        }

        private static string GetRandomFreeVoice(StackPanel stackPanelAddedVoices)
        {
            // Получаем список всех доступных голосов
            SpeechSynthesizer synth = new SpeechSynthesizer();
            var allVoices = synth.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList();

            var usedVoices = new List<string>();

            //StackPanel - матрёшка. В первой StackPanel ещё StackPanel в которых информация о голосах
            foreach (StackPanel horizontalStackPanel in stackPanelAddedVoices.Children.OfType<StackPanel>())
            {
                foreach (ComboBox comboBox in horizontalStackPanel.Children.OfType<ComboBox>())
                {
                    // Получаем выбранный голос и добавляем в список занятых
                    if (comboBox.SelectedItem != null)
                    {
                        string selectedVoice = comboBox.SelectedItem.ToString();
                        usedVoices.Add(selectedVoice);
                    }
                }
            }

            // Получаем список свободных голосов
            var freeVoices = allVoices.Except(usedVoices).ToList();

            // Если есть свободные голоса, выбираем случайный
            if (freeVoices.Any())
            {
                Random random = new Random();
                int randomIndex = random.Next(freeVoices.Count);
                return freeVoices[randomIndex]; // Возвращаем случайный свободный голос
            }
            else
            {
                return null; // Если свободных голосов нет
            }
        }
    }
}
