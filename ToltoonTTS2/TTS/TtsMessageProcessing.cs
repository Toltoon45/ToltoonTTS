using SQLite;
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

        private SQLiteConnection _LoadIndividualVoicesTwitchDb;
        private SQLiteConnection _LoadIndividualVoicesSettings;
        private ObservableCollection<PlaceVoicesInfoInWPF> _enabledVoices;
        public void SetDatabase(SQLiteConnection db, SQLiteConnection db2)
        {
            _LoadIndividualVoicesTwitchDb = db;
            _LoadIndividualVoicesSettings = db2;
        }

        public void SetEnabledVoices(ObservableCollection<PlaceVoicesInfoInWPF> voices)
        {
            _enabledVoices = voices;
        }

        public string GetVoiceForUser(string username)
        {
            var binding = _LoadIndividualVoicesTwitchDb.Table<TwitchIndividualVoices>().FirstOrDefault(x => x.UserName == username);
            return binding?.VoiceName;
        }



        //удаление эмодзи
        private const string EmojiPattern = @"(?:[\u203C-\u3299\u00A9\u00AE\u2000-\u3300\uF000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF])";
        //переделывание ссылок в слово link
        private const string LinkReplace = @"(?:https?:\/\/)?(?:www\.)?(?:x\.com|twitter\.com)[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]*|(?:https?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+";

        public ProcessedTtsMessage ProcessIncomingMessage(string username, string message)
        {
            if (_blackList.Any(blackListMember => username.Contains(blackListMember, StringComparison.OrdinalIgnoreCase)))
                return null;

            if (message == _skipMessage)
                return new ProcessedTtsMessage { Text = "пропуск1", VoiceName = null };

            if (message == _skipMessageAll)
                return new ProcessedTtsMessage { Text = "пропуск1все", VoiceName = null };

            if (!string.IsNullOrEmpty(_doNotTtsIfStartWith) && message.StartsWith(_doNotTtsIfStartWith))
                return null;

            var binding = _LoadIndividualVoicesTwitchDb.Table<TwitchIndividualVoices>().FirstOrDefault(x => x.UserName == username);
            if (binding == null)
            {
                var enabledVoiceList = _LoadIndividualVoicesSettings.Table<VoiceItem>()
                          .Where(v => v.IsEnabled)
                          .ToList();

                if (enabledVoiceList.Count == 0)
                    return null;

                var random = new Random();
                var selectedVoice = enabledVoiceList[random.Next(enabledVoiceList.Count)];

                binding = new TwitchIndividualVoices
                {
                    UserName = username,
                    VoiceName = selectedVoice.VoiceName
                };
                _LoadIndividualVoicesTwitchDb.Insert(binding);
            }

            string voiceName = binding.VoiceName;

            string erredactedMessage = message;
            erredactedMessage = Regex.Replace(erredactedMessage, @"\.{2,}", " . ");
            erredactedMessage = Regex.Replace(erredactedMessage, LinkReplace, "link");

            if (_removeEmoji)
                erredactedMessage = Regex.Replace(erredactedMessage, EmojiPattern, string.Empty);

            if (_wordToReplace.Count > 0)
            {
                string processedMessage = "";
                string wordReplacedMessage = "";
                string[] words = Regex.Split(erredactedMessage, @"\s+");

                foreach (string word in words)
                {
                    wordReplacedMessage = word;

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
                        wordReplacedMessage = _wordToReplaceWith[index];

                    processedMessage = $"{processedMessage} {wordReplacedMessage}";
                }

                erredactedMessage = processedMessage;
            }

            var voiceSettings = _LoadIndividualVoicesSettings.Table<VoiceItem>()
    .FirstOrDefault(v => v.VoiceName == voiceName);

            return new ProcessedTtsMessage
            {
                Text = erredactedMessage,
                VoiceName = voiceName,
                VoiceVolume = Convert.ToInt32(voiceSettings.Volume),
                VoiceSpeed = Convert.ToInt32(voiceSettings.Speed)
            };
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
