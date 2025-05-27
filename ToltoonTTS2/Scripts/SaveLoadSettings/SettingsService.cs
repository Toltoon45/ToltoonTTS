namespace ToltoonTTS2.Scripts.SaveLoadSettings
{
    public class SettingsService : ISettings
    {
        public AppSettings LoadSettings()
        {
            return new AppSettings
            {
                TwitchApi = Properties.Settings.Default.TwitchApi,
                TwitchClientId = Properties.Settings.Default.TwitchID,
                TwitchNickname = Properties.Settings.Default.TwitchNickname,
                ConnectToTwitch = Properties.Settings.Default.ConnectToTwitch,
                GoodgameNickname = Properties.Settings.Default.GoodgameNickname,
                ConnectToGoodgame = Properties.Settings.Default.ConnectToGoodgame,
                RemoveEmoji = Properties.Settings.Default.RemoveEmoji,
                SelectedVoice = Properties.Settings.Default.InstalledVoiceSelect,
                TtsSpeedValue = Properties.Settings.Default.TtsSpeedValue,
                TtsVolumeValue = Properties.Settings.Default.TtsVolumeValue,
                DoNotTtsIfStartWith = Properties.Settings.Default.TextBoxDoNotTtsIfStartWith,
                SkipMessage = Properties.Settings.Default.TextBoxMessageSkip,
                SkipMessageAll = Properties.Settings.Default.TextBoxMessageSkipAll,
                BlackListMembers = Properties.Settings.Default.BlackListMembers?.Cast<string>().ToList() ?? new List<string>(),  // 👈 Загрузка BlackList
                WhatToReplace = Properties.Settings.Default.WhatToReplace?.Cast<string>().ToList() ?? new List<string>(),
                WhatToReplaceWith = Properties.Settings.Default.WhatToReplaceWith?.Cast<string>().ToList() ?? new List<string>()

            };
        }

        public void SaveSettings(AppSettings settings)
        {
            Properties.Settings.Default.TwitchApi = settings.TwitchApi;
            Properties.Settings.Default.TwitchID = settings.TwitchClientId;
            Properties.Settings.Default.TwitchNickname = settings.TwitchNickname;
            Properties.Settings.Default.ConnectToTwitch = settings.ConnectToTwitch;
            Properties.Settings.Default.GoodgameNickname = settings.GoodgameNickname;
            Properties.Settings.Default.ConnectToGoodgame = settings.ConnectToGoodgame;
            Properties.Settings.Default.RemoveEmoji = settings.RemoveEmoji;
            Properties.Settings.Default.InstalledVoiceSelect = settings.SelectedVoice;
            Properties.Settings.Default.TtsSpeedValue = settings.TtsSpeedValue;
            Properties.Settings.Default.TtsVolumeValue = settings.TtsVolumeValue;
            Properties.Settings.Default.TextBoxDoNotTtsIfStartWith = settings.DoNotTtsIfStartWith;
            Properties.Settings.Default.TextBoxMessageSkip = settings.SkipMessage;
            Properties.Settings.Default.TextBoxMessageSkipAll = settings.SkipMessageAll;

            var blackListMembers = new System.Collections.Specialized.StringCollection();
            foreach (var item in settings.BlackListMembers)
            {
                blackListMembers.Add(item);
            }            
            Properties.Settings.Default.BlackListMembers = blackListMembers;

            var WhatToReplace = new System.Collections.Specialized.StringCollection();
            foreach (var item in settings.WhatToReplace)
            {
                WhatToReplace.Add(item);
            }
            Properties.Settings.Default.WhatToReplace = WhatToReplace;

            var WhatToReplaceWith = new System.Collections.Specialized.StringCollection();
            foreach (var item in settings.WhatToReplaceWith)
            {
                WhatToReplaceWith.Add(item);
            }
            Properties.Settings.Default.WhatToReplaceWith = WhatToReplaceWith;
            Properties.Settings.Default.Save();
        }
    }
}
