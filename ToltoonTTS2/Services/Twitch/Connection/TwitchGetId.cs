using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using TwitchLib.Client;
using TwitchLib.PubSub;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;

namespace ToltoonTTS2.Services.Twitch.Connection
{
    class TwitchGetId : ITwitchGetID
    {
        private string _userId;
        //получение ID twitch. нужно для получения всех функций чата
        public async Task<string> GetTwitchUserId(string TwitchApi, string TwitchClientId, string TwitchNickname)
        {
            HttpClient ClientToGetId = new HttpClient();
            ClientToGetId.DefaultRequestHeaders.Clear();
            ClientToGetId.DefaultRequestHeaders.Add("Client-ID", TwitchClientId);
            ClientToGetId.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TwitchApi);

            string url = $"https://api.twitch.tv/helix/users?login={TwitchNickname}";
            HttpResponseMessage response = await ClientToGetId.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string responseBody = await response.Content.ReadAsStringAsync();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
            _userId = data?.data?[0]?.id;
            return _userId;
        }
    }
}
