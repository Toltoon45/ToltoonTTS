using System.Collections.ObjectModel;

namespace ToltoonTTS2.Services.BlackList
{
    class WordReplace : IWordreplace
    {
        private readonly ObservableCollection<string> _wordToReplace;
        private readonly ObservableCollection<string> _wordWhatToReplaceWith;
        public WordReplace()
        {
            _wordToReplace = new ObservableCollection<string>();
            _wordWhatToReplaceWith = new ObservableCollection<string>();
        }

        public ObservableCollection<string> WordToReplace => _wordToReplace;
        public ObservableCollection<string> WordWhatToReplaceWith => _wordWhatToReplaceWith;

        public void AddToWordReplace(string item1, string item2)
        {
            if (string.IsNullOrWhiteSpace(item1) || string.IsNullOrWhiteSpace(item2) || item1 == item2)
                return;
            if (!_wordToReplace.Any(item => item.ToLower().Equals(item1.ToLower())))
            {
                _wordToReplace.Add(item1);
                _wordWhatToReplaceWith.Add(item2);
            }
        }

        public void DeleteWordFromReplace(int wordReplaceSelectedIndex)
        {
            _wordToReplace.RemoveAt(wordReplaceSelectedIndex);
            _wordWhatToReplaceWith.RemoveAt(wordReplaceSelectedIndex);
        }
    }
}
