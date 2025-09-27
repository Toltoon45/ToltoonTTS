using SQLite;

public class VoiceItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string VoiceName { get; set; }

    public string Volume { get; set; }

    public string Speed { get; set; }

    public bool IsEnabled { get; set; }
}
