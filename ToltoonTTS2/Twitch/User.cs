namespace ToltoonTTS2.Twitch
{
    class User
    {
        private int id { get; set; }
        private string UserName { get; set; }
        private string VoiceName { get; set; }

        public User() { }

        public User(int id, string userName, string voiceName)
        {
            this.id = id;
            this.UserName = userName;
            this.VoiceName = voiceName;
        }
    }
}
