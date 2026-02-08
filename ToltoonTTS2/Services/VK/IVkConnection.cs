namespace ToltoonTTS2.Services.VK
{
    public interface IVkConnection
    {
        Task Connection(string channelNick, string VkSecretApi, string VkAppId);
        event EventHandler<VkConnectionState> ConnectionStateChanged;
        event EventHandler<VkMessageEventArgs> MessageReceived;
        VkConnectionState CurrentState { get; }
    }
}