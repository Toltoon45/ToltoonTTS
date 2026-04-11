using System.IO;

namespace ToltoonTTS2.Services.EnsureFolderAndFileExist
{
    public class DirectoryService : IDirectoryService
    {
        private string DefaultJsonContent = "[]";
        private readonly string _cwd = Environment.CurrentDirectory;
        public Task EnsureAppStructureExists()
        {
            // Проверка и создание папок
            CreateFolder("DataForProgram", _cwd);
            CreateFolder("BlackList", Path.Combine(_cwd, "DataForProgram"));
            CreateFolder("Profiles", Path.Combine(_cwd, "DataForProgram"));
            CreateFolder("WordReplace", Path.Combine(_cwd, "DataForProgram"));
            CreateFolder("Voices", Path.Combine(_cwd, "DataForProgram"));
            CreateFolder("SoundEffects", Path.Combine(_cwd, "DataForProgram"));
            CreateFolder("models", _cwd);
            CreateFolder("piper", _cwd);

            // Проверка и создание файлов с дефолтным содержимым
            CreateFile("BlackListUsers.json", Path.Combine(_cwd, "DataForProgram", "BlackList"));
            CreateFile("WhatToReplace.json", Path.Combine(_cwd, "DataForProgram", "WordReplace"));
            CreateFile("WhatToReplaceWith.json", Path.Combine(_cwd, "DataForProgram", "WordReplace"));

            return Task.CompletedTask;
        }


        public void CreateFolder(string folderName, string basePath)
        {
            var fullPath = Path.Combine(basePath, folderName);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public void CreateFile(string fileName, string folderPath)
        {
            string fullPath = Path.Combine(folderPath, fileName); // Объединяем путь к папке и имя файла
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, DefaultJsonContent);
            }
        }
    }
}
