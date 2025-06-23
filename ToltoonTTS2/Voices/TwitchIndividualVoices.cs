using SQLite;

public class TwitchIndividualVoices
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string UserName { get; set; }
    public string VoiceName { get; set; }
}