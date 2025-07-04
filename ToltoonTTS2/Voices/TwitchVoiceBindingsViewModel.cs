using SQLite;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class TwitchVoiceBindingsViewModel
{
    private readonly SQLiteConnection _platformDb; // ← добавляем нужное поле
    private readonly SQLiteConnection _dbGoodgame; // ← добавляем нужное поле

    public ObservableCollection<UserVoiceBinding> UserVoiceBindings { get; set; }

    public ICommand SaveCommand { get; }

    public TwitchVoiceBindingsViewModel(SQLiteConnection platformDb, SQLiteConnection voicesDb)
    {
        _platformDb = platformDb;

        var userBindings = _platformDb.Table<PlatformsIndividualVoices>().ToList().OrderBy(x => x.UserName);


        var enabledVoices = voicesDb.Table<VoiceItem>()
            .Where(v => v.IsEnabled)
            .Select(v => v.VoiceName)
            .ToList();

        var allAvailableVoices = voicesDb.Table<VoiceItem>()
            .Select(v => v.VoiceName)
            .ToList();

        var rng = new Random();

        foreach (var binding in userBindings)
        {
            // Проверяем, включён ли голос
            bool isVoiceEnabled = voicesDb.Table<VoiceItem>()
                .Any(v => v.VoiceName == binding.VoiceName && v.IsEnabled);

            // Если голос отключён, выбираем случайный включённый
            if (!isVoiceEnabled && enabledVoices.Count > 0)
            {
                var newVoice = enabledVoices[rng.Next(enabledVoices.Count)];
                binding.VoiceName = newVoice;
                _platformDb.Update(binding); // сохраняем замену в базу
            }
        }

        UserVoiceBindings = new ObservableCollection<UserVoiceBinding>(
            userBindings.Select(b => new UserVoiceBinding
            {
                UserName = b.UserName,
                SelectedVoice = b.VoiceName,
                AvailableVoices = enabledVoices
            })
        );

        SaveCommand = new RelayCommand(SaveAllUserVoiceBindings);
    }

    public void SaveAllUserVoiceBindings()
    {
        foreach (var binding in UserVoiceBindings)
        {
            var record = _platformDb.Table<PlatformsIndividualVoices>().FirstOrDefault(r => r.UserName == binding.UserName);
            if (record != null)
            {
                record.VoiceName = binding.SelectedVoice;
                _platformDb.Update(record);
            }
            else
            {
                _platformDb.Insert(new PlatformsIndividualVoices
                {
                    UserName = binding.UserName,
                    VoiceName = binding.SelectedVoice
                });
            }
        }
    }

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
