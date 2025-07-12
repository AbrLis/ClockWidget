using System.IO;
using System.Text.Json;
using ClockWidgetApp.Models;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace ClockWidgetApp.Services
{
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
        /// <summary>Таймер для автосохранения при dirty-флаге.</summary>
        private DispatcherTimer? _autoSaveTimer;
        #endregion

        #region Constructor
        /// <summary>
        /// Создаёт сервис с указанными путями к файлам данных.
        /// </summary>
        public AppDataService(string settingsFilePath, string timersFilePath)
        {
            _settingsFilePath = settingsFilePath;
            _timersFilePath = timersFilePath;
            SubscribeToCollections();
            StartAutoSaveTimer();
        }
        #endregion

        #region Public Properties
        /// <inheritdoc/>
        public AppDataModel Data { get; private set; } = new AppDataModel();
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public void Load()
        {
            Serilog.Log.Information("[AppDataService] Начата загрузка данных приложения");
            var settings = LoadWithBackup<WidgetSettings>(_settingsFilePath);
            if (settings != null)
            {
                Data.WidgetSettings = settings;
                Serilog.Log.Information("[AppDataService] Настройки успешно загружены");
            }
            else
            {
                Serilog.Log.Warning("[AppDataService] Не удалось загрузить настройки — используются значения по умолчанию");
            }

            var timersModel = LoadWithBackup<TimersAndAlarmsPersistModel>(_timersFilePath);
            if (timersModel != null)
            {
                CopyCollections(timersModel);
                Serilog.Log.Information("[AppDataService] Таймеры и будильники успешно загружены");
            }
            else
            {
                Serilog.Log.Warning("[AppDataService] Не удалось загрузить таймеры/будильники — используются значения по умолчанию");
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Запускает таймер автосохранения, который каждую минуту сохраняет данные, если dirty-флаг выставлен.
        /// </summary>
        private void StartAutoSaveTimer()
        {
            _autoSaveTimer = new DispatcherTimer { Interval = System.TimeSpan.FromMinutes(1) };
            _autoSaveTimer.Tick += (s, e) =>
            {
                bool needSave = false;
                if (System.Threading.Interlocked.Exchange(ref ClockWidgetApp.App._widgetSettingsDirty, 0) == 1)
                    needSave = true;
                if (System.Threading.Interlocked.Exchange(ref ClockWidgetApp.App._timersAlarmsDirty, 0) == 1)
                    needSave = true;
                if (needSave)
                {
                    Serilog.Log.Information("[AppDataService] Автосохранение по dirty-флагу");
                    Save();
                }
            };
            _autoSaveTimer.Start();
        }

        /// <summary>
        /// Подписывает коллекции на события изменения для автосохранения.
        /// </summary>
        private void SubscribeToCollections()
        {
            Data.Timers.CollectionChanged += OnCollectionChanged;
            Data.Alarms.CollectionChanged += OnCollectionChanged;
            Data.LongTimers.CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        /// Обработчик изменения коллекций таймеров/будильников/длинных таймеров.
        /// </summary>
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ClockWidgetApp.App.MarkTimersAlarmsDirty();
        }

        /// <summary>
        /// Загружает объект из файла, при ошибке пробует резервную копию.
        /// </summary>
        private T? LoadWithBackup<T>(string filePath) where T : class, new()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
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
            if (File.Exists(backupPath))
            {
                try
                {
                    var json = File.ReadAllText(backupPath);
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
        /// Копирует коллекции таймеров, будильников и длинных таймеров из persist-модели в Data.
        /// </summary>
        private void CopyCollections(TimersAndAlarmsPersistModel model)
        {
            Data.Timers.Clear();
            foreach (var t in model.Timers)
                Data.Timers.Add(t);
            Data.Alarms.Clear();
            foreach (var a in model.Alarms)
                Data.Alarms.Add(a);
            Data.LongTimers.Clear();
            foreach (var l in model.LongTimers)
                Data.LongTimers.Add(l);
        }

        /// <summary>
        /// Сохраняет настройки виджета с резервным копированием и логированием.
        /// </summary>
        private void SaveSettings()
        {
            var settingsDir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(settingsDir) && !Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);
            if (File.Exists(_settingsFilePath))
            {
                string backupPath = Path.ChangeExtension(_settingsFilePath, ".bak");
                try
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(_settingsFilePath, backupPath);
                    Serilog.Log.Information($"[AppDataService] Создана резервная копия настроек: {backupPath}");
                }
                catch (IOException ex)
                {
                    Serilog.Log.Error(ex, $"[AppDataService] Ошибка создания резервной копии настроек: {backupPath}");
                }
            }
            try
            {
                var settingsJson = JsonSerializer.Serialize(Data.WidgetSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, settingsJson);
                Serilog.Log.Information($"[AppDataService] Настройки успешно сохранены: {_settingsFilePath}");
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
            var timersDir = Path.GetDirectoryName(_timersFilePath);
            if (!string.IsNullOrEmpty(timersDir) && !Directory.Exists(timersDir))
                Directory.CreateDirectory(timersDir);
            if (File.Exists(_timersFilePath))
            {
                string backupPath = Path.ChangeExtension(_timersFilePath, ".bak");
                try
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(_timersFilePath, backupPath);
                    Serilog.Log.Information($"[AppDataService] Создана резервная копия timers/alarms: {backupPath}");
                }
                catch (IOException ex)
                {
                    Serilog.Log.Error(ex, $"[AppDataService] Ошибка создания резервной копии timers/alarms: {backupPath}");
                }
            }
            try
            {
                var timersModel = new TimersAndAlarmsPersistModel
                {
                    Timers = new System.Collections.Generic.List<TimerPersistModel>(Data.Timers),
                    Alarms = new System.Collections.Generic.List<AlarmPersistModel>(Data.Alarms),
                    LongTimers = new System.Collections.Generic.List<LongTimerPersistModel>(Data.LongTimers)
                };
                var timersJson = JsonSerializer.Serialize(timersModel, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_timersFilePath, timersJson);
                Serilog.Log.Information($"[AppDataService] Таймеры и будильники успешно сохранены: {_timersFilePath}");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error(ex, $"[AppDataService] Ошибка сохранения timers/alarms: {_timersFilePath}");
            }
        }
        #endregion
    }
} 