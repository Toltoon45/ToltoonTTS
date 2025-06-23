using SQLite;
using System.Collections.ObjectModel;

namespace ToltoonTTS2.TTS
{
    public interface ITts
    {
        void Speak(ProcessedTtsMessage _result);

        void SetVoice(string voiceName);
        void SetRate(int rate);
        void SetVolume(int volume);
    }
    
    public interface ITtsMessageProcessing
    {
        ProcessedTtsMessage ProcessIncomingMessage(string username, string message);

        void SetSkipMessage(string SkipMessage);
        void SetSkipAllMessages(string SkipAllMessages);
        void WordToReplace(ObservableCollection<string> WordToReplace);
        void WordToReplaceWith(ObservableCollection<string> WordToReplaceWith);
        void SetBlackList(ObservableCollection<string> blackList);
        void SetRemoveEmoji(bool removeEmoji);
        void SetDoNotTtsIfStartWith(string start);

        string GetVoiceForUser(string username);
        void SetDatabase(SQLiteConnection db, SQLiteConnection db2);
    }
}
