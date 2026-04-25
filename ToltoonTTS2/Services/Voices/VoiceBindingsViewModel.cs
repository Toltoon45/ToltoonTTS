using SQLite;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class VoiceBindingsViewModel
{
    private readonly SQLiteConnection _platformDb;
    private readonly ObservableCollection<PlaceVoicesInfoInWPF> _allVoices;

    public ObservableCollection<UserVoiceBinding> UserVoiceBindings { get; set; }

    public ICommand SaveCommand { get; }

    public VoiceBindingsViewModel(
        SQLiteConnection platformDb,
        SQLiteConnection voicesDb,
        ObservableCollection<PlaceVoicesInfoInWPF> ItemSourceAllVoices)
    {
        _platformDb = platformDb;
        _allVoices = ItemSourceAllVoices;

        voicesDb.RunInTransaction(() =>
        {
            voicesDb.DeleteAll<VoiceItem>();

            foreach (var uiVoice in ItemSourceAllVoices)
            {
                voicesDb.Insert(new VoiceItem
                {
                    VoiceName = uiVoice.Name,
                    IsEnabled = uiVoice.IsEnabled,
                    Speed = uiVoice.TextBoxSpeed,
                    Volume = uiVoice.TextBoxVolume
                    // другие поля — если появятся
                });
            }
        });

        var enabledVoices = voicesDb.Table<VoiceItem>()
            .Where(v => v.IsEnabled)
            .Select(v => v.VoiceName)
            .ToList();

        // fallback: если вдруг все голоса выключены
        if (enabledVoices.Count == 0)
        {
            enabledVoices = voicesDb.Table<VoiceItem>()
                .Select(v => v.VoiceName)
                .ToList();
        }

        var rng = new Random();

        var userBindings = _platformDb
            .Table<PlatformsIndividualVoices>()
            .ToList()
            .OrderBy(x => x.UserName)
            .ToList();

        foreach (var binding in userBindings)
        {
            if (!enabledVoices.Contains(binding.VoiceName))
            {
                binding.VoiceName = enabledVoices[rng.Next(enabledVoices.Count)];
                _platformDb.Update(binding);
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
