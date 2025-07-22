// ВНИМАНИЕ: Данный репозиторий для коллекций (массивов) на данный момент не используется в проекте.
// Оставлен для возможного будущего расширения (например, если потребуется хранить списки объектов).

namespace ClockWidgetApp.Data
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using ClockWidgetApp.Services;
    using Serilog;

    /// <summary>
    /// Репозиторий для работы с JSON-файлами коллекций (массивов).
    /// <para>ВНИМАНИЕ: На данный момент не используется в проекте. Оставлен для возможного будущего расширения.</para>
    /// </summary>
    /// <typeparam name="T">Тип сохраняемой модели</typeparam>
    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly IFileSystemService _fileSystem;
        private readonly string _filePath;
        private readonly string _backupPath;
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonRepository(IFileSystemService fileSystem, string filePath)
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

        /// <inheritdoc/>
        /// <summary>
        /// Асинхронно возвращает все элементы из основного файла. Если основной файл повреждён, пробует восстановить из резервной копии (.bak).
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Сначала пробуем основной файл
            if (_fileSystem.FileExists(_filePath))
            {
                try
                {
                    using var stream = _fileSystem.OpenRead(_filePath);
                    return await JsonSerializer.DeserializeAsync<IEnumerable<T>>(
                        stream,
                        _serializerOptions) ?? Enumerable.Empty<T>();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка чтения основного файла {FilePath}, пробуем резервную копию", _filePath);
                }
            }

            // Если основной файл невалиден или отсутствует, пробуем .bak
            if (_fileSystem.FileExists(_backupPath))
            {
                try
                {
                    Log.Warning("Используется резервная копия для {FilePath}", _filePath);
                    using var stream = _fileSystem.OpenRead(_backupPath);
                    return await JsonSerializer.DeserializeAsync<IEnumerable<T>>(
                        stream,
                        _serializerOptions) ?? Enumerable.Empty<T>();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка чтения резервной копии {BackupPath}", _backupPath);
                }
            }

            // Если ничего не удалось — возвращаем пустую коллекцию
            return [];
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(string id)
        {
            var all = await GetAllAsync();
            // Для WidgetSettings просто возвращаем первый элемент
            return all.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task AddAsync(T entity)
        {
            var all = (await GetAllAsync()).ToList();
            all.Add(entity);
            await SaveAllAsync(all);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(T entity)
        {
            var all = (await GetAllAsync()).ToList();
            // Для WidgetSettings просто заменяем весь список
            all.Clear();
            all.Add(entity);
            await SaveAllAsync(all);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            var all = (await GetAllAsync()).ToList();
            // Для WidgetSettings просто очищаем
            all.Clear();
            await SaveAllAsync(all);
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            // Для JSON репозитория изменения сохраняются сразу,
            // поэтому этот метод не требует реализации
            await Task.CompletedTask;
        }

        /// <summary>
        /// Сохраняет все элементы в файл JSON, с резервным копированием и логированием.
        /// </summary>
        /// <param name="items">Коллекция для сохранения</param>
        private async Task SaveAllAsync(IEnumerable<T> items)
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    _fileSystem.CreateDirectory(dir);
                }

                // Создаем резервную копию если файл существует
                if (_fileSystem.FileExists(_filePath))
                {
                    await _fileSystem.CreateBackupAsync(_filePath, _backupPath);
                }

                var json = JsonSerializer.Serialize(items, _serializerOptions);
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