﻿using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using ToltoonTTS.Scripts.IndividualVoices;
using System.Security.Policy;
using Newtonsoft.Json;
using System.IO;

namespace ToltoonTTS.Scripts.GoodGame
{
    public static class GoodGameConnection
    {
        static string profile = $"https://goodgame.ru/{SaveContainers.JsonTextBoxGoodgameLogin}";

        public static Dictionary<string, string> goodgameUserVoicesDict;
        private static JArray newJsonForSaveGoodgame = UpdateVoices.LoadVoicesOnProgramStart(false, "goodgame");
        public static HashSet<string> goodgameNicknameSet = new HashSet<string>(newJsonForSaveGoodgame.Select(item => item["Nickname"]?.ToString()).Where(nick => nick != null));
        public static async Task GoodGameConnect(string login)
        {
            string ggid;


            JArray individualVoicesGoodgame = UpdateVoices.LoadVoicesOnProgramStart(false, "goodgame");
            // Убедитесь, что URL правильный
            string url = "wss://chat.goodgame.ru/chat/websocket";
            Uri uri = new Uri(url);

            while (true)
            {
                // Запуск обработки WebSocket в фоновом потоке
                Task websocketTask = Task.Run(async () => await ConnectAndReceiveMessages(uri));

                // Ожидание завершения задачи или возникновения ошибки
                await websocketTask;

            }
        }

        static async Task ConnectAndReceiveMessages(Uri uri)
        {
            //false - нам не нужно сохраняться перед загрузкой файлов
            var channelPreload = await GetChannelPreload(profile);
            JArray IndividualVoicesGoodGame = UpdateVoices.LoadVoicesOnProgramStart(false, "goodgame");
            Random random = new Random();
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                try
                {
                    // Подключаемся к серверу WebSocket
                    await webSocket.ConnectAsync(uri, CancellationToken.None);

                    // Формируем сообщение в формате JSON
                    var message = new
                    {
                        //data = new
                        //{
                        //    login = "toltoon45",
                        //    password = SaveContainers.JsonTextBoxGoodgamePassword
                        //}
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

                    // Бесконечный цикл получения сообщений от сервера
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var bufferToReceive = new byte[1024];
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(bufferToReceive), CancellationToken.None);

                        // Если сервер закрыл соединение
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("Сервер закрыл соединение.");
                            break;
                        }

                        // Преобразуем полученные данные в строку
                        string receivedMessage = Encoding.UTF8.GetString(bufferToReceive, 0, result.Count);
                        JObject jsonObj = JObject.Parse(receivedMessage);
                        string textValue = jsonObj["data"]?["text"]?.ToString();
                        if (textValue != null)
                        {
                            string textUserName = jsonObj["data"]?["user_name"]?.ToString();
                            if (!goodgameNicknameSet.Contains(textUserName.ToLower()))
                            {
                                JObject newUser = new JObject
                    {
                        { "Nickname", textUserName.ToLower() },
                        { "Voice", TextToSpeech.availableRandomVoices[random.Next(TextToSpeech.availableRandomVoices.Count)]}
                    };
                                //добавить нового человека, сохранить список и сразу его прочитать чтобы не было ошибок со сменой голоса пользователем
                                newJsonForSaveGoodgame.Add(newUser);
                                newJsonForSaveGoodgame = UpdateVoices.LoadAndSaveIndividualVoices(true, newJsonForSaveGoodgame, "goodgame");
                                goodgameNicknameSet.Add(textUserName.ToLower());
                                //goodgameUserVoicesDict.Add(newUser);
                                TextToSpeech.Play(textValue, textUserName.ToLower(), "goodgame");
                                //return;
                            }
                            else if (textValue == TextToSpeech.MessageSkip || textValue == TextToSpeech.MessageSkipAll)
                            {
                                TextToSpeech.CancelMessages(textValue);
                                //если использовать return то будет выход из цикла всё сломается лопнет взорвётся сдуется скомается уничтожится пропадёт исчезнет 
                                //return;
                            }
                            else
                                TextToSpeech.Play(textValue, textUserName.ToLower(), "goodgame");

                        }
                        // Можно выполнить какие-либо действия с textValue
                        //Console.WriteLine("Получено сообщение: " + textValue);
                    }

                    // Закрытие соединения после завершения работы
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // Обработка ошибок подключения
                    Console.WriteLine("Ошибка при подключении или получении данных: " + ex.Message);
                }
            }
        }

        //чтение и запоминание индивидуальных голосов
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

    }
}