namespace ToltoonTTS2.Services.SaveLoadSettings
{
    public class AppSettings
    {
        public string TwitchApi { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchNickname { get; set; }
        public bool ConnectToTwitch { get; set; }
        public string GoodgameNickname { get; set; }
        public bool ConnectToGoodgame { get; set; }
        public bool RemoveEmoji { get; set; }
        public string SelectedVoice { get; set; }
        public int TtsSpeedValue { get; set; }
        public int TtsVolumeValue { get; set; }
        public bool TtsForChannelPoints { get; set; }
        public string NameOfRewardTtsForChannelPoints { get; set; }
        public string DoNotTtsIfStartWith { get; set; }
        public string SkipMessage { get; set; }
        public string SkipMessageAll { get; set; }
        public string CommandToGetOtherUserVoiceName { get; set; }
        public List<string> BlackListMembers { get; set; } = new List<string>();
        public List<string> WhatToReplace { get; set; } = new List<string>();
        public List<string> WhatToReplaceWith { get; set; } = new List<string>();
        public bool IndividualVoicesEnabled { get; set; }
        public List<int> DynamicSpeedThresholds { get; set; } = new List<int>();
        public string VkLogin { get; set; }
        public string VkOpenApi {  get; set; }
        public string VkSecretApi { get; set; }
        public bool ConnectToVk { get; set; }
        public bool VibratoEnabled { get; set; }
        public int VibratoStrength { get; set; }
        public int VibratoChance { get; set; }
        public bool RobotEnabled { get; set; }
        public int RobotStrength { get; set; }
        public int RobotChance { get; set; }
        public bool DelayEnabled { get; set; }
        public int DelayStrength { get; set; }
        public int DelayChance { get; set; }
        public bool DistortionEnabled { get; set; }
        public int DistortionStrength { get; set; }
        public int DistortionChance { get; set; }
        public bool NormalizationEnabled { get; set; }
        public int NormalizationTargetVolume { get; set; }
    }
}
