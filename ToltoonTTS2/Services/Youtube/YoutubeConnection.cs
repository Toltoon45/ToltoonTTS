using System.Windows.Forms;
using ToltoonTTS2.Services.VK;
using YoutubeLiveChatSharp;

namespace ToltoonTTS2.Services.Youtube
{
    public enum YoutubeConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Failed
    }


    public class YoutubeConnection : IYoutubeConnection
    {
        private static CancellationTokenSource _cts;

        public YoutubeConnectionState CurrentState { get; private set; }

        public event EventHandler<YoutubeConnectionState> ConnectionStateChanged;

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
                UpdateState(YoutubeConnectionState.Failed);
                return Task.CompletedTask;
            }

            UpdateState(YoutubeConnectionState.Connecting);

            _ = Task.Run(() => ListenLoop(streamId, _cts.Token));
            UpdateState(YoutubeConnectionState.Connected);
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
                    catch
                    {
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

        private void UpdateState(YoutubeConnectionState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                ConnectionStateChanged?.Invoke(this, newState);
            }
        }

    }

    public class YoutubeMessageEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}