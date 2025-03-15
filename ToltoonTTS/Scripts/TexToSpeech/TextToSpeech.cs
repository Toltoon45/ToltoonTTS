using Newtonsoft.Json.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using ToltoonTTS.Scripts.GoodGame;
public static class TextToSpeech
{
    public static readonly SpeechSynthesizer Synth = new SpeechSynthesizer(); // Статический экземпляр
    public static bool CanRemoveEmoji;
    public static string? TtsVoiceName;
    public static int TtsVolume;
    public static int TtsSpeed;
    public static bool IndividualVoiceForAll;

    public static JArray twitchUserVoices;
    public static Dictionary<string, string> twitchUserVoicesDict;
    private static string targetNickname;
    private static string targetVoice;
    public static JArray voiceFullInfo;
    public static JArray voiceFullInfoGoodgame;

    public static string GoodgameId;

    //Выставленные голоса и изменённые имена голосов
    public static List<string> availableRandomVoices = new List<string>();
    public static List<string> individualVoicesRenamed = new List<string>();

    public static List<string> WhatToReplace = new List<string>();
    public static List<string> WhatToReplaceWith = new List<string>();
    public static string whatPlatformMessageFrom;
    // Очередь сообщений
    private static ConcurrentQueue<(string message, string username)> messageQueue = new ConcurrentQueue<(string message, string username)>();
    private static bool isSpeaking = false;

    public static string DoNotTtsIfStartWith;
    public static string MessageSkip;
    public static string MessageSkipAll;

    public static void Play(string message, string username, string whatService)
    {
        if (isSpeaking)
        {
            if (message == MessageSkip || message == MessageSkipAll)
                CancelMessages(message);
        }
        whatPlatformMessageFrom = whatService;

        if (!string.IsNullOrEmpty(DoNotTtsIfStartWith))
            if (message.StartsWith(DoNotTtsIfStartWith))
            {
                return;
            }

        // Добавляем сообщение в очередь
        messageQueue.Enqueue((message, username));

        // Если сейчас не происходит озвучивание, начинаем процесс
        if (!isSpeaking)
        {
            ProcessQueue();
        }
    }
    static TextToSpeech()
    {
        Synth.SpeakCompleted += Synth_SpeakCompleted; // Подписываемся один раз

    }

    private static void ProcessQueue()
    {
        try
        {
            if (messageQueue.TryDequeue(out var msgInfo))
            {
                //Первичная обработка
                isSpeaking = true;

                string message = msgInfo.message;
                string username = msgInfo.username;

                // Изменение текста и подготовка сообщения к озвучиванию
                var erredactedMessage = message;
                string emojiPattern = @"(?:[\u203C-\u3299\u00A9\u00AE\u2000-\u3300\uF000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF])";

                // Заменяем несколько точек на одну точку
                erredactedMessage = Regex.Replace(erredactedMessage, @"\.{2,}", " . ");

                // Заменяем ссылки словом "link"
                erredactedMessage = Regex.Replace(erredactedMessage, @"(?:http(s)?:\/\/)?[\w.-]+\D(?:\.[\w\.-]+)+[\w\-\._~:/?%#[\]@!\$&'\(\)\*\+,;=.]+", "link");

                // Удаление эмодзи, если включено
                if (CanRemoveEmoji)
                {
                    erredactedMessage = Regex.Replace(erredactedMessage, emojiPattern, string.Empty);
                }
                //Вторичная обработка
                if (WhatToReplace.Count > 0) // Надо ли вообще менять
                {
                    string processedMessage = "";
                    string wordReplacedMessage = "";

                    // Разделяем сообщение на слова
                    string[] words = Regex.Split(erredactedMessage, @"\s+");

                    foreach (string word in words)
                    {
                        wordReplacedMessage = word;
                        // Проверяем, есть ли слово в списке WhatToReplace
                        int index = WhatToReplace.FindIndex(w => w.ToLower() == wordReplacedMessage.ToLower());
                        if (index >= 0)
                        {
                            // Если слово найдено, заменяем его
                            wordReplacedMessage = WhatToReplaceWith[index];
                        }
                        processedMessage = $"{processedMessage} {wordReplacedMessage}";
                    }

                    erredactedMessage = processedMessage;
                }

                if (IndividualVoiceForAll)
                {
                    if (whatPlatformMessageFrom == "twitch")
                    {
                        targetVoice = twitchUserVoicesDict.TryGetValue(username, out var voice) ? voice : null;
                    }
                    else if (whatPlatformMessageFrom == "goodgame")
                    {
                        targetVoice = GoodGameConnection.goodgameUserVoicesDict.TryGetValue(username, out var voice) ? voice : null;
                    }
                    if (targetVoice != null)
                    {
                        foreach (var item in voiceFullInfo)
                        {
                            try
                            {
                                if (targetVoice == item["ComboBoxValue"]?.ToString())
                                {
                                    Synth.SelectVoice(item["ComboBoxValue"]?.ToString());
                                    Synth.Volume = Convert.ToInt32(item["TextBoxVoice"] ?? "50");
                                    Synth.Rate = Convert.ToInt32(item["TextBoxSpeed"] ?? "3");
                                    Synth.SpeakAsync(erredactedMessage);
                                    break;
                                }
                            }
                            catch
                            {
                                Synth.SelectVoice(availableRandomVoices[0]);
                                Synth.Volume = 50;
                                Synth.Rate = 3;
                                Synth.SpeakAsync("Заглушка. С говорилкой что-то не так");
                                break;
                            }

                        }
                    }
                    else
                    {
                        Synth.SelectVoice(availableRandomVoices[0]);
                        Synth.Volume = 50;
                        Synth.Rate = 3;
                        Synth.SpeakAsync(erredactedMessage);
                    }

                }
                else
                {
                    try
                    {
                        var installedVoices = Synth.GetInstalledVoices();
                        Synth.Rate = TtsSpeed;
                        Synth.Volume = TtsVolume;
                        Synth.SelectVoice(TtsVoiceName);

                        Synth.SpeakAsync(erredactedMessage);
                    }
                    catch
                    {
                        ClearQueue();
                        Synth.SpeakAsyncCancelAll();
                        foreach (var voices in Synth.GetInstalledVoices())
                        {
                            try
                            {
                                Synth.SelectVoice(voices.VoiceInfo.Name);

                                isSpeaking = false;
                                TtsVoiceName = voices.VoiceInfo.Name;
                                return;
                            }
                            catch
                            {
                                continue;
                            }

                        }
                        return;
                    }
                }
            }
            else
            {
                isSpeaking = false; // Очередь пуста
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки, если необходимо
            Console.WriteLine($"Произошла ошибка: {ex.Message}");

            // Вызов метода DeleteAllQueuePanic в случае ошибки
            DeleteAllQueuePanic();
        }
    }

    private static void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
    {
        // Продолжаем с озвучиванием следующего сообщения в очереди
        ProcessQueue();
    }

    internal static void CancelMessages(string message)
    {
            if (Convert.ToString(message.ToLower()) == MessageSkip)
            {
                var current = Synth.GetCurrentlySpokenPrompt();

                if (current != null)
                    Synth.SpeakAsyncCancel(current);
                return;
            }
            if (Convert.ToString(message.ToLower()) == MessageSkipAll)
            {
                // Очищаем очередь сообщений перед отменой
                ClearQueue();
                Synth.SpeakAsyncCancelAll();
                return;
            }
            return;
    }
    private static void DeleteAllQueuePanic()
    {
        ClearQueue();
        Synth.SpeakAsyncCancelAll();
    }

    private static void ClearQueue()
    {
        isSpeaking = false;
        while (messageQueue.TryDequeue(out _))
        {
            // Просто извлекаем сообщения из очереди, не обрабатываем их
        }
    }

}
