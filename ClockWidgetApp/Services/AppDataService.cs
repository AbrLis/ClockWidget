namespace ClockWidgetApp.Services
{
    using Models;
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
        /// <summary>Репозиторий для настроек виджета.</summary>
        private readonly Data.SingleObjectJsonRepository<WidgetSettings> _settingsRepository;
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
            _settingsRepository = new Data.SingleObjectJsonRepository<WidgetSettings>(fileSystemService, settingsFilePath);
            _timersRepository = new Data.SingleObjectJsonRepository<TimersAndAlarmsPersistModel>(fileSystemService, timersFilePath);
            SubscribeToCollections();
            // Удаляю метод StartAutoSaveTimer
        }
        #endregion

        #region Public Properties
        /// <inheritdoc/>
        public AppDataModel Data { get; } = new();
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
            var settings = _settingsRepository.LoadAsync().GetAwaiter().GetResult();
            if (settings != null)
            {
                Data.WidgetSettings = settings;
                SubscribeToWidgetSettings();
                Serilog.Log.Information("[AppDataService] Настройки успешно загружены (репозиторий)");
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
            var settings = await _settingsRepository.LoadAsync();
            if (settings != null)
            {
                Data.WidgetSettings = settings;
                SubscribeToWidgetSettings();
                Serilog.Log.Information("[AppDataService] Настройки успешно загружены (репозиторий)");
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
            await _settingsSaveDebounceCts?.CancelAsync()!;
            await _timersSaveDebounceCts?.CancelAsync()!;
            await SaveAsync();
        }

        /// <summary>
        /// Синхронно отменяет все отложенные автосохранения и немедленно сохраняет все данные приложения.
        /// Используется при закрытии приложения для гарантированного сохранения.
        /// </summary>
        public void FlushPendingSaves()
        {
            try
            {
                Serilog.Log.Information("[AppDataService] Принудительное синхронное сохранение при закрытии приложения");
                
                // Отменяем отложенные сохранения
                _settingsSaveDebounceCts?.Cancel();
                _timersSaveDebounceCts?.Cancel();
                
                // Выполняем сохранение синхронно
                Save();
                
                Serilog.Log.Information("[AppDataService] Принудительное сохранение завершено");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AppDataService] Ошибка при принудительном сохранении");
            }
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
        public void ScheduleTimersAndAlarmsSave()
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
            Data.Timers.CollectionChanged += (s, _) => { OnCollectionChanged(s); ScheduleTimersAndAlarmsSave(); };
            Data.Alarms.CollectionChanged += (s, _) => { OnCollectionChanged(s); ScheduleTimersAndAlarmsSave(); };
            Data.LongTimers.CollectionChanged += (s, _) => { OnCollectionChanged(s); ScheduleTimersAndAlarmsSave(); };
            SubscribeToWidgetSettings();
        }

        private void SubscribeToWidgetSettings()
        {
            if (Data.WidgetSettings is not INotifyPropertyChanged npc) return;
            npc.PropertyChanged -= WidgetSettings_PropertyChanged;
            npc.PropertyChanged += WidgetSettings_PropertyChanged;
        }

        private void WidgetSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ScheduleSettingsSave();
        }

        /// <summary>
        /// Обработчик изменения коллекций таймеров/будильников/длинных таймеров.
        /// </summary>
        private void OnCollectionChanged(object? sender)
        {
            if (sender == Data.Timers)
                TimersChanged?.Invoke(this, EventArgs.Empty);
            else if (sender == Data.Alarms)
                AlarmsChanged?.Invoke(this, EventArgs.Empty);
            else if (sender == Data.LongTimers)
                LongTimersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Обновляет содержимое целевой коллекции, минимизируя количество изменений (удаляет, добавляет и обновляет только отличающиеся элементы).
        /// Гарантирует, что индексы не выходят за границы коллекции.
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
            for (var i = 0; i < source.Count; i++)
            {
                var src = source[i];
                var existing = target.FirstOrDefault(t => equals(t, src));
                if (existing == null)
                {
                    // Если индекс больше размера коллекции — добавляем в конец
                    if (i >= target.Count)
                        target.Add(src);
                    else
                        target.Insert(i, src);
                }
                else
                {
                    var oldIndex = target.IndexOf(existing);
                    // Перемещаем только если оба индекса валидны и отличаются
                    if (oldIndex != i && oldIndex >= 0 && i >= 0 && i < target.Count)
                    {
                        target.Move(oldIndex, i);
                    }
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
                var oldTimers = Data.Timers.Count;
                var oldAlarms = Data.Alarms.Count;
                var oldLongTimers = Data.LongTimers.Count;

                // Сравнение теперь по уникальному идентификатору Id
                UpdateCollection(Data.Timers, model.Timers, (a, b) => a.Id == b.Id);
                UpdateCollection(Data.Alarms, model.Alarms, (a, b) => a.Id == b.Id);
                UpdateCollection(Data.LongTimers, model.LongTimers, (a, b) => a.Id == b.Id);

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
            try
            {
                // Простой синхронный вызов без Task.Run
                _settingsRepository.SaveAsync(Data.WidgetSettings).GetAwaiter().GetResult();
                Serilog.Log.Information($"[AppDataService] Настройки успешно сохранены (репозиторий): {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"[AppDataService] Ошибка сохранения настроек: {_settingsFilePath}");
            }
        }

        /// <summary>
        /// Асинхронно сохраняет настройки виджета с резервным копированием и логированием.
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            try
            {
                await _settingsRepository.SaveAsync(Data.WidgetSettings);
                Serilog.Log.Debug($"[AppDataService] Настройки успешно сохранены (репозиторий): {_settingsFilePath}");
            }
            catch (Exception ex)
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
                    Timers = [..Data.Timers],
                    Alarms = [..Data.Alarms],
                    LongTimers = new List<LongTimerPersistModel>(Data.LongTimers)
                };
                // Простой синхронный вызов без Task.Run
                _timersRepository.SaveAsync(timersModel).GetAwaiter().GetResult();
                Serilog.Log.Information("[AppDataService] Таймеры и будильники успешно сохранены (репозиторий): {TimersFilePath}", _timersFilePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AppDataService] Ошибка сохранения timers/alarms в репозиторий: {TimersFilePath}", _timersFilePath);
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
                    Timers = [..Data.Timers],
                    Alarms = [..Data.Alarms],
                    LongTimers = new List<LongTimerPersistModel>(Data.LongTimers)
                };
                await _timersRepository.SaveAsync(timersModel);
                Serilog.Log.Information("[AppDataService] Асинхронное сохранение timers/alarms в репозиторий: {TimersFilePath}", _timersFilePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AppDataService] Ошибка сохранения timers/alarms в репозиторий: {TimersFilePath}", _timersFilePath);
            }
        }
        #endregion
    }
}