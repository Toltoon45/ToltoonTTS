namespace ToltoonTTS2.Services.Twitch.Connection
{
    public interface ITwitchGetID
    {
        Task<string> GetTwitchUserId(string TwitchApi, string TwitchClientId, string TwitchNickname);
    }

    public interface ITwitchConnectToChat
    {
        Task ConnectToChat(string twitchApi, string twitchNickname);
        //event Action<string> onMessageReceived;
        event EventHandler<TwitchLib.Client.Events.OnMessageReceivedArgs> MessageReceived;
        event EventHandler<TwitchConnectionState> ConnectionStateChanged;
        TwitchConnectionState CurrentState { get; }
    }

}
