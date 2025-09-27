using ToltoonTTS2.Services.Twitch.Connection;
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
    private readonly TwitchClient _client;
    public TwitchConnectionState CurrentState { get; private set; } = TwitchConnectionState.Disconnected;

    private readonly TwitchClient _test2;

    public event EventHandler<TwitchLib.Client.Events.OnMessageReceivedArgs> MessageReceived;
    public event EventHandler<TwitchConnectionState> ConnectionStateChanged;

    public TwitchConnectToChat()
    {
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
            _test2.Initialize(credentials, "toltoon47");
            _client.Connect();
            _test2.Connect();
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