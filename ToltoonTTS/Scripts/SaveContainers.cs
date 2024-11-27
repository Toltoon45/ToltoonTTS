using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ToltoonTTS.Scripts
{
    public static class SaveContainers
    {
        public static string? JsonSaveTwitchApi;
        public static string? JsonSaveTwitchId;
        public static string? JsonSaveTwitchNick;
        public static bool JsonSaveRemoveEmoji;
        public static string? JsonSaveInstalledVoicesSelect;
        public static int? JsonSaveTtsVolumeValue;
        public static int? JsonSaveTtsSpeedValue;
        public static bool? JsonIndividualVoices;
        public static string? JsonDoNotTtsIfStartWith;
        public static string? JsonMessageSkip;
        public static string? JsonMessageSkipAll;
        public static string? JsonTextBoxGoodgameLogin;
        public static string? JsonTextBoxGoodgamePassword;
        public static bool? JsonCheckBoxConnectToGoodgame;
        private const string DefaultJsonContent = "[]";

        private static readonly string ProfilesDirectory = "DataForProgram/Profiles";

        // Сохранение данных профилей в JSON файл
        public static void SaveJsonFileMainWindow(string fileName, ComboBox comboBoxProfileSelect)
        {
            var jsonFileData = new
            {
                twitchApi = JsonSaveTwitchApi,
                twitchId = JsonSaveTwitchId,
                twitchNick = JsonSaveTwitchNick,
                removeEmoji = JsonSaveRemoveEmoji,
                installedVoicesSelect = JsonSaveInstalledVoicesSelect,
                ttsVolumeValue = JsonSaveTtsVolumeValue,
                ttsSpeedValue = JsonSaveTtsSpeedValue,
                ttsIndividualVoices = JsonIndividualVoices,
                DoNotTtsIfStartWith = JsonDoNotTtsIfStartWith,
                MessageSkip = JsonMessageSkip,
                MessageSkipAll = JsonMessageSkipAll,
                goodgameLogin = JsonTextBoxGoodgameLogin,
                goodgamePassword = JsonTextBoxGoodgamePassword,
                checkboxConnectToGoodgame = JsonCheckBoxConnectToGoodgame
            };

            string filePath = Path.Combine(ProfilesDirectory, $"{fileName}.json");

            try
            {
                string jsonData = JsonSerializer.Serialize(jsonFileData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);

                if (!comboBoxProfileSelect.Items.Contains($"{fileName}.json"))
                {
                    comboBoxProfileSelect.Items.Add($"{fileName}.json");
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки или отображение сообщения в интерфейсе
            }
        }

        // Создание необходимых файлов и папок
        public static void CreateFiles()
        {
            var directories = new[]
            {
                "DataForProgram/BlackList",
                "DataForProgram/Profiles",
                "DataForProgram/WordReplace",
                "DataForProgram/Anecdots",
                "DataForProgram/IndividualVoices"
            };
            var files = new Dictionary<string, string[]>
            {
                { "DataForProgram/IndividualVoices", new[] { "IndividualVoices.json", "TwitchIndividualVoices.json", "GoodgameIndividualVoices.json" } },
                { "DataForProgram/WordReplace", new[] { "WhatToReplace.json", "WhatToReplaceWith.json" } }
            };

            foreach (var dir in directories)
            {
                Directory.CreateDirectory(dir);
            }

            foreach (var (dir, fileNames) in files)
            {
                foreach (var fileName in fileNames)
                {
                    string filePath = Path.Combine(dir, fileName);
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(filePath, DefaultJsonContent); // Создаем стандартный JSON-файл, если его нет
                    }

                }
            }
        }

        // Сохранение данных индивидуальных голосов в JSON файл
        public static void SaveJsonFileIndividualVoices(StackPanel parentPanel)
        {
            var stackPanelsData = new List<StackPanelData>();

            foreach (StackPanel stackPanel in parentPanel.Children)
            {
                var data = new StackPanelData();

                foreach (var element in stackPanel.Children)
                {
                    if (element is ComboBox comboBox)
                    {
                        data.ComboBoxValue = comboBox.SelectedItem?.ToString();
                    }
                    else if (element is TextBox textBox)
                    {
                        if (textBox.Name == "textBoxVoice")
                            data.TextBoxVoice = textBox.Text;
                        else if (textBox.Name == "textBoxSpeed")
                            data.TextBoxSpeed = textBox.Text;
                    }
                }
                stackPanelsData.Add(data);
            }

            string filePath = "DataForProgram/IndividualVoices/IndividualVoices.json";
            var jsonData = JsonSerializer.Serialize(stackPanelsData, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonData);
            //TextToSpeech.voiceFullInfo = JArray.Parse(File.ReadAllText(filePath));
            
        }

        // Сохранение данных для текстовых модификаторов
        public static void SaveTextModifier(ListBox listBoxBlackList, ListBox listBoxWhatToReplace, ListBox listBoxWhatToReplaceWith)
        {
            SaveListBoxToJson(listBoxBlackList, "DataForProgram/BlackList/BlackListUsers.json");
            SaveListBoxToJson(listBoxWhatToReplace, "DataForProgram/WordReplace/WhatToReplace.json");
            SaveListBoxToJson(listBoxWhatToReplaceWith, "DataForProgram/WordReplace/WhatToReplaceWith.json");
        }

        // Вспомогательный метод для сериализации ListBox в JSON файл
        private static void SaveListBoxToJson(ListBox listBox, string filePath)
        {
            var items = new List<string>();

            foreach (var item in listBox.Items)
            {
                items.Add(item?.ToString() ?? string.Empty);
            }

            string jsonData = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonData);
        }

        public static void DeleteProfile(ComboBox comboBoxProfileSelect)
        {
            MessageBoxResult result = MessageBox.Show("Sure", "Some Title", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.Delete($@"DataForProgram/Profiles//{comboBoxProfileSelect.Text}");
                    comboBoxProfileSelect.Items.Remove(comboBoxProfileSelect.SelectedItem);
                }
                catch
                {
                }
            }
        }

        public static void JsonIndividualVoicesListClosing(StackPanel stackPanelUserIndividualVoicesList, string platform)
        {
            // Создаем массив для хранения объектов JObject
            JArray jsonArray = new JArray();

            // Проходим по каждому элементу StackPanel
            foreach (StackPanel horizontalStackPanel in stackPanelUserIndividualVoicesList.Children.OfType<StackPanel>())
            {
                // Находим первый элемент (Label) и извлекаем значение Nickname
                Label nicknameLabel = horizontalStackPanel.Children.OfType<Label>().First();
                string nickname = nicknameLabel.Content.ToString();

                // Находим второй элемент (ComboBox) и извлекаем значение Voice
                ComboBox voiceComboBox = horizontalStackPanel.Children.OfType<ComboBox>().First();
                string voice = voiceComboBox.SelectedItem?.ToString() ?? "";


                // Создаем JObject с полями Nickname и Voice
                JObject item = new JObject(
                    new JProperty("Nickname", nickname),
                    new JProperty("Voice", voice)
                );

                // Добавляем объект в массив
                jsonArray.Add(item);
            }

            // Определяем путь к файлу в зависимости от платформы
            string filePath = platform switch
            {
                "twitch" => $@"DataForProgram/IndividualVoices/TwitchIndividualVoices.json",
                "goodgame" => $@"DataForProgram/IndividualVoices/GoodgameIndividualVoices.json",
                _ => throw new ArgumentException("Неподдерживаемая платформа", nameof(platform))
            };

            // Записываем JSON в файл
            File.WriteAllText(filePath, jsonArray.ToString());
        }

        public class StackPanelData
        {
            [JsonPropertyName("ComboBoxValue")]
            public string? ComboBoxValue { get; set; }

            [JsonPropertyName("TextBoxVoice")]
            public string? TextBoxVoice { get; set; }

            [JsonPropertyName("TextBoxSpeed")]
            public string? TextBoxSpeed { get; set; }
        }
    }
}
