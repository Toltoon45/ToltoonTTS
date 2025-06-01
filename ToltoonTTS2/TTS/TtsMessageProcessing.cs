using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace ToltoonTTS2.TTS
{
    public class TtsMessageProcessing : ITtsMessageProcessing
    {
        private ObservableCollection<string> _blackList;
        private ObservableCollection<string> _wordToReplace;
        private ObservableCollection<string> _wordToReplaceWith;
        private readonly ITts _tts;
        private bool _removeEmoji;
        private string _doNotTtsIfStartWith;
        private string _skipMessage;
        private string _skipMessageAll;
        //удаление эмодзи
        private const string EmojiPattern = @"(?:[\u203C-\u3299\u00A9\u00AE\u2000-\u3300\uF000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF])";
        //переделывание ссылок в слово link
        private const string LinkReplace = @"(?:https?:\/\/)?(?:www\.)?(?:x\.com|twitter\.com)[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]*|(?:https?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+";

        public string ProcessIncomingMessage(string username, string message)
        {
            if (_blackList.Any(blackListMember => username.Contains(blackListMember, StringComparison.OrdinalIgnoreCase)))
                return null;

            if (message == _skipMessage)
                return "пропуск1";
            if (message == _skipMessageAll)
                return "пропуск1все";

            if (_doNotTtsIfStartWith != null && message.StartsWith(_doNotTtsIfStartWith) && _doNotTtsIfStartWith != "")
                return null;



            var erredactedMessage = message;
            // Замена точек для избежания незапланированной замены слова на link
            erredactedMessage = Regex.Replace(erredactedMessage, @"\.{2,}", " . ");

            // Заменяем ссылки словом "link"
            erredactedMessage = Regex.Replace(erredactedMessage, LinkReplace, "link");//@"(?:https?:\/\/)?(?:www\.)?(?:x\.com|twitter\.com)[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]*|(?:https?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+",

            // Удаление эмодзи
            if (_removeEmoji)
            {
                erredactedMessage = Regex.Replace(erredactedMessage, EmojiPattern, string.Empty);
            }
            //Вторичная обработка
            if (_wordToReplace.Count > 0) // Надо ли вообще менять
            {
                string processedMessage = "";
                string wordReplacedMessage = "";

                // Разделяем сообщение на слова
                string[] words = Regex.Split(erredactedMessage, @"\s+");

                foreach (string word in words)
                {
                    wordReplacedMessage = word;

                    // Находим индекс слова в коллекции
                    int index = -1;
                    for (int i = 0; i < _wordToReplace.Count; i++)
                    {
                        if (_wordToReplace[i].ToLower() == wordReplacedMessage.ToLower())
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        wordReplacedMessage = _wordToReplaceWith[index];
                    }

                    processedMessage = $"{processedMessage} {wordReplacedMessage}";
                }
                erredactedMessage = processedMessage;
                //erredactedMessage = processedMessage;

            }
            return erredactedMessage;
        }

        public void SetSkipMessage(string SkipMessage)
        {
            _skipMessage = SkipMessage;
        }

        public void SetSkipAllMessages(string SkipAllMessages)
        {
            _skipMessageAll = SkipAllMessages;
        }

        public void WordToReplace(ObservableCollection<string> WordToReplace)
        {
            _wordToReplace = WordToReplace;
        }

        public void WordToReplaceWith(ObservableCollection<string> WordToReplaceWith)
        {
            _wordToReplaceWith = WordToReplaceWith;
        }

        public void SetBlackList(ObservableCollection<string> blackList)
        {
            _blackList = blackList;
        }

        public void SetRemoveEmoji(bool removeEmoji)
        {
            _removeEmoji = removeEmoji;
        }

        public void SetDoNotTtsIfStartWith(string start)
        {
            _doNotTtsIfStartWith = start;
        }
    }
}
