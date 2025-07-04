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
        ProcessedTtsMessage ProcessIncomingMessage(string username, string message, string platform);

        void SetSkipMessage(string SkipMessage);
        void SetSkipAllMessages(string SkipAllMessages);
        void WordToReplace(ObservableCollection<string> WordToReplace);
        void WordToReplaceWith(ObservableCollection<string> WordToReplaceWith);
        void SetBlackList(ObservableCollection<string> blackList);
        void SetRemoveEmoji(bool removeEmoji);
        void SetDoNotTtsIfStartWith(string start);
        void SetIndividualVoicesEnabled(bool enabled);
        void SetStandartVoiceName(string name);
        void SetStandardVoiceVolume(int volume);
        void SetStandartVoiceSpeed(int speed);

        //string GetVoiceForUser(string username);
        void SetDatabase(SQLiteConnection TwitchIndividualVoicesDb, SQLiteConnection IndividualVoicesSettingsDb, SQLiteConnection GoodgameIndividualVoicesDb);
    }
}
