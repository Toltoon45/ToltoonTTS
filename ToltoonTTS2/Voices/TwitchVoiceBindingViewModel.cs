using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class TwitchVoiceBindingsViewModel
{
    public ObservableCollection<UserVoiceBinding> UserVoiceBindings { get; set; }

    public TwitchVoiceBindingsViewModel(SQLiteConnection twitchDb, SQLiteConnection voicesDb)
    {
        var userBindings = twitchDb.Table<TwitchIndividualVoices>().ToList();
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
                twitchDb.Update(binding); // сохраняем замену в базу
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
    }
}
