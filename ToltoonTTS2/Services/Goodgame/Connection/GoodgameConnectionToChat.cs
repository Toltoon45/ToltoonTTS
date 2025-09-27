
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;

namespace ToltoonTTS2.Services.Goodgame.Connection
{
    public enum GoodgameConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Failed
    }

    public class GoodgameConnectionToChat : IGoodgameConnection
    {
        public GoodgameConnectionState CurrentState { get; private set; }

        public event EventHandler<GoodgameMessageEventArgs> MessageReceived;

        public event EventHandler<GoodgameConnectionState> ConnectionStateChanged;

        protected virtual void OnMessageReceived(GoodgameMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public async Task Connection(string ChannelToConnect)
        {
            UpdateState(GoodgameConnectionState.Connecting);
            string profile = $"https://goodgame.ru/{ChannelToConnect}";
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                try
                {
                    const string url = "wss://chat.goodgame.ru/chat/websocket";
                    Uri uri = new Uri(url);
                    // Подключаемся к серверу WebSocket
                    await webSocket.ConnectAsync(uri, CancellationToken.None);
                    var channelPreload = await GetChannelPreload(profile);
                    // Формируем сообщение в формате JSON
                    var message = new
                    {
                        type = "join",
                        data = new
                        {
                            channel_id = channelPreload
                        }
                    };
                    string jsonMessage = System.Text.Json.JsonSerializer.Serialize(message);
                    byte[] bufferToSend = Encoding.UTF8.GetBytes(jsonMessage);

                    // Отправляем сообщение на сервер
                    await webSocket.SendAsync(new ArraySegment<byte>(bufferToSend), WebSocketMessageType.Text, true, CancellationToken.None);
                    UpdateState(GoodgameConnectionState.Connected);
                    // Бесконечный цикл получения сообщений от сервера
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var bufferToReceive = new byte[4096];
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(bufferToReceive), CancellationToken.None);

                        // Если сервер закрыл соединение
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            UpdateState(GoodgameConnectionState.Disconnected);
                            break;
                        }

                        // Преобразуем полученные данные в строку
                        string receivedMessage = Encoding.UTF8.GetString(bufferToReceive, 0, result.Count);
                        JObject jsonObj = JObject.Parse(receivedMessage);
                        string textUserName2 = jsonObj["data"]?["user_name"]?.ToString();
                        //public static ListBox GoodGameBlackList;
                        //if (GoodGameBlackList!= null && GoodGameBlackList.Items.Contains(textUserName2.ToLower()))
                        //    return;
                        string textValue = jsonObj["data"]?["text"]?.ToString();
                        if (textValue != null)
                        {
                            string textUserName = jsonObj["data"]?["user_name"]?.ToString();
                            OnMessageReceived(new GoodgameMessageEventArgs
                            {
                                UserName = textUserName2,
                                Message = textValue
                            });


                        }

                    }
                }
                catch (Exception ex)
                {
                    UpdateState(GoodgameConnectionState.Failed);
                    Console.WriteLine("Ошибка при подключении или получении данных: " + ex.Message);
                }
            }
        }

        private static async Task<string> GetChannelPreload(string url)
        {
            // Загрузка HTML-контента страницы
            var httpClient = new HttpClient();
            var htmlContent = await httpClient.GetStringAsync(url);

            // Парсинг HTML-документа
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Поиск скрипта с ChannelPreload
            var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'ChannelPreload')]");

            if (scriptNode == null)
                return null;

            // Извлечение содержимого скрипта
            var scriptText = scriptNode.InnerText.Trim();

            // Удаление лишних символов и преобразование в JSON
            var startIndex = scriptText.IndexOf('{');
            var endIndex = scriptText.LastIndexOf('}');
            var jsonContent = scriptText.Substring(startIndex, endIndex - startIndex + 1);

            // Парсинг JSON
            using (var reader = new JsonTextReader(new StringReader(jsonContent)))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == "channel_id")
                    {
                        reader.Read();
                        return reader.Value.ToString();
                    }
                }
            }

            return null;
        }

        private void UpdateState(GoodgameConnectionState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                ConnectionStateChanged?.Invoke(this, newState);
            }
        } 
    }

    public class GoodgameMessageEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}
