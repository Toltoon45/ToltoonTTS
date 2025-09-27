using System.ComponentModel;

public class UserVoiceBinding : INotifyPropertyChanged
{
    private string _selectedVoice;

    public string UserName { get; set; }

    public List<string> AvailableVoices { get; set; }

    public string SelectedVoice
    {
        get => _selectedVoice;
        set
        {
            if (_selectedVoice != value)
            {
                _selectedVoice = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVoice)));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}