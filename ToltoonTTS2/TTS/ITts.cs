using System.Collections.ObjectModel;

namespace ToltoonTTS2.TTS
{
    public interface ITts
    {
        void Speak(string text);

        void SetVoice(string voiceName);
        void SetRate(int rate);
        void SetVolume(int volume);
    }
    
    public interface ITtsMessageProcessing
    {
        void ProcessIncomingMessage(string username, string message);

        void SetSkipMessage(string SkipMessage);
        void SetSkipAllMessages(string SkipAllMessages);
        void WordToReplace(ObservableCollection<string> WordToReplace);
        void WordToReplaceWith(ObservableCollection<string> WordToReplaceWith);
    }
}
