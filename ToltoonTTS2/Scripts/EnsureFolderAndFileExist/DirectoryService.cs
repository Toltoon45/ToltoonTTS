using System.IO;

namespace ToltoonTTS2.Scripts.EnsureFolderAndFileExist
{
    public class DirectoryService : IDirectoryService
    {
        private string DefaultJsonContent = "[]";
        public void EnsureAppStructureExists()
        {
            // Проверка и создание папок
            CreateFolder("DataForProgram", "DataForProgram");
            CreateFolder("BlackList", "DataForProgram/BlackList");
            CreateFolder("Profiles", "DataForProgram/Profiles");
            CreateFolder("WordReplace", "DataForProgram/WordReplace");
            CreateFolder("Voices", "DataForProgram/Voices");

            // Проверка и создание файлов с дефолтным содержимым
            CreateFile("BlackListUsers.json", "DataForProgram/BlackList");
            CreateFile("WhatToReplace.json", "DataForProgram/WordReplace");
            CreateFile("WhatToReplaceWith.json", "DataForProgram/WordReplace");
        }


        public void CreateFolder(string folderName, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
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
