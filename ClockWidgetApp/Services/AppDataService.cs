namespace ClockWidgetApp.Services
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Text.Json;
    using ClockWidgetApp.Models;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ComponentModel;

    /// <summary>
    /// Сервис управления всеми данными приложения: настройки, таймеры, будильники, длинные таймеры.
    /// Хранит данные в памяти, обеспечивает загрузку/сохранение, резервное копирование и автосохранение.
    /// </summary>
    public class AppDataService : IAppDataService
    {
        #region Private Fields
        /// <summary>Путь к файлу настроек виджета.</summary>
        private readonly string _settingsFilePath;
        /// <summary>Путь к файлу таймеров и будильников.</summary>
        private readonly string _timersFilePath;
        /// <summary>Сервис работы с файловой системой.</summary>
        private readonly IFileSystemService _fileSystemService;
        /// <summary>Репозиторий для таймеров и будильников.</summary>
        private readonly Data.SingleObjectJsonRepository<TimersAndAlarmsPersistModel> _timersRepository;
        private CancellationTokenSource? _settingsSaveDebounceCts;
        private CancellationTokenSource? _timersSaveDebounceCts;
        private static readonly TimeSpan AutoSaveDebounceDelay = TimeSpan.FromSeconds(2);
        #endregion

        #region Events
        /// <inheritdoc/>
        public event EventHandler? SettingsChanged;
        /// <inheritdoc/>
        public event EventHandler? TimersChanged;
        /// <inheritdoc/>
        public event EventHandler? AlarmsChanged;
        /// <inheritdoc/>
        public event EventHandler? LongTimersChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// Создаёт сервис с указанными путями к файлам данных.
        /// </summary>
        public AppDataService(string settingsFilePath, string timersFilePath, IFileSystemService fileSystemService)
        {
            _settingsFilePath = settingsFilePath;
            _timersFilePath = timersFilePath;
            _fileSystemService = fileSystemService;
            _timersRepository = new Data.SingleObjectJsonRepository<TimersAndAlarmsPersistModel>(fileSystemService, timersFilePath);
            SubscribeToCollections();
            // Удаляю метод StartAutoSaveTimer
        }
        #endregion

        #region Public Properties
        /// <inheritdoc/>
        public AppDataModel Data { get; private set; } = new AppDataModel();
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public void NotifySettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            Serilog.Log.Debug("[AppDataService] Уведомление об изменении настроек");
        }

        /// <inheritdoc/>
        public void Load()
        {
            Serilog.Log.Information("[AppDataService] Начата загрузка данных приложения");
            var settings = LoadWithBackup<WidgetSettings>(_settingsFilePath);
            if (settings != null)
            {
                Data.WidgetSettings = settings;
                SubscribeToWidgetSettings();
                Serilog.Log.Information("[AppDataService] Настройки успешно загружены");
            }
            else
            {
                Serilog.Log.Warning("[AppDataService] Не удалось загрузить настройки — используются значения по умолчанию");
            }

            try
            {
                // Используем репозиторий одиночного объекта для загрузки timers/alarms
                var timersModel = _timersRepository.LoadAsync().GetAwaiter().GetResult();
                if (timersModel != null)
                {
                    CopyCollections(timersModel);
                    Serilog.Log.Information("[AppDataService] Таймеры и будильники успешно загружены (репозиторий)");
                }
                else
                {
                    Serilog.Log.Warning("[AppDataService] Не удалось загрузить таймеры/будильники — используются значения по умолчанию");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AppDataService] Ошибка загрузки timers/alarms из репозитория");
            }

            SubscribeToCollections();
        }

        /// <inheritdoc/>
        public async Task LoadAsync()
        {
            Serilog.Log.Information("[AppDataService] Начата асинхронная загрузка данных приложения");
            var settings = await LoadWithBackupAsync<WidgetSettings>(_settingsFilePath);
            if (settings != null)
            {
                Data.WidgetSettings = settings;
                SubscribeToWidgetSettings();
                Serilog.Log.Information("[AppDataService] Настройки успешно загружены");
            }
            else
            {
                Serilog.Log.Warning("[AppDataService] Не удалось загрузить настройки — используются значения по умолчанию");
            }

            try
            {
                // Используем репозиторий одиночного объекта для загрузки timers/alarms
                var timersModel = await _timersRepository.LoadAsync();
                if (timersModel != null)
                {
                    CopyCollections(timersModel);
                    Serilog.Log.Information("[AppDataService] Таймеры и будильники успешно загружены (репозиторий)");
                }
                else
                {
                    Serilog.Log.Warning("[AppDataService] Не удалось загрузить таймеры/будильники — используются значения по умолчанию");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AppDataService] Ошибка загрузки timers/alarms из репозитория");
            }

            SubscribeToCollections();
        }

        /// <inheritdoc/>
        public void Save()
        {
            Serilog.Log.Information("[AppDataService] Начато сохранение данных приложения");
            SaveSettings();
            SaveTimersAndAlarms();
        }

        /// <inheritdoc/>
        public async Task SaveAsync()
        {
            Serilog.Log.Information("[AppDataService] Начато асинхронное сохранение данных приложения");
            await SaveSettingsAsync();
            await SaveTimersAndAlarmsAsync();
            Serilog.Log.Information("[AppDataService] Асинхронное сохранение данных приложения завершено");
        }

        /// <summary>
        /// Отменяет все отложенные автосохранения и немедленно сохраняет все данные приложения.
        /// </summary>
        public async Task FlushPendingSavesAsync()
        {
            _settingsSaveDebounceCts?.Cancel();
            _timersSaveDebounceCts?.Cancel();
            await SaveAsync();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Планирует автосохранение только настроек с debounce.
        /// </summary>
        private void ScheduleSettingsSave()
        {
            _settingsSaveDebounceCts?.Cancel();
            _settingsSaveDebounceCts = new CancellationTokenSource();
            var token = _settingsSaveDebounceCts.Token;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(AutoSaveDebounceDelay, token);
                    await SaveSettingsAsync();
                }
                catch (TaskCanceledException) { }
            }, token);
        }

        /// <summary>
        /// Планирует автосохранение только timers/alarms с debounce.
        /// </summary>
        private void ScheduleTimersAndAlarmsSave()
        {
            _timersSaveDebounceCts?.Cancel();
            _timersSaveDebounceCts = new CancellationTokenSource();
            var token = _timersSaveDebounceCts.Token;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(AutoSaveDebounceDelay, token);
                    await SaveTimersAndAlarmsAsync();
                }
                catch (TaskCanceledException) { }
            }, token);
        }

        /// <summary>
        /// Подписывает коллекции и настройки на события изменения для автосохранения.
        /// </summary>
        private void SubscribeToCollections()
        {
            Data.Timers.CollectionChanged += (s, e) => { OnCollectionChanged(s, e); ScheduleTimersAndAlarmsSave(); };
            Data.Alarms.CollectionChanged += (s, e) => { OnCollectionChanged(s, e); ScheduleTimersAndAlarmsSave(); };
            Data.LongTimers.CollectionChanged += (s, e) => { OnCollectionChanged(s, e); ScheduleTimersAndAlarmsSave(); };
            SubscribeToWidgetSettings();
        }

        private void SubscribeToWidgetSettings()
        {
            if (Data.WidgetSettings is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged -= WidgetSettings_PropertyChanged;
                npc.PropertyChanged += WidgetSettings_PropertyChanged;
            }
        }

        private void WidgetSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ScheduleSettingsSave();
        }

        /// <summary>
        /// Обработчик изменения коллекций таймеров/будильников/длинных таймеров.
        /// </summary>
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == Data.Timers)
                TimersChanged?.Invoke(this, EventArgs.Empty);
            else if (sender == Data.Alarms)
                AlarmsChanged?.Invoke(this, EventArgs.Empty);
            else if (sender == Data.LongTimers)
                LongTimersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Сериализует объект в файл JSON с форматированием.
        /// </summary>
        private async Task SerializeToFileAsync<T>(string filePath, T obj)
        {
            Serilog.Log.Debug($"[AppDataService] Начало сериализации и записи файла: {filePath}");
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            await _fileSystemService.WriteAllTextAsync(filePath, json);
            Serilog.Log.Debug($"[AppDataService] Файл успешно сериализован и записан: {filePath}");
        }

        /// <summary>
        /// Десериализует объект из файла JSON.
        /// </summary>
        private async Task<T?> DeserializeFromFileAsync<T>(string filePath) where T : class
        {
            var json = await _fileSystemService.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Создаёт резервную копию файла, если он валиден (может быть десериализован).
        /// </summary>
        private async Task CreateBackupIfValidAsync<T>(string filePath, string backupPath) where T : class
        {
            Serilog.Log.Debug($"[AppDataService] Проверка валидности файла для бэкапа: {filePath}");
            try
            {
                var obj = await DeserializeFromFileAsync<T>(filePath);
                if (obj != null)
                {
                    Serilog.Log.Debug($"[AppDataService] Файл валиден, создаём бэкап: {backupPath}");
                    await _fileSystemService.CreateBackupAsync(filePath, backupPath);
                    Serilog.Log.Debug($"[AppDataService] Бэкап успешно создан: {backupPath}");
                }
                else
                {
                    Serilog.Log.Warning($"[AppDataService] Текущий файл повреждён, .bak не обновляется: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, $"[AppDataService] Ошибка при проверке валидности файла, .bak не обновляется: {filePath}");
            }
        }

        /// <summary>
        /// Загружает объект из файла, при ошибке пробует резервную копию.
        /// </summary>
        private T? LoadWithBackup<T>(string filePath) where T : class, new()
        {
            if (_fileSystemService.FileExists(filePath))
            {
                try
                {
                    var json = _fileSystemService.ReadAllTextAsync(filePath).GetAwaiter().GetResult();
                    var obj = JsonSerializer.Deserialize<T>(json);
                    if (obj != null)
                        return obj;
                }
                catch (System.Exception ex)
                {
                    Serilog.Log.Error(ex, $"[AppDataService] Ошибка чтения файла {filePath}, попытка использовать резервную копию");
                }
            }
            string backupPath = Path.ChangeExtension(filePath, ".bak");
            if (_fileSystemService.FileExists(backupPath))
            {
                try
                {
                    var json = _fileSystemService.ReadAllTextAsync(backupPath).GetAwaiter().GetResult();
                    var obj = JsonSerializer.Deserialize<T>(json);
                    if (obj != null)
                    {
                        Serilog.Log.Warning($"[AppDataService] Использована резервная копия для файла {filePath}");
                        return obj;
                    }
                }
                catch (System.Exception ex)
                {
                    Serilog.Log.Error(ex, $"[AppDataService] Ошибка чтения резервной копии {backupPath}");
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронно загружает объект из файла, при ошибке пробует резервную копию.
        /// </summary>
        private async Task<T?> LoadWithBackupAsync<T>(string filePath) where T : class, new()
        {
            if (_fileSystemService.FileExists(filePath))
            {
                try
                {
                    var obj = await DeserializeFromFileAsync<T>(filePath);
                    if (obj != null)
                        return obj;
                }
                catch (System.Exception ex)
                {
                    Serilog.Log.Error(ex, $"[AppDataService] Ошибка чтения файла {filePath}, попытка использовать резервную копию");
                }
            }
            string backupPath = Path.ChangeExtension(filePath, ".bak");
            if (_fileSystemService.FileExists(backupPath))
            {
                try
                {
                    var obj = await DeserializeFromFileAsync<T>(backupPath);
                    if (obj != null)
                    {
                        Serilog.Log.Warning($"[AppDataService] Использована резервная копия для файла {filePath}");
                        return obj;
                    }
                }
                catch (System.Exception ex)
                {
                    Serilog.Log.Error(ex, $"[AppDataService] Ошибка чтения резервной копии {backupPath}");
                }
            }
            return null;
        }

        /// <summary>
        /// Обновляет содержимое целевой коллекции, минимизируя количество изменений (удаляет, добавляет и обновляет только отличающиеся элементы).
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <param name="target">Целевая коллекция (ObservableCollection)</param>
        /// <param name="source">Исходная коллекция (List)</param>
        /// <param name="equals">Функция сравнения элементов</param>
        private void UpdateCollection<T>(System.Collections.ObjectModel.ObservableCollection<T> target, List<T> source, Func<T, T, bool> equals)
        {
            // Удаляем отсутствующие
            for (int i = target.Count - 1; i >= 0; i--)
            {
                if (!source.Any(s => equals(s, target[i])))
                    target.RemoveAt(i);
            }
            // Добавляем и обновляем
            for (int i = 0; i < source.Count; i++)
            {
                var src = source[i];
                var existing = target.FirstOrDefault(t => equals(t, src));
                if (existing == null)
                {
                    target.Insert(i, src);
                }
                else
                {
                    // Если элемент на другой позиции — переместить
                    int oldIndex = target.IndexOf(existing);
                    if (oldIndex != i)
                        target.Move(oldIndex, i);
                    // Если требуется глубокое обновление — реализовать здесь
                }
            }
        }

        /// <summary>
        /// Копирует коллекции таймеров, будильников и длинных таймеров из persist-модели в Data с оптимизацией и расширенным логированием.
        /// </summary>
        private void CopyCollections(TimersAndAlarmsPersistModel model)
        {
            try
            {
                int oldTimers = Data.Timers.Count;
                int oldAlarms = Data.Alarms.Count;
                int oldLongTimers = Data.LongTimers.Count;

                UpdateCollection(Data.Timers, model.Timers, (a, b) => a.Duration == b.Duration);
                UpdateCollection(Data.Alarms, model.Alarms, (a, b) => a.AlarmTime == b.AlarmTime && a.IsEnabled == b.IsEnabled && a.NextTriggerDateTime == b.NextTriggerDateTime);
                UpdateCollection(Data.LongTimers, model.LongTimers, (a, b) => a.Name == b.Name && a.TargetDateTime == b.TargetDateTime);

                Serilog.Log.Information($"[AppDataService] Коллекции обновлены: Timers {oldTimers}->{Data.Timers.Count}, Alarms {oldAlarms}->{Data.Alarms.Count}, LongTimers {oldLongTimers}->{Data.LongTimers.Count}");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AppDataService] Ошибка при обновлении коллекций Timers/Alarms/LongTimers");
            }
        }

        /// <summary>
        /// Сохраняет настройки виджета с резервным копированием и логированием.
        /// </summary>
        private void SaveSettings()
        {
            var settingsDir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(settingsDir))
                _fileSystemService.CreateDirectory(settingsDir);

            string backupPath = Path.ChangeExtension(_settingsFilePath, ".bak");
            if (_fileSystemService.FileExists(_settingsFilePath))
            {
                CreateBackupIfValidAsync<WidgetSettings>(_settingsFilePath, backupPath).GetAwaiter().GetResult();
            }

            try
            {
                SerializeToFileAsync(_settingsFilePath, Data.WidgetSettings).GetAwaiter().GetResult();
                Serilog.Log.Information($"[AppDataService] Настройки успешно сохранены: {_settingsFilePath}");

                bool backupAfterSave = _fileSystemService.FileExists(_settingsFilePath) && !_fileSystemService.FileExists(backupPath);
                if (backupAfterSave)
                {
                    _fileSystemService.CreateBackupAsync(_settingsFilePath, backupPath).GetAwaiter().GetResult();
                }
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error(ex, $"[AppDataService] Ошибка сохранения настроек: {_settingsFilePath}");
            }
        }

        /// <summary>
        /// Асинхронно сохраняет настройки виджета с резервным копированием и логированием.
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            var settingsDir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(settingsDir))
                _fileSystemService.CreateDirectory(settingsDir);

            string backupPath = Path.ChangeExtension(_settingsFilePath, ".bak");
            if (_fileSystemService.FileExists(_settingsFilePath))
            {
                Serilog.Log.Debug($"[AppDataService] Перед созданием бэкапа настроек: {backupPath}");
                await CreateBackupIfValidAsync<WidgetSettings>(_settingsFilePath, backupPath);
                Serilog.Log.Debug($"[AppDataService] После создания бэкапа настроек: {backupPath}");
            }

            try
            {
                Serilog.Log.Debug($"[AppDataService] Перед сериализацией настроек: {_settingsFilePath}");
                await SerializeToFileAsync(_settingsFilePath, Data.WidgetSettings);
                Serilog.Log.Debug($"[AppDataService] После сериализации настроек: {_settingsFilePath}");

                bool backupAfterSave = _fileSystemService.FileExists(_settingsFilePath) && !_fileSystemService.FileExists(backupPath);
                if (backupAfterSave)
                {
                    Serilog.Log.Debug($"[AppDataService] Перед созданием бэкапа после сохранения: {backupPath}");
                    await _fileSystemService.CreateBackupAsync(_settingsFilePath, backupPath);
                    Serilog.Log.Debug($"[AppDataService] После создания бэкапа после сохранения: {backupPath}");
                }
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error(ex, $"[AppDataService] Ошибка сохранения настроек: {_settingsFilePath}");
            }
        }

        /// <summary>
        /// Сохраняет таймеры и будильники с резервным копированием и логированием.
        /// </summary>
        private void SaveTimersAndAlarms()
        {
            try
            {
                var timersModel = new TimersAndAlarmsPersistModel
                {
                    Timers = new System.Collections.Generic.List<TimerPersistModel>(Data.Timers),
                    Alarms = new System.Collections.Generic.List<AlarmPersistModel>(Data.Alarms),
                    LongTimers = new System.Collections.Generic.List<LongTimerPersistModel>(Data.LongTimers)
                };
                _timersRepository.SaveAsync(timersModel).GetAwaiter().GetResult();
                Serilog.Log.Information($"[AppDataService] Таймеры и будильники успешно сохранены (репозиторий): {_timersFilePath}");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"[AppDataService] Ошибка сохранения timers/alarms в репозиторий: {_timersFilePath}");
            }
        }

        /// <summary>
        /// Асинхронно сохраняет таймеры и будильники с резервным копированием и логированием.
        /// </summary>
        private async Task SaveTimersAndAlarmsAsync()
        {
            try
            {
                var timersModel = new TimersAndAlarmsPersistModel
                {
                    Timers = new System.Collections.Generic.List<TimerPersistModel>(Data.Timers),
                    Alarms = new System.Collections.Generic.List<AlarmPersistModel>(Data.Alarms),
                    LongTimers = new System.Collections.Generic.List<LongTimerPersistModel>(Data.LongTimers)
                };
                Serilog.Log.Debug($"[AppDataService] Перед сохранением timers/alarms: {_timersFilePath}");
                await _timersRepository.SaveAsync(timersModel);
                Serilog.Log.Debug($"[AppDataService] После сохранения timers/alarms: {_timersFilePath}");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"[AppDataService] Ошибка сохранения timers/alarms в репозиторий: {_timersFilePath}");
            }
        }
        #endregion
    }
}