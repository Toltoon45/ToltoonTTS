using ToltoonTTS2.Properties;
using ToltoonTTS2.Services.TTS;
using System.Text.Json;

namespace ToltoonTTS2.Services.SaveLoadSettings
{
    public class SettingsService : ISettings
    {
        public AppSettings LoadSettings()
        {
            var dynamicSpeedList = Settings.Default.DynamicSpeed?
                .Split(',')
                .Select(s => int.TryParse(s, out var val) ? val : 0)
                .ToList(); // Значения по умолчанию
            var effects = ReadEffects();

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
                TtsVolumeValue = Settings.Default.TtsVolumeValue == 0 ? 50 : Settings.Default.TtsVolumeValue,
                TtsForChannelPoints = Settings.Default.TtsForChannelPoints,
                NameOfRewardTtsForChannelPoints = Settings.Default.NameOfRewardTtsForChannelPoints,
                DoNotTtsIfStartWith = Settings.Default.TextBoxDoNotTtsIfStartWith,
                SkipMessage = Settings.Default.TextBoxMessageSkip,
                SkipMessageAll = Settings.Default.TextBoxMessageSkipAll,
                BlackListMembers = Settings.Default.BlackListMembers?.Cast<string>().ToList() ?? new List<string>(),
                WhatToReplace = Settings.Default.WhatToReplace?.Cast<string>().ToList() ?? new List<string>(),
                WhatToReplaceWith = Settings.Default.WhatToReplaceWith?.Cast<string>().ToList() ?? new List<string>(),
                IndividualVoicesEnabled = Settings.Default.IndividualVoicesEnabled,
                DynamicSpeedThresholds = dynamicSpeedList,
                VkLogin = Settings.Default.ConnectToVkLogin,
                ConnectToVk = Settings.Default.ConnectToVkCheckBox,
                VkOpenApi = Settings.Default.ConnectToVkAppId,
                VkSecretApi = Settings.Default.ConnectToVkSecretApi,
                VibratoEnabled = effects.Vibrato.Enabled,
                VibratoStrength = effects.Vibrato.Strength,
                VibratoChance = effects.Vibrato.Chance,
                RobotEnabled = effects.Robot.Enabled,
                RobotStrength = effects.Robot.Strength,
                RobotChance = effects.Robot.Chance,
                DelayEnabled = effects.Delay.Enabled,
                DelayStrength = effects.Delay.Strength,
                DelayChance = effects.Delay.Chance,
                DistortionEnabled = effects.Distortion.Enabled,
                DistortionStrength = effects.Distortion.Strength,
                DistortionChance = effects.Distortion.Chance,
                NormalizationEnabled = effects.Normalization.Enabled
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
            Settings.Default.TtsForChannelPoints = settings.TtsForChannelPoints;
            Settings.Default.NameOfRewardTtsForChannelPoints = settings.NameOfRewardTtsForChannelPoints;
            Settings.Default.TextBoxDoNotTtsIfStartWith = settings.DoNotTtsIfStartWith;
            Settings.Default.TextBoxMessageSkip = settings.SkipMessage;
            Settings.Default.TextBoxMessageSkipAll = settings.SkipMessageAll;
            Settings.Default.IndividualVoicesEnabled = settings.IndividualVoicesEnabled;
            Settings.Default.ConnectToVkLogin = settings.VkLogin;
            Settings.Default.ConnectToVkCheckBox = settings.ConnectToVk;
            Settings.Default.ConnectToVkAppId = settings.VkOpenApi;
            Settings.Default.ConnectToVkSecretApi = settings.VkSecretApi;
            var effectSettings = new AudioEffectSettings
            {
                Vibrato = new EffectSetting { Enabled = settings.VibratoEnabled, Strength = settings.VibratoStrength, Chance = settings.VibratoChance },
                Robot = new EffectSetting { Enabled = settings.RobotEnabled, Strength = settings.RobotStrength, Chance = settings.RobotChance },
                Delay = new EffectSetting { Enabled = settings.DelayEnabled, Strength = settings.DelayStrength, Chance = settings.DelayChance },
                Distortion = new EffectSetting { Enabled = settings.DistortionEnabled, Strength = settings.DistortionStrength, Chance = settings.DistortionChance },
                Normalization = new EffectSetting
                {
                    Enabled = settings.NormalizationEnabled,
                    Strength = settings.NormalizationTargetVolume
                },
            };
            Settings.Default.AudioEffectsJson = JsonSerializer.Serialize(effectSettings);

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
            foreach (var item in settings.DynamicSpeedThresholds)
            {
                DynamicSpeed.Add(Convert.ToString(item));
            }

            Settings.Default.Save();
        }

        private static AudioEffectSettings ReadEffects()
        {
            try
            {
                return string.IsNullOrWhiteSpace(Settings.Default.AudioEffectsJson)
                    ? new AudioEffectSettings()
                    : JsonSerializer.Deserialize<AudioEffectSettings>(Settings.Default.AudioEffectsJson) ?? new AudioEffectSettings();
            }
            catch
            {
                return new AudioEffectSettings();
            }
        }
    }
}
