using System.IO;
using System.Text.Json;
using ClockWidgetApp.Services;
using Serilog;

namespace ClockWidgetApp.Data
{
    /// <summary>
    /// Репозиторий для хранения и загрузки одиночного объекта в JSON-файле с поддержкой резервной копии (.bak).
    /// </summary>
    /// <typeparam name="T">Тип сохраняемой модели</typeparam>
    public class SingleObjectJsonRepository<T> where T : class
    {
        private readonly IFileSystemService _fileSystem;
        private readonly string _filePath;
        private readonly string _backupPath;
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Создаёт репозиторий для одиночного объекта.
        /// </summary>
        public SingleObjectJsonRepository(IFileSystemService fileSystem, string filePath)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _backupPath = Path.ChangeExtension(filePath, ".bak");
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Асинхронно загружает объект из основного файла. Если основной файл повреждён, пробует восстановить из резервной копии (.bak).
        /// </summary>
        public async Task<T?> LoadAsync()
        {
            // Пробуем основной файл
            if (_fileSystem.FileExists(_filePath))
            {
                try
                {
                    using var stream = _fileSystem.OpenRead(_filePath);
                    return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка чтения основного файла {FilePath}, пробуем резервную копию", _filePath);
                }
            }
            // Пробуем .bak
            if (_fileSystem.FileExists(_backupPath))
            {
                try
                {
                    Log.Warning("Используется резервная копия для {FilePath}", _filePath);
                    using var stream = _fileSystem.OpenRead(_backupPath);
                    return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка чтения резервной копии {BackupPath}", _backupPath);
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронно сохраняет объект в файл с созданием резервной копии.
        /// </summary>
        public async Task SaveAsync(T obj)
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                    _fileSystem.CreateDirectory(dir);
                // Создаём резервную копию, если файл существует
                if (_fileSystem.FileExists(_filePath))
                    await _fileSystem.CreateBackupAsync(_filePath, _backupPath);
                var json = JsonSerializer.Serialize(obj, _serializerOptions);
                await _fileSystem.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка сохранения данных в {FilePath}", _filePath);
                throw;
            }
        }
    }
}