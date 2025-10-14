﻿using System.ComponentModel;
using ToltoonTTS2.Services.Twitch.Connection;
using ToltoonTTS2.ViewModels;
using TwitchLib.Client;
//using TwitchLib.PubSub;

public enum TwitchConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Failed
}
class TwitchConnectToChat : ITwitchConnectToChat
{
    private readonly AdditionalSettingsViewModel _settings;


    private readonly TwitchClient _client;
    public TwitchConnectionState CurrentState { get; private set; } = TwitchConnectionState.Disconnected;

    private readonly TwitchClient _test2;

    public event EventHandler<TwitchLib.Client.Events.OnMessageReceivedArgs> MessageReceived;
    public event EventHandler<TwitchConnectionState> ConnectionStateChanged;

    public TwitchConnectToChat(AdditionalSettingsViewModel additionalSettings)
    {
        _settings = additionalSettings;
        _client = new TwitchClient();
        _test2 = new TwitchClient();
        _client.OnConnected += (s, e) =>
        {
            UpdateState(TwitchConnectionState.Connected);
        };

        _client.OnConnectionError += (s, e) =>
        {
            UpdateState(TwitchConnectionState.Failed);
        };

        //_test2.OnMessageReceived += (s, e) =>
        //{
        //    Thread.Sleep(2000);
        //    //_test2.SendMessage(e.ChatMessage.Channel, $"{e.ChatMessage.Message} 1");
        //};


        _client.OnDisconnected += (s, e) =>
        {
            UpdateState(TwitchConnectionState.Disconnected);
        };

        _settings.PropertyChanged += OnSettingsChanged;
        _client.OnMessageReceived += (s, e) =>
        {
            if (_settings.AllChecked)
            {
                MessageReceived?.Invoke(this, e);
                return;
            }
            if (_settings.StreamerChecked && e.ChatMessage.IsBroadcaster)
            {
                MessageReceived?.Invoke(this, e);
                return;
            }
            if (_settings.VipChecked && e.ChatMessage.IsVip)
            {
                MessageReceived?.Invoke(this, e);
                return;
            }
            if (_settings.ModeratorChecked && e.ChatMessage.IsModerator)
            {
                MessageReceived?.Invoke(this, e);
                return;
            }
            if (_settings.SubscriberlChecked && e.ChatMessage.IsSubscriber)
            {
                MessageReceived?.Invoke(this, e);
                return;
            }

        };
    }
    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settings.StreamerChecked) ||
            e.PropertyName == nameof(_settings.ModeratorChecked) ||
            e.PropertyName == nameof(_settings.VipChecked) ||
            e.PropertyName == nameof(_settings.SubscriberlChecked) ||
            e.PropertyName == nameof(_settings.AllChecked))
        {
            Console.WriteLine($"[DEBUG] Setting changed: {e.PropertyName}");
        }
    }



    public async Task ConnectToChat(string twitchApi, string twitchNickname)
    {
        try
        {
            UpdateState(TwitchConnectionState.Connecting);

            var credentials = new TwitchLib.Client.Models.ConnectionCredentials(twitchNickname, twitchApi);
            _client.Initialize(credentials, twitchNickname);
            //_test2.Initialize(credentials, "toltoon47");
            _client.Connect();
            //_test2.Connect();
            //await Task.Delay(2000);
        }
        catch (Exception)
        {
            UpdateState(TwitchConnectionState.Failed);
        }
    }

    private void UpdateState(TwitchConnectionState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            ConnectionStateChanged?.Invoke(this, newState);
        }
    }
}