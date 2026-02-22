using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using ToltoonTTS2.Services.Goodgame.Connection;

namespace ToltoonTTS2.Services.VK
{
    public enum VkConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Failed
    }
    public class VkMessageEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }

    internal class VkConnection : IVkConnection
    {
        public VkConnectionState CurrentState { get; private set; }

        public event EventHandler<VkMessageEventArgs> MessageReceived;

        public event EventHandler<VkConnectionState> ConnectionStateChanged;

        protected virtual void OnMessageReceived(VkMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }
        
        public async Task Connection(string channelNick, string VkSecretApi, string VkAppId)
        {
            UpdateState(VkConnectionState.Connecting);
            string scope = "";
            string redirectUri = "http://127.0.0.1:8553/da/";

            //update
            var waitForCodeTask = WaitForCodeAsync();

            string authUrl =
                $"https://auth.live.vkvideo.ru/app/oauth2/authorize" +
                $"?client_id={VkAppId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&response_type=code" +
                $"&scope={Uri.EscapeDataString(scope)}";

            Process.Start(new ProcessStartInfo(authUrl)
            {
                UseShellExecute = true
            });
            string code = await waitForCodeTask;
            string accessToken = await GetAccessTokenAsync(VkAppId, VkSecretApi, code, redirectUri);
            var channelInfo = await GetChannelInfo(channelNick, accessToken);
            string chatChannel = channelInfo["data"]?["channel"]?["web_socket_channels"]?["chat"]?.ToString();
            string wsToken = await GetWebSocketJwtAsync(accessToken);
            var (clientIdWs, ws) = await ConnectToWebsocket(wsToken);
            await Subscribe(ws, chatChannel);
            await Listen(ws);
        }

        private void UpdateState(VkConnectionState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                ConnectionStateChanged?.Invoke(this, newState);
            }
        }

        private async Task Subscribe(ClientWebSocket ws, string channel)
        {
            var subObj = new
            {
                id = 2,
                subscribe = new
                {
                    channel = channel
                }
            };

            string json = JsonConvert.SerializeObject(subObj);

            await ws.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            UpdateState(VkConnectionState.Connected);
        }

        // Подключение к Centrifugo и отправка connect
        private static async Task<(string Response, ClientWebSocket Socket)> ConnectToWebsocket(string websocketToken)
        {
            const string websocketUrl =
                "wss://pubsub-dev.live.vkvideo.ru/connection/websocket?format=json&cf_protocol_version=v2";

            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(websocketUrl), CancellationToken.None);

            var connectObj = new
            {
                id = 1,
                connect = new
                {
                    token = websocketToken
                }
            };

            string jsonToSend = JsonConvert.SerializeObject(connectObj);

            await ws.SendAsync(
                Encoding.UTF8.GetBytes(jsonToSend),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None
            );

            var buffer = new byte[8192];
            var sb = new StringBuilder();

            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        //поставить изменение лейбла статистики
                    }

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                } while (!result.EndOfMessage);

                string fullMessage = sb.ToString();
                sb.Clear();

                // Centrifugo может прислать несколько JSON-объектов через \n
                var messages = fullMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var msg in messages)
                {
                    var json = JObject.Parse(msg);

                    // Ответ на connect
                    if (json["connect"] != null)
                    {
                        string clientId = json["connect"]?["client"]?.ToString() ?? "";
                        return (clientId, ws);
                    }
                }
            }

            return (null, ws);
        }

        // Слушаем все входящие сообщения
        private async Task Listen(ClientWebSocket ws)
        {
            var buffer = new byte[8192];
            var sb = new StringBuilder();

            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        //для состояния
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None);
                        return;
                    }


                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                } while (!result.EndOfMessage);

                string fullMessage = sb.ToString();
                sb.Clear();

                var messages = fullMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var msg in messages)
                {
                    JObject json;

                    try
                    {
                        json = JObject.Parse(msg);
                    }
                    catch
                    {
                        continue;
                    }

                    //ping
                    Console.WriteLine("RAW: " + msg);
                    // PING пустой JSON {}
                    if (!json.Properties().Any())
                    {
                        var pong = new { pong = new { } };
                        string pongJson = JsonConvert.SerializeObject(pong);

                        await ws.SendAsync(
                            Encoding.UTF8.GetBytes(pongJson),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                        continue;
                    }


                    //чат
                    try
                    {
                        var type = json["push"]?["pub"]?["data"]?["type"]?.ToString();

                        if (type == "channel_chat_message_send")
                        {
                            var chat = json["push"]?["pub"]?["data"]?["data"]?["chat_message"];

                            if (chat != null)
                            {
                                var parts = chat["parts"];
                                if (parts != null)
                                {
                                    string messageText = string.Concat(
                                        parts.Select(p => p["text"]?["content"]?.ToString())
                                    );
                                    string messageNick = chat["author"]?["nick"]?.ToString();

                                    OnMessageReceived(new VkMessageEventArgs
                                    {
                                        UserName = messageNick,
                                        Message = messageText.Trim()
                                    });


                                }

                                string author = chat["author"]?["nick"]?.ToString();
                                Console.WriteLine($"Автор: {author}");
                            }

                            continue;
                        }

                        Console.WriteLine("Служебное сообщение RAW: " + msg);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка парсинга: " + ex.Message);
                    }

                }

            }
        }

        // Получение информации о канале
        static async Task<JObject> GetChannelInfo(string channelUrl, string accessToken)
        {
            string url = $"https://apidev.live.vkvideo.ru/v1/channel?channel_url={Uri.EscapeDataString(channelUrl)}";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            Console.WriteLine("Channel info:");
            Console.WriteLine(data.ToString());
            return data;
        }

        // Получение WebSocket JWT
        static async Task<string> GetWebSocketJwtAsync(string userAccessToken)
        {
            string url = "https://apidev.live.vkvideo.ru/v1/websocket/token";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userAccessToken);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("WS token response: " + json);

            var data = JObject.Parse(json);

            string wsToken = data["data"]?["token"]?.ToString();

            return wsToken;
        }

        // Обмен кода на access_token
        static async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string code, string redirectUri)
        {
            string tokenUrl = "https://api.live.vkvideo.ru/oauth/server/token";

            using var client = new HttpClient();

            string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", basicAuth);

            var postData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            };

            var content = new FormUrlEncodedContent(postData);

            HttpResponseMessage response = await client.PostAsync(tokenUrl, content);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var tokenData = JObject.Parse(json);

            string accessToken = tokenData["access_token"]?.ToString()
                                 ?? throw new Exception("Не удалось получить access_token");

            Console.WriteLine($"Access Token: {accessToken}");

            return accessToken;
        }

        // Локальный HTTP-сервер для приёма кода
        static async Task<string> WaitForCodeAsync()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8553/da/");
            listener.Start();

            try
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                string? code = request.QueryString["code"];

                string responseHtml = @"
<html>
<body>
    <h3>YA VSE POLUCHIL</h3>
    <p>VK SKORO PODKLUCHITSYA, MOZHESH ZAKRIVAT/p>
</body>
</html>";

                byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();

                return code ?? throw new Exception("Код не получен в query string");
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
