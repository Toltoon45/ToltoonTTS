using System.Windows.Forms;
using YoutubeLiveChatSharp;

namespace ToltoonTTS2.Services.Youtube
{

    public class YoutubeConnection : IYoutubeConnection
    {
        private static CancellationTokenSource _cts;

        public event EventHandler<YoutubeMessageEventArgs> MessageReceived;

        protected virtual void OnMessageReceived(YoutubeMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public Task Connect(string streamId, string connectionStatus)
        {
            _cts = new CancellationTokenSource();

            if (string.IsNullOrEmpty(streamId))
            {
                connectionStatus = "Youtube Failed";
                return Task.CompletedTask;
            }

            connectionStatus = "Youtube Connecting...";

            _ = Task.Run(() => ListenLoop(streamId, _cts.Token));

            return Task.CompletedTask;
        }

        private async Task ListenLoop(string streamId, CancellationToken token)
        {
            try
            {
                var chatFetch = new ChatFetch(streamId);

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var comments = await Task.Run(() => chatFetch.Fetch(), token);

                        foreach (var comment in comments)
                        {
                            OnMessageReceived(new YoutubeMessageEventArgs
                            {
                                UserName = comment.userName,
                                Message = comment.text
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }

                    await Task.Delay(1000, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        internal static void StopYoutubeConnect()
        {
            _cts?.Cancel();
        }

    }

    public class YoutubeMessageEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}
