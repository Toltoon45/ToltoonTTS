namespace ToltoonTTS2.Services.EnsureFolderAndFileExist
{
    public interface IDirectoryService
    {
        void CreateFolder(string folderName, string folderPath);
        void CreateFile(string fileName, string filePath);
        void EnsureAppStructureExists();
    }
}
