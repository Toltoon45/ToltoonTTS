namespace ToltoonTTS2.Goodgame.Connection
{
    public interface IGoodgameConnection
    {
        Task Connection(string ChannelToConnect);
        event EventHandler<GoodgameConnectionState> ConnectionStateChanged;
        event EventHandler<GoodgameMessageEventArgs> MessageReceived;
        GoodgameConnectionState CurrentState { get; }
    }
}
