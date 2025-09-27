using ToltoonTTS2.Properties;

namespace ToltoonTTS2.Services.SaveLoadSettings
{
    public class SettingsService : ISettings
    {
        public AppSettings LoadSettings()
        {
            // Попробуем разобрать строку динамической скорости в список int
            var dynamicSpeedList = Settings.Default.DynamicSpeed?
                .Split(',')
                .Select(s => int.TryParse(s, out var val) ? val : 0)
                .ToList() ; // Значения по умолчанию

            return new AppSettings
            {
                TwitchApi = Settings.Default.TwitchApi,
                TwitchClientId = Settings.Default.TwitchID,
                TwitchNickname = Settings.Default.TwitchNickname,
                ConnectToTwitch = Settings.Default.ConnectToTwitch,
                GoodgameNickname = Settings.Default.GoodgameNickname,
                ConnectToGoodgame = Settings.Default.ConnectToGoodgame,
                RemoveEmoji = Settings.Default.RemoveEmoji,
                SelectedVoice = Settings.Default.InstalledVoiceSelect,
                TtsSpeedValue = Settings.Default.TtsSpeedValue,
                TtsVolumeValue = Settings.Default.TtsVolumeValue,
                DoNotTtsIfStartWith = Settings.Default.TextBoxDoNotTtsIfStartWith,
                SkipMessage = Settings.Default.TextBoxMessageSkip,
                SkipMessageAll = Settings.Default.TextBoxMessageSkipAll,
                BlackListMembers = Settings.Default.BlackListMembers?.Cast<string>().ToList() ?? new List<string>(),
                WhatToReplace = Settings.Default.WhatToReplace?.Cast<string>().ToList() ?? new List<string>(),
                WhatToReplaceWith = Settings.Default.WhatToReplaceWith?.Cast<string>().ToList() ?? new List<string>(),
                IndividualVoicesEnabled = Settings.Default.IndividualVoicesEnabled,
                DynamicSpeedThresholds = dynamicSpeedList
            };
        }

        public void SaveSettings(AppSettings settings)
        {

            Settings.Default.TwitchApi = settings.TwitchApi;
            Settings.Default.TwitchID = settings.TwitchClientId;
            Settings.Default.TwitchNickname = settings.TwitchNickname;
            Settings.Default.ConnectToTwitch = settings.ConnectToTwitch;
            Settings.Default.GoodgameNickname = settings.GoodgameNickname;
            Settings.Default.ConnectToGoodgame = settings.ConnectToGoodgame;
            Settings.Default.RemoveEmoji = settings.RemoveEmoji;
            Settings.Default.InstalledVoiceSelect = settings.SelectedVoice;
            Settings.Default.TtsSpeedValue = settings.TtsSpeedValue;
            Settings.Default.TtsVolumeValue = settings.TtsVolumeValue;
            Settings.Default.TextBoxDoNotTtsIfStartWith = settings.DoNotTtsIfStartWith;
            Settings.Default.TextBoxMessageSkip = settings.SkipMessage;
            Settings.Default.TextBoxMessageSkipAll = settings.SkipMessageAll;
            Settings.Default.IndividualVoicesEnabled = settings.IndividualVoicesEnabled;

            var blackListMembers = new System.Collections.Specialized.StringCollection();
            foreach (var item in settings.BlackListMembers)
            {
                blackListMembers.Add(item);
            }            
            Settings.Default.BlackListMembers = blackListMembers;

            var WhatToReplace = new System.Collections.Specialized.StringCollection();
            foreach (var item in settings.WhatToReplace)
            {
                WhatToReplace.Add(item);
            }
            Settings.Default.WhatToReplace = WhatToReplace;

            var WhatToReplaceWith = new System.Collections.Specialized.StringCollection();
            foreach (var item in settings.WhatToReplaceWith)
            {
                WhatToReplaceWith.Add(item);
            }
            Settings.Default.WhatToReplaceWith = WhatToReplaceWith;

            var DynamicSpeed = new System.Collections.Specialized.StringCollection();
            foreach(var item in settings.DynamicSpeedThresholds)
            {
                DynamicSpeed.Add(Convert.ToString(item));
            }

            Settings.Default.Save();
        }
    }
}
