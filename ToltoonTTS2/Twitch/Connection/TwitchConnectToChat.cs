using ToltoonTTS2.Twitch.Connection;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
//using TwitchLib.PubSub;

class TwitchConnectToChat : ITwitchConnectToChat
{
    private readonly TwitchClient _client;
    //private readonly TwitchPubSub _pubsub;
    private string _twitchNickname;

    public event EventHandler<OnMessageReceivedArgs> MessageReceived;

    public TwitchConnectToChat()
    {
        _client = new TwitchClient();
    }

    //public async Task ConnectToChat(string twitchApi, string twitchClientId, string twitchNickname)
    public async Task ConnectToChat(string twitchApi, string twitchNickname)
    {
        _twitchNickname = twitchNickname;
        ConnectionCredentials Tcreds = new ConnectionCredentials(twitchNickname, twitchApi);
        _client.OnConnected += onConnected;
        _client.OnMessageReceived += (s, e) => MessageReceived?.Invoke(s, e);
        _client.Initialize(Tcreds, twitchNickname);
        await Task.Run(() => _client.Connect());
    }

    private void onConnected(object? sender, OnConnectedArgs e)
    {
        _client.SendMessage(_twitchNickname, "Connect");
    }
}
