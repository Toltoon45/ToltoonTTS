using System.Collections.ObjectModel;

namespace ToltoonTTS2.Services.BlackList
{
    public interface IBlackListServices
    {
        void AddToBlackList(string item);
        void RemoveFromBlackList(string item);
        ObservableCollection<string> BlackList { get; }
    }

    public interface IWordreplace
    {
        void AddToWordReplace(string item1, string item2);
        void DeleteWordFromReplace(int wordReplaceSelectedIndex);

        ObservableCollection<string> WordToReplace { get; }
        ObservableCollection<string> WordWhatToReplaceWith { get; }
    }
}
