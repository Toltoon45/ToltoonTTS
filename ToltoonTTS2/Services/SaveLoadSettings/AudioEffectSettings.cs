namespace ToltoonTTS2.Services.TTS
{
    public class AudioEffectSettings
    {
        public EffectSetting Vibrato { get; set; } = new();
        public EffectSetting Robot { get; set; } = new();
        public EffectSetting Delay { get; set; } = new();
        public EffectSetting Distortion { get; set; } = new();
        public EffectSetting Normalization { get; set; } = new();
    }

    public class EffectSetting
    {
        public bool Enabled { get; set; } = false;
        public int Strength { get; set; } = 50;
        public int Chance { get; set; } = 0;
    }
}