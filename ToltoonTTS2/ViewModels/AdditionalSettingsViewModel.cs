using System.ComponentModel;

namespace ToltoonTTS2.ViewModels
{
    public class AdditionalSettingsViewModel : INotifyPropertyChanged
    {
        public AdditionalSettingsViewModel()
        {
            // Загружаем значения при старте
            StreamerChecked = Properties.Settings.Default.StreamerChecked;
            ModeratorChecked = Properties.Settings.Default.ModeratorChecked;
            VipChecked = Properties.Settings.Default.VipChecked;
            AllChecked = Properties.Settings.Default.AllChecked;
            SubscriberlChecked = Properties.Settings.Default.SubscriberlChecked;
        }


        private bool _streamerChecked;
        private bool _moderatorChecked;
        private bool _vipChecked;
        private bool _allChecked;
        private bool _subscriberlChecked;

        public bool SubscriberlChecked
        {
            get => _subscriberlChecked;
            set
            {
                if (_subscriberlChecked != value)
                {
                    _subscriberlChecked = value;
                    OnPropertyChanged(nameof(SubscriberlChecked));
                    OnPropertyChanged(nameof(IsAnyChecked));
                    Save(nameof(SubscriberlChecked), value);
                }
            }
        }

        public bool StreamerChecked
        {
            get => _streamerChecked;
            set
            {
                if (_streamerChecked != value)
                {
                    _streamerChecked = value;
                    OnPropertyChanged(nameof(StreamerChecked));
                    OnPropertyChanged(nameof(IsAnyChecked));
                    Save(nameof(StreamerChecked), value);
                }
            }
        }

        public bool ModeratorChecked
        {
            get => _moderatorChecked;
            set
            {
                if (_moderatorChecked != value)
                {
                    _moderatorChecked = value;
                    OnPropertyChanged(nameof(ModeratorChecked));
                    OnPropertyChanged(nameof(IsAnyChecked));
                    Save(nameof(ModeratorChecked), value);
                }
            }
        }

        public bool VipChecked
        {
            get => _vipChecked;
            set
            {
                if (_vipChecked != value)
                {
                    _vipChecked = value;
                    OnPropertyChanged(nameof(VipChecked));
                    OnPropertyChanged(nameof(IsAnyChecked));
                    Save(nameof(VipChecked), value);
                }
            }
        }

        public bool AllChecked
        {
            get => _allChecked;
            set
            {
                if (_allChecked != value)
                {
                    _allChecked = value;
                    OnPropertyChanged(nameof(AllChecked));
                    OnPropertyChanged(nameof(IsAnyChecked));
                    Save(nameof(AllChecked), value);
                }
            }
        }

        private void Save(string name, bool value)
        {
            // универсальный метод
            Properties.Settings.Default[name] = value;
            Properties.Settings.Default.Save();
        }

        public bool IsAnyChecked => StreamerChecked || ModeratorChecked || VipChecked || AllChecked || SubscriberlChecked;


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
