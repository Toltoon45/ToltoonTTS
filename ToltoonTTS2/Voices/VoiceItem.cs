using System.ComponentModel;
using System.Runtime.CompilerServices;

public class VoiceItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Culture { get; set; }

    private string _textBox1Value;
    public string TextBox1Value
    {
        get => _textBox1Value;
        set
        {
            _textBox1Value = value;
            OnPropertyChanged();
        }
    }

    private string _textBox2Value;
    public string TextBox2Value
    {
        get => _textBox2Value;
        set
        {
            _textBox2Value = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
