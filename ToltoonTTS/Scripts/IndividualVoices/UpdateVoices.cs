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
                TextToSpeech.twitchUserVoicesDict = jsonArray.ToDictionary(
                    item => item["Nickname"].ToString(),
                    item => item["Voice"].ToString());
            }
            if (platform == "goodgame")
            {
                GoodGameConnection.goodgameUserVoicesDict = jsonArray.ToDictionary(
item => item["Nickname"].ToString(),
item => item["Voice"].ToString());
                return jsonArray;
            }

            return jsonArray;
        }

    }
}
