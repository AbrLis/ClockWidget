namespace ClockWidgetApp.Services
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Абстракция для работы с файловой системой
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>Проверяет существование файла</summary>
        bool FileExists(string path);

        /// <summary>Открывает файл для чтения</summary>
        Stream OpenRead(string path);

        /// <summary>Создает директорию если ее нет</summary>
        void CreateDirectory(string path);

        /// <summary>Читает весь текст из файла</summary>
        Task<string> ReadAllTextAsync(string path);

        /// <summary>Записывает текст в файл</summary>
        Task WriteAllTextAsync(string path, string contents);

        /// <summary>Создает резервную копию файла</summary>
        Task CreateBackupAsync(string sourcePath, string backupPath);

        /// <summary>Удаляет файл если он существует</summary>
        void DeleteFileIfExists(string path);
    }
}