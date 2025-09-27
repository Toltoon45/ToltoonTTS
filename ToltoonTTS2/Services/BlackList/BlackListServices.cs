using System.Collections.ObjectModel;

namespace ToltoonTTS2.Services.BlackList
{
    class BlackListServices : IBlackListServices
    {
        private readonly ObservableCollection<string> _blackList;

        public BlackListServices()
        {
            _blackList = new ObservableCollection<string>();
        }

        public ObservableCollection<string> BlackList => _blackList;

        public void AddToBlackList(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                return;

            if (!_blackList.Contains(item))
            {
                _blackList.Add(item);
            }
        }

        public void RemoveFromBlackList(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                return;

            if (_blackList.Contains(item))
            {
                _blackList.Remove(item);
            }
        }
    }
}
