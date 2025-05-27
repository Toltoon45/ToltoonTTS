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

        public TtsMessageProcessing()
        {
            //_blackList = blackList;
            //_removeEmoji = removeEmoji;
            //_doNotTtsIfStartWith = doNotTtsIfStartWith;
            //_skipMessage = skipMessage;
            //_skipMessageAll = skipMessageAll;
        }

        public void ProcessIncomingMessage(string username, string message)
        {
            if (_blackList.Any(blackListMember => username.Contains(blackListMember, StringComparison.OrdinalIgnoreCase)))
                return;
            if (message.StartsWith(_doNotTtsIfStartWith))
                return;
            // Изменение текста и подготовка сообщения к озвучиванию
            var erredactedMessage = message;
            // Заменяем несколько точек на одну точку/
            erredactedMessage = Regex.Replace(erredactedMessage, @"\.{2,}", " . ");

            // Заменяем ссылки словом "link"
            erredactedMessage = Regex.Replace(erredactedMessage, LinkReplace, "link");//@"(?:https?:\/\/)?(?:www\.)?(?:x\.com|twitter\.com)[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]*|(?:https?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+",

            // Удаление эмодзи, если включено
            //if (CanRemoveEmoji)
            //{
            //    erredactedMessage = Regex.Replace(erredactedMessage, emojiPattern, string.Empty);
            //}
            //Вторичная обработка
            //if (WhatToReplace.Count > 0) // Надо ли вообще менять
            //{
            //    string processedMessage = "";
            //    string wordReplacedMessage = "";

                // Разделяем сообщение на слова
                string[] words = Regex.Split(erredactedMessage, @"\s+");

                //foreach (string word in words)
                //{
                //    wordReplacedMessage = word;
                //    // Проверяем, есть ли слово в списке WhatToReplace
                //    int index = WhatToReplace.FindIndex(w => w.ToLower() == wordReplacedMessage.ToLower());
                //    if (index >= 0)
                //    {
                //        // Если слово найдено, заменяем его
                //        wordReplacedMessage = WhatToReplaceWith[index];
                //    }
                //    processedMessage = $"{processedMessage} {wordReplacedMessage}";
                //}

                //erredactedMessage = processedMessage;
            //}
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
    }
}
