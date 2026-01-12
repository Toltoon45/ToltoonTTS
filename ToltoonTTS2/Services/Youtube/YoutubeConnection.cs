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

        public async Task Connect(string streamId, string connectionStatus)
        {
            _cts = new CancellationTokenSource();

            if (string.IsNullOrEmpty(streamId))
            {
                connectionStatus = "Youtube  Failed";
                return;
            }
            //библиотека для получения сообщений чата выдаёт только comment.text, остальное всё "". Может потом переделаю
            try
            {
                connectionStatus = "Youtube  Connecting...";
                var chatFetch = new ChatFetch(streamId);
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var fetchTask = Task.Run(() => chatFetch.Fetch(), _cts.Token);
                        var timeoutTask = Task.Delay(5000, _cts.Token);

                        if (await Task.WhenAny(fetchTask, timeoutTask) == fetchTask)
                        {
                            var comments = await fetchTask;
                            foreach (var comment in comments)
                            {
                                OnMessageReceived(new YoutubeMessageEventArgs
                                {
                                    UserName = comment.userName,
                                    Message = comment.text
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }

                    await Task.Delay(1000, _cts.Token); // Задержка между запросами
                }
            }
            catch (OperationCanceledException)
            {
                connectionStatus = "Youtube Disconnected";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
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
