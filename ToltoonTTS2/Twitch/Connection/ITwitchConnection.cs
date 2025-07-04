﻿namespace ToltoonTTS2.Twitch.Connection
{
    public interface ITwitchGetID
    {
        Task<string> GetTwitchUserId(string TwitchApi, string TwitchClientId, string TwitchNickname);
        //Task ConnectToTwitchChat(string TwitchApi, string TwitchClientId, string TwitchNickname);
        //event Action<string> OnMessageReceived;
    }

    //public interface ITwitchConnectToChat
    //{
    //    Task ConnectToChat(string twitchApi, string twitchClientId, string twitchNickname);
    //    //event Action<string> onMessageReceived;
    //    event EventHandler<TwitchLib.Client.Events.OnMessageReceivedArgs> MessageReceived;
    //}
    public interface ITwitchConnectToChat
    {
        Task ConnectToChat(string twitchApi, string twitchNickname);
        //event Action<string> onMessageReceived;
        event EventHandler<TwitchLib.Client.Events.OnMessageReceivedArgs> MessageReceived;
        event EventHandler<TwitchConnectionState> ConnectionStateChanged;
        TwitchConnectionState CurrentState { get; }
    }

}
