using ToltoonTTS2.Twitch.Connection;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
//using TwitchLib.PubSub;

public enum TwitchConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Failed
}
public class TwitchConnectToChat : ITwitchConnectToChat
{
    private readonly TwitchClient _client;
    public TwitchConnectionState CurrentState { get; private set; } = TwitchConnectionState.Disconnected;

    public event EventHandler<TwitchLib.Client.Events.OnMessageReceivedArgs> MessageReceived;
    public event EventHandler<TwitchConnectionState> ConnectionStateChanged;

    public TwitchConnectToChat()
    {
        _client = new TwitchClient();
        _client.OnConnected += (s, e) =>
        {
            UpdateState(TwitchConnectionState.Connected);
        };

        _client.OnConnectionError += (s, e) =>
        {
            UpdateState(TwitchConnectionState.Failed);
        };

        _client.OnDisconnected += (s, e) =>
        {
            UpdateState(TwitchConnectionState.Disconnected);
        };

        _client.OnMessageReceived += (s, e) =>
        {
            MessageReceived?.Invoke(this, e);
        };
    }

    public async Task ConnectToChat(string twitchApi, string twitchNickname)
    {
        try
        {
            UpdateState(TwitchConnectionState.Connecting);

            var credentials = new TwitchLib.Client.Models.ConnectionCredentials(twitchNickname, twitchApi);
            _client.Initialize(credentials, twitchNickname);

            _client.Connect();
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