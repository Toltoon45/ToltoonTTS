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

            var comboBox = new ComboBox
            {
                Name = $"comboBoxInstalledVoice",
                Width = 100,
                Margin = new Thickness(10, 10, 10, 0),
                ToolTip = "Выбор голоса",
                ItemsSource = InstalledVoices
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
    }
}
