using System.ComponentModel;
using System.Runtime.CompilerServices;

public class PlaceVoicesInfoInWPF : INotifyPropertyChanged
{
    public string Name { get; set; }
    public int Id { get; set; }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }
    private string _textBoxVolume;
    public string TextBoxVolume
    {
        get => _textBoxVolume;
        set
        {
            _textBoxVolume = value;
            OnPropertyChanged();
        }
    }

    private string _textBoxSpeed;
    public string TextBoxSpeed
    {
        get => _textBoxSpeed;
        set
        {
            _textBoxSpeed = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}