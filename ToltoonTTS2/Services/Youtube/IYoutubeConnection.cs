namespace ToltoonTTS2.Services.Youtube
{
    public interface IYoutubeConnection
    {
        Task Connect(string streamId, string connectionStatus);
        event EventHandler<YoutubeMessageEventArgs> MessageReceived;
    }
}
