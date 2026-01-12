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
    }
}
