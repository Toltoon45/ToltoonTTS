using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToltoonTTS.Scripts.GoodGame
{
    internal class NewUserNameAddVoice
    {
        public static void GoodgameNewUserAddVoice(string userName)
        {
            Random random = new Random();
            JObject newUser = new JObject
                    {
                        { "Nickname", userName},
                        { "Voice", TextToSpeech.availableRandomVoices[random.Next(TextToSpeech.availableRandomVoices.Count)]}
                    };
        }
    }
}
