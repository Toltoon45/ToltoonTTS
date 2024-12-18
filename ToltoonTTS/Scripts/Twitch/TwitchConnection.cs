using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using System.Windows.Media;
using ToltoonTTS.Scripts.IndividualVoices;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace ToltoonTTS.Scripts.Twitch
{

    public class TwitchConnection
    {
        TwitchClient TClient = new TwitchClient();
        TwitchPubSub TPubSub = new TwitchPubSub();
        ConnectionCredentials Creds;
        ConnectionCredentials creds2;

        JArray individualVoicesTwitch = UpdateVoices.LoadVoicesOnProgramStart(false, "twitch");
        string? randomVoices;
        Random random = new Random();
        public static Label LabelTwitchStatusMessage = new Label();
        private static readonly object _lock = new object();
        private static TwitchConnection _instance;

        public static ListBox TwitchBlackList;

        public static string twitchApi;
        public static string twitchId;
        public static string twitchNick;

        public static string? individualVoices;
        private static JArray newJsonForSaveTwitch = UpdateVoices.LoadVoicesOnProgramStart(false, "twitch");
        private static string FinalSave;

        public static string twitchUserMessageUserName;
        public static string twitchUserMessageInput;

        string TwitchUserId;
        string TwitchUserApi;

        public static string ChangeVoiceChannelPointsRewardName;
        public static string GetUserVoiceCommand;
        //ников будет много, чтобы не пробегаться по каждому
        public static HashSet<string> twitchNicknameSet = new HashSet<string>(newJsonForSaveTwitch.Select(item => item["Nickname"]?.ToString()).Where(nick => nick != null));

        int ReconnectTryNumber = 1;
        private TwitchConnection()
        {
            UpdateLabelConnectionStatus("Контора грузится...", System.Windows.Media.Colors.Black);
            GetTwitchUserIdAsync();
        }
        public static TwitchConnection Instance
        { //синглтон для твича
            get
            {
                lock (_lock)
                {
                    return _instance ??= new TwitchConnection();
                }
            }
        }

        private async Task<string> GetTwitchUserIdAsync()

        {
            string ErrorMessageConnection = "";
            using (HttpClient twitchToken = new HttpClient())
            {//получение уникального номера пользователя (ID) для подключения pubSub
                twitchToken.DefaultRequestHeaders.Add("Client-ID", twitchId);
                twitchToken.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", twitchApi);
                string url = $"https://api.twitch.tv/helix/users?login={twitchNick}";
                HttpResponseMessage response = await twitchToken.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                if (responseBody == null || string.IsNullOrEmpty(responseBody))
                {
                    UpdateLabelConnectionStatus("Проверь API токен", System.Windows.Media.Colors.Red);
                    _instance = null;
                    return null;
                }

                // Проверка на ошибку 401
                if (data.error != null && data.error.ToString() == "Unauthorized")
                {
                    UpdateLabelConnectionStatus("Неверный OAuth токен", System.Windows.Media.Colors.Red);
                    _instance = null;
                    return null;
                }

                string userId = data.data != null && data.data.Count > 0 ? data.data[0].id : null;

                // Если userId не найден, выведите ошибку
                if (string.IsNullOrEmpty(userId))
                {
                    UpdateLabelConnectionStatus("Ошибка получения данных пользователя", System.Windows.Media.Colors.Red);
                    _instance = null;
                    return null;
                }

                //чтобы программа не фризила при подключении
                await Task.Run(() =>
                {
                    ConnectToTwitch(twitchApi, userId, twitchNick);
                });

                TwitchUserId = userId;
                TwitchUserApi = twitchApi;
                return userId;
            }
        }

        private async void ConnectToTwitch(string Api, string userId, string Nick)
        {
            try
            {
                Creds = new ConnectionCredentials(Nick, Api);                
                TClient.Initialize(Creds, Nick);
                TClient.OnConnected += TClientOnConnected;
                TClient.OnMessageReceived += TClientOnMessageReceived;
                TClient.OnChatCommandReceived += TClientOcChatCommandReceived;
                TClient.OnDisconnected += TClientOnDisconnected;
                TClient.OnConnectionError += TClientOnConnectionError;
                TClient.Connect();

                TPubSub.OnPubSubServiceConnected += TPubSubServiceConnected;
                TPubSub.OnChannelPointsRewardRedeemed += TPubSubChannelPointsRewardRedeemed;
                TPubSub.Connect();



                // Обновление Label в UI-потоке
                UpdateLabelConnectionStatus("Twitch подключен", System.Windows.Media.Colors.Green);
            }

            catch (Exception)
            {
                UpdateLabelConnectionStatus("Twitch не смог подключиться", System.Windows.Media.Colors.Green);
            }
        }

        private void TClientOnReconnected(object? sender, OnReconnectedEventArgs e)
        {
            //TClient.Reconnect();
        }

        private void TClientOnConnectionError(object? sender, OnConnectionErrorArgs e)
        {
            try
            { //переподключение к твичу
                UpdateLabelConnectionStatus($"{Convert.ToString(ReconnectTryNumber)} Переподключение...", System.Windows.Media.Colors.Purple);
                ReconnectTryNumber += 1;
                TClient?.Disconnect();
                TPubSub?.Disconnect();
                TClient = null;
                TPubSub = null;
                Instance.Disconnect();
                _instance = null;
                _instance = new TwitchConnection();
                Thread.Sleep(3000);
            }
            catch { TClientOnConnectionError(null, null); }
            ReconnectTryNumber = 1;

            if (_instance.TClient.IsConnected == false)
                TClientOnConnectionError(null, null);
        }

        //команды

        private void TClientOcChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            if (e.Command.ChatMessage.Message.StartsWith(GetUserVoiceCommand) || e.Command.ChatMessage.Message.StartsWith($"!{GetUserVoiceCommand}"))
            {
                //вывод голоса X пользователя в чат
                string[] parts = e.Command.ChatMessage.Message.Split(' ');
                if (parts.Length >= 2)
                {
                    string nickname = parts[^1].ToLowerInvariant();
                    try
                    {
                        foreach (var item in TextToSpeech.twitchUserVoicesDict)
                        {
                            if (item.Key.ToLower() == nickname)
                            {
                                TClient.SendMessage(twitchNick, item.Value);
                            }

                        }
                    }
                    catch { }
                }



            }
        }



        // //////////////////////////////////////////////////////////////////////////////////////////////
        private void TPubSubChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
        {
            //изменение голоса за баллы канала
            if (e.RewardRedeemed.Redemption.Reward.Title == ChangeVoiceChannelPointsRewardName)
            {
                //сначала записывает в файл, потом читает его заново
                UserChangeVoice.UserChangeName(e.RewardRedeemed.Redemption.User.DisplayName.ToLower(), e.RewardRedeemed.Redemption.UserInput, "twitch");
                individualVoicesTwitch = UpdateVoices.LoadAndSaveIndividualVoices(false, null, "twitch");
                
            }
        }

        private void TClientOnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            TClient.OnDisconnected -= TClientOnDisconnected;
            TClient.OnConnected -= TClientOnConnected;
            TClient.OnMessageReceived -= TClientOnMessageReceived;
            TClient.OnChatCommandReceived -= TClientOcChatCommandReceived;
            TClient.OnDisconnected -= TClientOnDisconnected;
            TPubSub.OnPubSubServiceConnected -= TPubSubServiceConnected;
            TPubSub.OnChannelPointsRewardRedeemed -= TPubSubChannelPointsRewardRedeemed;
            TClient.OnConnectionError -= TClientOnConnectionError;
            TClient.Disconnect();
            TPubSub.Disconnect();
            UpdateLabelConnectionStatus("Twitch отключился...", System.Windows.Media.Colors.Red);
            _instance = null;
            TClientOnConnectionError(null, null);
        }

        private async void  TClientOnConnected(object? sender, OnConnectedArgs e)
        {
            try
            {
                TClient.SendMessage(twitchNick, "Connected");
            }
            catch (Exception)
            {
                UpdateLabelConnectionStatus("Ошибка при отключении ты тут навечно", System.Windows.Media.Colors.Red);
            }
        }

        private void TClientOnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            //добавление новому пользователю случайного голоса
            if (e.ChatMessage.CustomRewardId != null)
            {
                return;
            }

            try
            {
                if (!TwitchBlackList.Items.Contains(e.ChatMessage.Username))
                {
                    if (TextToSpeech.availableRandomVoices.Count > 0)
                    {
                        if (!twitchNicknameSet.Contains(e.ChatMessage.Username))
                        { //11.16.2024 20:47 это можно перенести в TTS часть, но я хочу оставить тут, 
                          //чтобы для каждого сервиса логика могла быть своей при необходимости
                            string randomVoice = TextToSpeech.availableRandomVoices[random.Next(TextToSpeech.availableRandomVoices.Count)];
                            JObject newUser = new JObject
                    {
                        { "Nickname", e.ChatMessage.Username },
                        { "Voice", randomVoice}
                    };
                            //добавить нового человека, сохранить список и сразу его прочитать чтобы не было ошибок со сменой голоса пользователем
                            individualVoicesTwitch.Add(newUser);
                            individualVoicesTwitch = UpdateVoices.LoadAndSaveIndividualVoices(true, individualVoicesTwitch, "twitch");
                            twitchNicknameSet.Add(e.ChatMessage.Username);
                            //TwitchIndividualVoicesList.TwitchAddNewUserToStackPanel(); //см. метод
                            TextToSpeech.Play(e.ChatMessage.Message, e.ChatMessage.Username, "twitch");
                            return;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(TextToSpeech.MessageSkip) || !string.IsNullOrEmpty(TextToSpeech.MessageSkipAll))
                            {
                                if (e.ChatMessage.Message == TextToSpeech.MessageSkip || e.ChatMessage.Message == TextToSpeech.MessageSkipAll)
                                {
                                    TextToSpeech.CancelMessages(e.ChatMessage.Message);
                                    return;
                                }
                                else if (e.ChatMessage.Message[0] == TextToSpeech.DoNotTtsIfStartWith[0])
                                    return;
                            }
                            TextToSpeech.Play(e.ChatMessage.Message, e.ChatMessage.Username, "twitch");
                        }
                    }
                    else TClient.SendMessage(twitchNick, "Нет доступных голосов в индивидуальных голосах");
                    return;

                }
            }
            catch
            {
                JObject newUser = new JObject
                {
                    { "Nickname", e.ChatMessage.Username },
                    { "Voice", Convert.ToString(TextToSpeech.Synth.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList())}
                };
                individualVoicesTwitch.Add(newUser);
                UpdateVoices.LoadAndSaveIndividualVoices(false, null, "twitch"); //true = saveBeforeLoad
                TextToSpeech.Play(e.ChatMessage.Message, e.ChatMessage.Username, "twitch");
            }
        }
        string Message;
        string[] words;
        string wordReplacedMessage;
        string processedMessage;
        string emojiPattern = @"(?:[\u203C-\u3299\u00A9\u00AE\u2000-\u3300\uF000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF])";

        private void TPubSubServiceConnected(object? sender, EventArgs e)
        {
            //не из-за ошибок в коде (провайдер, роскомнадзор, сервера твича) МОЖЕТ не подключаться к пабсабу
            try
            {
                TPubSub.ListenToChannelPoints(TwitchUserId);
                TPubSub.SendTopics(TwitchUserApi, false);
                TClient.SendMessage("toltoon45", "ПабСаб");
            }
            catch { }
        }

        public void Disconnect()
        {
            try
            {
                if (TClient.IsConnected == true)
                {
                    TClient.Disconnect();
                    TPubSub.Disconnect();
                    UpdateLabelConnectionStatus("Twitch отключен вручную", System.Windows.Media.Colors.Red);
                }

            }
            catch (Exception ex)
            {
                UpdateLabelConnectionStatus("Ошибка при отключении ты тут навечно", System.Windows.Media.Colors.Red);
            }
        }

        private void UpdateLabelConnectionStatus(string errorText, Color color)
        {
            LabelTwitchStatusMessage.Dispatcher.Invoke(() =>
            {
                LabelTwitchStatusMessage.Content = $"{errorText}";
                LabelTwitchStatusMessage.Foreground = new SolidColorBrush(color);
            });
        }

        public static void UpdateTwitchNicknameSet()
        {
            //при удалении пользователя во время работы программы twitchNicknameSet НЕ обновляется
            twitchNicknameSet = new HashSet<string>(newJsonForSaveTwitch.Select(item => item["Nickname"]?.ToString()).Where(nick => nick != null));
        }

        public static void Reconnect()
        {
            
        }
    }
}
