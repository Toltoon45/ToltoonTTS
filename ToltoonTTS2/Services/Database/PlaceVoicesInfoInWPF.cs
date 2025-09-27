using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Windows.Input;
using SQLite;
using ToltoonTTS2.ViewModels;

public class PlaceVoicesInfoInWPF : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SQLiteConnection Db { get; set; }
    private string _textBoxVolume;
    private string _textBoxSpeed;
    private bool _isEnabled;
    public MainViewModel StatusReporter { get; set; }

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

    public ICommand TestVoiceCommand => new RelayCommand(TestVoice);

    private SpeechSynthesizer _synth;

    private void TestVoice()
    {
        try
        {
            StatusReporter.VoiceTestErrorMessage = "";
            // Если синтезатор уже существует — остановить и пересоздать
            _synth?.Dispose();
            _synth = new SpeechSynthesizer();

            _synth.SelectVoice(Name);

            if (int.TryParse(TextBoxVolume, out int vol))
                _synth.Volume = Math.Clamp(vol, 0, 100);

            if (int.TryParse(TextBoxSpeed, out int rate))
                _synth.Rate = Math.Clamp(rate, -10, 10);

            _synth.SpeakAsync("123");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения: {ex.Message}");
            StatusReporter.VoiceTestErrorMessage = $"Ошибка воспроизведения: {ex.Message}";
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









    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged;
    }

}
