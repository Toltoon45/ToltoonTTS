using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class PlaceVoicesInfoInWPF : INotifyPropertyChanged
{
    public int Id { get; set; }

    private string _textBoxVolume;
    private string _textBoxSpeed;
    private bool _isEnabled;

    public string Name { get; set; }

    public SQLiteConnection Db { get; set; } // ← передаётся из ViewModel/инициализатора

    public string TextBoxVolume
    {
        get => _textBoxVolume;
        set
        {
            if (_textBoxVolume != value)
            {
                _textBoxVolume = value;
                OnPropertyChanged(nameof(TextBoxVolume));
                SaveToDatabase();
            }
        }
    }

    public string TextBoxSpeed
    {
        get => _textBoxSpeed;
        set
        {
            if (_textBoxSpeed != value)
            {
                _textBoxSpeed = value;
                OnPropertyChanged(nameof(TextBoxSpeed));
                SaveToDatabase();
            }
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
                SaveToDatabase();
            }
        }
    }

    private void SaveToDatabase()
    {
        if (Db == null) return;

        var existing = Db.Table<VoiceItem>().FirstOrDefault(v => v.Id == Id);
        if (existing != null)
        {
            existing.Volume = TextBoxVolume;
            existing.Speed = TextBoxSpeed;
            existing.IsEnabled = IsEnabled;
            Db.Update(existing);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}