using Newtonsoft.Json.Linq;
using System.IO;
using ToltoonTTS.Scripts.Twitch;

namespace ToltoonTTS.Scripts.IndividualVoices
{
    internal class UserChangeVoice
    {
        public static void UserChangeName(string userName, string userInput, string platform)
        {
                if (TextToSpeech.availableRandomVoices.Contains(userInput.Trim()))
                {
                string filePath = platform switch
                {
                    "twitch" => $@"DataForProgram/IndividualVoices/TwitchIndividualVoices.json",
                    "goodgame" => $@"DataForProgram/IndividualVoices/GoodgameIndividualVoices.json",
                    _ => throw new ArgumentException("Неподдерживаемая платформа", nameof(platform))
                };
                JArray jsonArray = JArray.Parse(File.ReadAllText(filePath));
                    var matchingItem = jsonArray.FirstOrDefault(item => item["Nickname"].ToString() == userName);

                    if (matchingItem != null)
                    {
                        matchingItem["Voice"] = userInput.Trim();
                    //сначала нужно записать имеющиеся настройки голосов и потом считать ещё раз
                    //иначе когда придёт новый человек, то настройки перейдут к начальным (не будет учитываться смена голоса пользователем)
                    File.WriteAllText(filePath, jsonArray.ToString());
                    JArray jsonArray2 = JArray.Parse(File.ReadAllText(filePath));
                    TextToSpeech.twitchUserVoicesDict = jsonArray2.ToDictionary(
    item => item["Nickname"].ToString(),
    item => item["Voice"].ToString());
                }
            }

        }
    }
}
