namespace ClockWidgetApp.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Serilog;

    /// <summary>
    /// Стандартная реализация работы с файловой системой
    /// </summary>
    public class FileSystemService : IFileSystemService
    {
        /// <summary>
        /// Проверяет существование файла по указанному пути
        /// </summary>
        /// <param name="path">Полный путь к файлу</param>
        /// <returns>True если файл существует, иначе False</returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Создает директорию по указанному пути, если она не существует
        /// </summary>
        /// <param name="path">Полный путь к директории</param>
        /// <exception cref="UnauthorizedAccessException">Нет прав на создание директории</exception>
        /// <exception cref="PathTooLongException">Слишком длинный путь</exception>
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Log.Information("Создана директория: {Path}", path);
            }
        }

        /// <summary>
        /// Асинхронно читает все содержимое текстового файла
        /// </summary>
        /// <param name="path">Путь к файлу для чтения</param>
        /// <returns>Task со строкой содержимого файла</returns>
        /// <exception cref="FileNotFoundException">Файл не существует</exception>
        /// <exception cref="IOException">Ошибка ввода-вывода</exception>
        public async Task<string> ReadAllTextAsync(string path)
        {
            try
            {
                Log.Debug("Начало чтения файла: {Path}", path);
                var result = await File.ReadAllTextAsync(path);
                Log.Debug("Файл успешно прочитан: {Path}", path);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка чтения файла: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно записывает текст в файл
        /// </summary>
        /// <param name="path">Путь к файлу для записи</param>
        /// <returns>Task представляющий асинхронную операцию</returns>
        /// <exception cref="UnauthorizedAccessException">Нет прав на запись</exception>
        /// <exception cref="DirectoryNotFoundException">Директория не существует</exception>
        public Stream OpenRead(string path)
        {
            try
            {
                Log.Debug("Открытие файла для чтения: {Path}", path);
                var stream = File.OpenRead(path);
                Log.Debug("Файл открыт для чтения: {Path}", path);
                return stream;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка открытия файла для чтения: {Path}", path);
                throw;
            }
        }

        public async Task WriteAllTextAsync(string path, string contents)
        {
            try
            {
                Log.Debug("Начало записи файла: {Path}", path);
                await File.WriteAllTextAsync(path, contents);
                Log.Debug("Файл успешно записан: {Path}", path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка записи файла: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно создает резервную копию файла
        /// </summary>
        /// <param name="sourcePath">Путь к исходному файлу</param>
        /// <param name="backupPath">Путь для резервной копии</param>
        /// <returns>Task представляющий асинхронную операцию</returns>
        /// <exception cref="FileNotFoundException">Исходный файл не существует</exception>
        /// <exception cref="IOException">Ошибка при копировании</exception>
        public async Task CreateBackupAsync(string sourcePath, string backupPath)
        {
            try
            {
                Log.Debug("Начало создания резервной копии: {SourcePath} -> {BackupPath}", sourcePath, backupPath);
                if (FileExists(sourcePath))
                {
                    DeleteFileIfExists(backupPath);
                    using (var sourceStream = File.OpenRead(sourcePath))
                    using (var backupStream = File.Create(backupPath))
                    {
                        await sourceStream.CopyToAsync(backupStream);
                    }
                    Log.Information("Создана резервная копия: {BackupPath}", backupPath);
                }
                Log.Debug("Резервная копия завершена: {SourcePath} -> {BackupPath}", sourcePath, backupPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка создания резервной копии: {BackupPath}", backupPath);
                throw;
            }
        }

        /// <summary>
        /// Удаляет файл, если он существует
        /// </summary>
        /// <param name="path">Путь к файлу для удаления</param>
        /// <exception cref="UnauthorizedAccessException">Нет прав на удаление</exception>
        /// <exception cref="IOException">Файл используется другим процессом</exception>
        public void DeleteFileIfExists(string path)
        {
            try
            {
                if (FileExists(path))
                {
                    File.Delete(path);
                    Log.Information("Файл удален: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка удаления файла: {Path}", path);
                throw;
            }
        }
    }
}