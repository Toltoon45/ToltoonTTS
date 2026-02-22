using NickBuhro.Translit;
using Nut;
using Nut.Models;
using PiperSharp;
using PiperSharp.Models;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ToltoonTTS2.Services.TTS
{
    public static class PiperSharpTTS
    {

        async public static Task<bool> DownloadPiper(string VoiceName)
        {
            try
            {
                var cwd = Directory.GetCurrentDirectory();
                var modelsPath = Path.Combine(cwd, "models");
                var voiceModelPath = Path.Combine(modelsPath, VoiceName);

                // Проверяем, есть ли папка модели
                if (Directory.Exists(voiceModelPath))
                {
                    return false; // модель уже есть, скачивать не нужно
                }

                Directory.CreateDirectory(modelsPath);

                var model = await PiperDownloader.GetModelByKey(VoiceName);
                await model.DownloadModel("models");

                return true; // модель скачана
            }
            catch { return false; }
        }

        public static async Task<MemoryStream> GenerateVoice(string voiceName, string text, float voiceSpeed, bool translitToRussian)
        {
            try
            {
                text = ConvertNumbers(text);
                bool NeedToTranslitToRussian = voiceName.StartsWith("ru_RU") || voiceName.StartsWith("uk_UA");
                if (NeedToTranslitToRussian)
                {
                    text = Transliteration.LatinToCyrillic(text);
                    text = Regex.Replace(text, "q", "к", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "w", "в", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "y", "и", RegexOptions.IgnoreCase);
                }
                else
                {
                    text = Transliteration.CyrillicToLatin(text);
                    text = text.Replace("`", "");
                    text = Regex.Replace(text, "x", "h", RegexOptions.IgnoreCase);
                }

                    //if (voiceName.StartsWith )
                    text = CleanForTts(text);
                //if (translitToRussian)
                //{ //не в рашн ёмаё а в английский у тебя и так всё на русском крендель!
                //    text = Transliteration.LatinToCyrillic(text);
                //}
                var cwd = Directory.GetCurrentDirectory();
                var model = await VoiceModel.LoadModelByKey(voiceName);
                // чтобы избежать кэширования одинаковых запросов
                var piperModel = new PiperProvider(new PiperConfiguration()
                {
                    ExecutableLocation = Path.Join(cwd, "piper", "piper.exe"),
                    WorkingDirectory = Path.Join(cwd, "piper"),
                    Model = model,
                    SpeakingRate = 1 - voiceSpeed / 10 //войс спид 0 почему-то всегда
                });

                byte[] wavBytes = await piperModel.InferAsync(
                    text,
                    AudioOutputType.Wav
                );

                var wavMessage = new MemoryStream(wavBytes);

                return wavMessage;
            }
            catch (Exception)
            {
                return null;
            }

            
        }

static string ConvertNumbers(string text)
    {
        return Regex.Replace(text, @"\d+", match =>
        {
            long number;

            // пытаемся распарсить число
            if (!long.TryParse(match.Value, out number))
            {
                return match.Value;
            }

            try
            {
                return number.ToText("ru");
            }
            catch
            {
                return match.Value;
            }
        });
    }

    static string CleanForTts(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return new string(
                input.Where(c =>
                {
                    var cat = Char.GetUnicodeCategory(c);

                    return cat != UnicodeCategory.Control &&
                           cat != UnicodeCategory.Format &&
                           cat != UnicodeCategory.NonSpacingMark &&
                           cat != UnicodeCategory.Surrogate &&
                           c != '\0';
                }).ToArray()
            );
        }

    }
}
