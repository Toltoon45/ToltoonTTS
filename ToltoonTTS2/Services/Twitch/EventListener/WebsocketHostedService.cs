using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.Models;

namespace TwitchLib.EventSub.Websockets.Example
{
    public class WebsocketHostedService : IHostedService
    {
        private readonly ILogger<WebsocketHostedService> _logger;
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;
        private readonly TwitchAPI _twitchApi = new();
        private string _userId;
        private bool _ttsForChannelPointsEnabled;
        private string _nameOfRewardTtsForChannelPoints;

        public WebsocketHostedService(ILogger<WebsocketHostedService> logger, EventSubWebsocketClient eventSubWebsocketClient, 
            string twitchApi,
            string twitchClientId,
            string twitchId,
            bool ttsForChannelPointsEnabled,
            string nameOfTtsReward)
        {
            _twitchApi.Settings.AccessToken = twitchApi;
            _userId = twitchId;
            _twitchApi.Settings.ClientId = twitchClientId;
            _nameOfRewardTtsForChannelPoints = nameOfTtsReward;
            _ttsForChannelPointsEnabled = ttsForChannelPointsEnabled;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _eventSubWebsocketClient = eventSubWebsocketClient ?? throw new ArgumentNullException(nameof(eventSubWebsocketClient));
            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += OnChannelPointsRedemption;

            _eventSubWebsocketClient.UnknownEventSubNotification += OnUnknownEventSubNotification;
        }

        public WebsocketHostedService()
        {
        }

        private async Task OnChannelPointsRedemption(object? sender, ChannelPointsCustomRewardRedemptionArgs e)
        {
            var redemption = e.Payload.Event;
            string redemptionName = redemption.Reward.Title;
            if (_ttsForChannelPointsEnabled)
            {
                if (_nameOfRewardTtsForChannelPoints == redemptionName)
                {
                    OnRewardRedeemed(new ChannelPointsMessageEventArgs
                    {
                        UserName = e.Payload.Event.UserName,
                        Message = e.Payload.Event.UserInput
                    });
                }


            }
        }

        public async static void SaveWavToEffectsFolder(MemoryStream wavStream)
        {//
            // Папка рядом с exe: /DataForProgram/SoundEffects
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string folder = Path.Combine(basePath, "DataForProgram", "SoundEffects");
            Directory.CreateDirectory(folder);
            string saveFolder = Path.Combine(basePath, "DataForProgram", "SoundEffects");
            Process.Start("SilentReverb.vbs", "DataForProgram\\SoundEffects");
            // Имя файла
            string fileName = "SoundEffect.wav";
            string filePath = Path.Combine(folder, fileName);

            // Сохраняем поток
            wavStream.Position = 0;
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                wavStream.CopyToAsync(fs);
            }

            wavStream.Position = 0;
        }


        public event EventHandler<ChannelPointsMessageEventArgs> RewardRedeemed;
        protected virtual void OnRewardRedeemed(ChannelPointsMessageEventArgs e)
        {
            RewardRedeemed?.Invoke(this, e);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.DisconnectAsync();
        }

        private async Task OnWebsocketConnected(object sender, WebsocketConnectedArgs e)
        {

            _logger.LogInformation($"Websocket {_eventSubWebsocketClient.SessionId} connected!");

            if (!e.IsRequestedReconnect)
            {
                // subscribe to topics
                // create condition Dictionary
                // You need BOTH broadcaster and moderator values or EventSub returns an Error!
                var condition = new Dictionary<string, string> { { "broadcaster_user_id", _userId }, { "moderator_user_id", _userId } };
                // Create and send EventSubscription
                await _twitchApi.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.follow", "2", condition, EventSubTransportMethod.Websocket,
                    _eventSubWebsocketClient.SessionId, accessToken: _twitchApi.Settings.AccessToken);

                await _twitchApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    "channel.channel_points_custom_reward_redemption.add", // событие о редемпшене награды
                    "1",
                    condition,
                    EventSubTransportMethod.Websocket,
                    _eventSubWebsocketClient.SessionId,
                    accessToken: _twitchApi.Settings.AccessToken
                );

                //_twitchApi.Helix.Chat.SendChatAnnouncementAsync();

                // If you want to get Events for special Events you need to additionally add the AccessToken of the ChannelOwner to the request.
                // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/
            }




        }

        private async Task OnWebsocketDisconnected(object sender, WebsocketDisconnectedArgs e)
        {
            _logger.LogError($"Websocket {_eventSubWebsocketClient.SessionId} disconnected!");

            // Don't do this in production. You should implement a better reconnect strategy
            while (!await _eventSubWebsocketClient.ReconnectAsync())
            {
                _logger.LogError("Websocket reconnect failed!");
                await Task.Delay(1000);
            }
        }

        private async Task OnWebsocketReconnected(object sender, WebsocketReconnectedArgs e)
        {
            _logger.LogWarning($"Websocket {_eventSubWebsocketClient.SessionId} reconnected");
        }

        // Handling notifications that are not (yet) implemented
        private async Task OnUnknownEventSubNotification(object sender, UnknownEventSubNotificationArgs e)
        {
            var metadata = (WebsocketEventSubMetadata)e.Metadata;
            _logger.LogInformation("Received event that has not yet been implemented: type:{type}, version:{version}", metadata.SubscriptionType, metadata.SubscriptionVersion);

            switch ((metadata.SubscriptionType, metadata.SubscriptionVersion))
            {
                case ("channel.chat.message", "1"): /*code to handle the event*/ break;
                default: break;
            }
        }
        public void EnableTtsForChannelPoint(bool value)
        {
            _ttsForChannelPointsEnabled = value;
        }
        public void SetNameForChannelPoints(string value)
        {
            _nameOfRewardTtsForChannelPoints = value;
        }
    }
    public class ChannelPointsMessageEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}