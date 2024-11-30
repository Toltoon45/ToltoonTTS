using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ToltoonTTS.Scripts.IndividualVoices;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace ToltoonTTS.Scripts
{
    public class LoadContainers
    {

        public static void ReadJsonProfiles(ComboBox comboBoxProfileSelect)
        {
            string profilesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DataForProgram\\Profiles");
            string[] jsonFiles = Directory.GetFiles(profilesDirectory, "*.json");
            foreach (string filePath in jsonFiles)
            {
                string fileName = Path.GetFileName(filePath);
                comboBoxProfileSelect.Items.Add(fileName);
            }
        }
        //загрузка профилей
        public static Newtonsoft.Json.Linq.JObject? LoadProfile(string ProfileName)
        {
            if (File.Exists($@"DataForProgram/Profiles//{ProfileName}"))
            {
                JObject jsonFile = JObject.Parse(File.ReadAllText($@"DataForProgram/Profiles//{ProfileName}"));
                return jsonFile;
            }
            return null;  
        }

        public static void LoadJsonFileIndividualVoices(StackPanel stackPanel)
        {
            try
            {
                // Загрузка индивидуальных голосов из файла
                JArray jsonArray = JArray.Parse(File.ReadAllText($@"DataForProgram/IndividualVoices/IndividualVoices.json"));
                TextToSpeech.voiceFullInfo = jsonArray;
                int elementIndex = 0;

                foreach (var item in jsonArray)
                {
                    StackPanel newPanel = new StackPanel();
                    {
                        newPanel.Orientation = Orientation.Horizontal;
                    }
                    ComboBox comboBox = new ComboBox();
                    {
                        comboBox.Name = $"comboBoxInstalledVoice";
                        comboBox.Width = 100;
                        comboBox.Margin = new Thickness(10, 10, 10, 0);
                        comboBox.ToolTip = "Выбор голоса";
                        comboBox.ItemsSource = AddVoiceToPool.InstalledVoices;
                        comboBox.Text = Convert.ToString(item["ComboBoxValue"]);
                        if (!TextToSpeech.availableRandomVoices.Contains(comboBox.Text))
                            TextToSpeech.availableRandomVoices.Add(comboBox.Text);
                    }
                    TextBox textBoxVolume = new TextBox();
                    {
                        textBoxVolume.Name = $"textBoxVoice";
                        textBoxVolume.Width = 25;
                        textBoxVolume.Margin = new Thickness(10, 10, 10, 0);
                        textBoxVolume.ToolTip = "Громкость этого голоса";
                        textBoxVolume.Text = Convert.ToString(item["TextBoxVoice"]);
                    }

                    TextBox textBoxSpeed = new TextBox();
                    {
                        textBoxSpeed.Name = $"textBoxSpeed";
                        textBoxSpeed.Width = 25;
                        textBoxSpeed.Margin = new Thickness(10, 10, 10, 0);
                        textBoxSpeed.ToolTip = "Скорость голоса";
                        textBoxSpeed.Text = Convert.ToString(item["TextBoxSpeed"]);
                    }
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
                            stackPanel.Children.Remove(parentStackPanel);
                        };
                    }
                    TextBox textBoxYourVoiceName = new TextBox();
                    {
                        textBoxYourVoiceName.Name = $"textBoxYourVoiceName";
                        textBoxYourVoiceName.ToolTip = "Для баллов канала";
                        textBoxYourVoiceName.Margin = new Thickness(10, 10, 10, 0);
                        textBoxYourVoiceName.Width = 150;
                    }

                    newPanel.Children.Add(comboBox);
                    newPanel.Children.Add(textBoxVolume);
                    newPanel.Children.Add(textBoxSpeed);
                    newPanel.Children.Add(buttonDelete);
                    //newPanel.Children.Add(textBoxYourVoiceName);
                    stackPanel.Children.Add(newPanel);

                    elementIndex++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static ListBox LoadBlackListUser(ListBox listBox, string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    // Считываем содержимое файла
                    string jsonData = File.ReadAllText(filePath);
                    JArray jsonArray = JArray.Parse(jsonData);

                    // Добавляем элементы из JSON в оригинальный ListBox
                    foreach (var obj in jsonArray)
                    {
                        listBox.Items.Add(obj.ToString());
                    }
                }
                catch { } //добавить в лейб сообщение об ошибке

            }
            return listBox;
        }

        public static void LoadJsonFileIndividualVoicesUserList(string platform, StackPanel stackPanel)
        {
            try
            {
                string filePath = platform switch
                {
                    "twitch" => $@"DataForProgram/IndividualVoices/TwitchIndividualVoices.json",
                    "goodgame" => $@"DataForProgram/IndividualVoices/GoodgameIndividualVoices.json",
                    _ => throw new ArgumentException("Неподдерживаемая платформа", nameof(platform))
                };
                JArray jsonArray = JArray.Parse(File.ReadAllText(filePath));
                var sortedItems = jsonArray.OrderBy(item => (string)item["Nickname"]).ToArray();
                foreach (JObject item in sortedItems)
                {
                    StackPanel newStackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    string nickname = (string)item["Nickname"];
                    string voice = (string)item["Voice"];

                    Label nicknameLabel = new Label
                    {
                        Content = nickname,
                        Margin = new System.Windows.Thickness(10),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Width = 100
                    };
                    ComboBox voiceCombobox = new ComboBox
                    {
                        ItemsSource = TextToSpeech.availableRandomVoices,
                        SelectedItem = voice,
                        Height = 25,
                    };
                    Button buttonDelete = new Button();
                    {
                        buttonDelete.Content = "Удалить";
                        buttonDelete.Height = 25;
                        buttonDelete.Margin = new Thickness(10, 0, 0, 0);


                        buttonDelete.Click += (sender, e) =>
                        {
                            var currentButton = sender as Button;
                            var parentStackPanel = currentButton.Parent as StackPanel;
                            stackPanel.Children.Remove(parentStackPanel);
                        };
                    };
                    newStackPanel.Children.Add(nicknameLabel);
                    newStackPanel.Children.Add(voiceCombobox);
                    newStackPanel.Children.Add((Button)buttonDelete);
                    stackPanel.Children.Add(newStackPanel);
                }
            }
            catch { }

            
        }
    }
}
