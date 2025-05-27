namespace ToltoonTTS2.Scripts.EnsureFolderAndFileExist
{
    public interface IDirectoryService
    {
        void CreateFolder(string folderName, string folderPath);
        void CreateFile(string fileName, string filePath);
        void EnsureAppStructureExists();
    }
}
