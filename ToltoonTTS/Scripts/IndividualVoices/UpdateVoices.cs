using Newtonsoft.Json.Linq;
using System.IO;
using ToltoonTTS.Scripts.GoodGame;

namespace ToltoonTTS.Scripts.IndividualVoices
{
    public class UpdateVoices
    {
        public static JArray LoadVoicesOnProgramStart(bool saveBeforeLoad, string platform)
        {
            string filePath = platform switch
            {
                "twitch" => $@"DataForProgram/IndividualVoices/TwitchIndividualVoices.json",
                "goodgame" => $@"DataForProgram/IndividualVoices/GoodgameIndividualVoices.json",
                _ => throw new ArgumentException("Неподдерживаемая платформа", nameof(platform))
            };
            // Чтение JSON файла
            string jsonContentFixFile = File.ReadAllText(filePath);
            // Парсинг JSON данных
            JArray jsonArrayFixFile = JArray.Parse(jsonContentFixFile);
            // Словарь для хранения последних экземпляров по Nickname
            Dictionary<string, JObject> latestEntries = new Dictionary<string, JObject>();
            // Обработка JSON объектов
            foreach (JObject item in jsonArrayFixFile)
            {
                string nickname = item["Nickname"].ToString();

                // Сохраняем или обновляем последний экземпляр
                latestEntries[nickname] = item;
            }
            // Подготовка результата
            JArray resultArray = new JArray(latestEntries.Values);
            // Запись результата в новый файл
            File.WriteAllText(filePath, resultArray.ToString());
            JArray jsonArray = JArray.Parse(File.ReadAllText(filePath));
            if (platform == "twitch")
            {
                TextToSpeech.twitchUserVoicesDict = jsonArray.ToDictionary(
item => item["Nickname"].ToString(),
item => item["Voice"].ToString());
                return jsonArray;
            }
            if (platform == "goodgame")
            {
                GoodGameConnection.goodgameUserVoicesDict = jsonArray.ToDictionary(
item => item["Nickname"].ToString(),
item => item["Voice"].ToString());
                return jsonArray;
            }
            else
            {
                try
                {
                    JArray jsonArray2 = JArray.Parse(File.ReadAllText($@"DataForProgram/IndividualVoices/TwitchIndividualVoices.json"));
                    TextToSpeech.twitchUserVoicesDict = jsonArray.ToDictionary(
item => item["Nickname"].ToString(),
item => item["Voice"].ToString());
                    return jsonArray;
                }
                catch { }
            }
            return new JArray();
        }
        internal static JArray LoadAndSaveIndividualVoices(bool saveBeforeLoad, JArray JArrayToSave, string platform)
        {
            string filePath = platform switch
            {
                "twitch" => $@"DataForProgram/IndividualVoices/TwitchIndividualVoices.json",
                "goodgame" => $@"DataForProgram/IndividualVoices/GoodgameIndividualVoices.json",
                _ => throw new ArgumentException("Неподдерживаемая платформа", nameof(platform))
            };
            JArray jsonArray;
            if (saveBeforeLoad && JArrayToSave != null)
            {
                // Сохраняем новый JArray в файл
                File.WriteAllText(filePath, JArrayToSave.ToString());
                jsonArray = JArrayToSave; // Используем переданный JArray
            }
            else
            {
                // Читаем файл заново
                string fileContent = File.ReadAllText(filePath);
                jsonArray = JArray.Parse(fileContent);
            }
            if (platform == "twitch")
            {
                foreach (var item in jsonArray)
                {
                    var nickname = item["Nickname"].ToString();
                    var voice = item["Voice"].ToString();

                    if (TextToSpeech.twitchUserVoicesDict.ContainsKey(nickname))
                    {
                        TextToSpeech.twitchUserVoicesDict.Remove(nickname);
                    }
                    TextToSpeech.twitchUserVoicesDict[nickname] = voice;
                }
            }
            if (platform == "goodgame")
            {
                foreach (var item in jsonArray)
                {
                    var nickname = item["Nickname"].ToString();
                    var voice = item["Voice"].ToString();

                    if (GoodGameConnection.goodgameUserVoicesDict.ContainsKey(nickname))
                    {
                        GoodGameConnection.goodgameUserVoicesDict.Remove(nickname);
                    }
                    GoodGameConnection.goodgameUserVoicesDict[nickname] = voice;
                }
            }
            return jsonArray;
        }
    }
}
