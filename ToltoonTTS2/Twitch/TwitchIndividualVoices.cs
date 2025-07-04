using SQLite;

public class PlatformsIndividualVoices
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string UserName { get; set; }
    public string VoiceName { get; set; }
}