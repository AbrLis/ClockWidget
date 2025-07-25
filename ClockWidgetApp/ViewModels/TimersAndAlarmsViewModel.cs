using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// Агрегирующий ViewModel для вкладки "Будильники и таймеры". Делегирует работу специализированным ViewModel и сервисам.
/// </summary>
public class TimersAndAlarmsViewModel : INotifyPropertyChanged
{
    #region Private fields
    private readonly IAppDataService _appDataService;
    private readonly AlarmMonitorService _alarmMonitorService;
    private readonly TrayIconManager _trayIconManager;
    private readonly System.Windows.Threading.DispatcherTimer _trayUpdateTimer;

    #endregion

    #region Public properties
    /// <summary>
    /// ViewModel для управления таймерами.
    /// </summary>
    public TimersViewModel TimersVm { get; }
    /// <summary>
    /// ViewModel для управления будильниками.
    /// </summary>
    public AlarmsViewModel AlarmsVm { get; }
    /// <summary>
    /// ViewModel для управления длинными таймерами.
    /// </summary>
    public LongTimersViewModel LongTimersVm { get; }
    /// <summary>
    /// Локализованные строки для UI.
    /// </summary>
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();
    #endregion

    #region Constructor
    public TimersAndAlarmsViewModel(IAppDataService appDataService, ISoundService soundService, TrayIconManager trayIconManager)
    {
        _appDataService = appDataService;
        TimersVm = new TimersViewModel(appDataService);
        TimersVm.OnTimerTrayStateChanged = (timer, isRunning) =>
        {
            if (isRunning)
                AddTimerTray(timer);
            else
                RemoveTimerTray(timer);
        };
        AlarmsVm = new AlarmsViewModel(appDataService);
        LongTimersVm = new LongTimersViewModel(appDataService, soundService);
        _alarmMonitorService = new AlarmMonitorService(AlarmsVm.Alarms);
        _alarmMonitorService.AlarmTriggered += OnAlarmTriggered;
        _trayIconManager = trayIconManager;
        _trayIconManager.StopRequested += OnTrayIconStopRequested;
        TimersVm.TimerEntries.CollectionChanged += Timers_CollectionChanged;
        AlarmsVm.Alarms.CollectionChanged += Alarms_CollectionChanged;
        foreach (var longTimer in LongTimersVm.LongTimers)
        {
            longTimer.RequestExpire += OnLongTimerExpired;
        }
        UpdateLongTimersTrayIcon();
        LongTimersVm.LongTimers.CollectionChanged += LongTimers_CollectionChanged;
        _trayUpdateTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _trayUpdateTimer.Tick += (_, _) => UpdateTrayTooltips();
        _trayUpdateTimer.Start();
        LocalizationManager.LanguageChanged += (_, _) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
        // После инициализации _trayIconManager подписываем все будильники и добавляем иконки для активных
        foreach (var alarm in AlarmsVm.Alarms)
        {
            SubscribeAlarm(alarm);
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Сохраняет таймеры, длинные таймеры и будильники (асинхронно).
    /// </summary>
    public async Task SaveTimersAndAlarmsAsync()
    {
        Serilog.Log.Information("[TimersAndAlarmsViewModel] Сохранение timers/alarms...");
        await _appDataService.SaveAsync();
        Serilog.Log.Information("[TimersAndAlarmsViewModel] Сохранение timers/alarms завершено.");
    }

    /// <summary>
    /// Отменяет все отложенные автосохранения и немедленно сохраняет все данные приложения.
    /// </summary>
    public async Task FlushPendingSavesAsync()
    {
        if (_appDataService is AppDataService concreteService)
            await concreteService.FlushPendingSavesAsync();
        else
            await _appDataService.SaveAsync();
    }

    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion

    #region Private methods
    /// <summary>
    /// Обновляет тултипы всех иконок трея.
    /// </summary>
    private void UpdateTrayTooltips()
    {
        foreach (var timer in TimersVm.TimerEntries)
        {
            if (timer.IsRunning)
                _trayIconManager.UpdateTooltip(GetTimerId(timer), timer.DisplayTime);
        }
        foreach (var alarm in AlarmsVm.Alarms)
        {
            if (alarm.IsEnabled && alarm.NextTriggerDateTime.HasValue)
            {
                var left = alarm.NextTriggerDateTime.Value - DateTime.Now;
                string text = left > TimeSpan.Zero ? left.ToString(@"hh\:mm\:ss") : "00:00:00";
                _trayIconManager.UpdateTooltip(GetAlarmId(alarm), text);
            }
        }
        // Для длинных таймеров обновляем только одну иконку
        UpdateLongTimersTrayIcon();
    }

    #region Collection event handlers
    /// <summary>
    /// Обработчик изменений коллекции таймеров. Помечает таймеры/будильники как изменённые.
    /// </summary>
    private void Timers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Не логируем и не трогаем иконки при перемещении (Move)
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            return;
        if (e.NewItems != null)
            foreach (TimerEntryViewModel t in e.NewItems)
            {
                Serilog.Log.Information($"[TimersAndAlarmsViewModel] Добавлен таймер: {t.Duration}");
            }
        if (e.OldItems != null)
            foreach (TimerEntryViewModel t in e.OldItems)
            {
                RemoveTimerTray(t);
                Serilog.Log.Information($"[TimersAndAlarmsViewModel] Удалён таймер: {t.Duration}");
            }
    }

    /// <summary>
    /// Обработчик изменений коллекции будильников. Помечает таймеры/будильники как изменённые.
    /// </summary>
    private void Alarms_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (AlarmEntryViewModel a in e.NewItems)
            {
                SubscribeAlarm(a);
                Serilog.Log.Information($"[TimersAndAlarmsViewModel] Добавлен будильник: {a.AlarmTime}");
            }
        }

        if (e.OldItems != null)
        {
            foreach (AlarmEntryViewModel a in e.OldItems)
            {
                a.OnTrayStateChanged = null;
                RemoveAlarmTray(a);
                Serilog.Log.Information($"[TimersAndAlarmsViewModel] Удалён будильник: {a.AlarmTime}");
            }
        }
    }

    /// <summary>
    /// Обработчик изменений коллекции длинных таймеров. Помечает таймеры/будильники как изменённые.
    /// </summary>
    private void LongTimers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (LongTimerEntryViewModel t in e.NewItems)
            {
                t.RequestExpire += OnLongTimerExpired;
                Serilog.Log.Information($"[TimersAndAlarmsViewModel] Добавлен длинный таймер: {t.Name} ({t.TargetDateTime})");
            }
        }

        if (e.OldItems != null)
        {
            foreach (LongTimerEntryViewModel t in e.OldItems)
            {
                t.Dispose();
                Serilog.Log.Information($"[TimersAndAlarmsViewModel] Длинный таймер удалён из коллекции: {t.Name} ({t.TargetDateTime})");
            }
        }
        // После любого изменения коллекции обновляем иконку
        UpdateLongTimersTrayIcon();
    }
    #endregion

    #region Tray icon management
    private void AddTimerTray(TimerEntryViewModel timer)
    {
        string id = GetTimerId(timer);
        string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "timer.ico");
        _trayIconManager.AddOrUpdateTrayIcon(id, iconPath, timer.DisplayTime);
        Serilog.Log.Information($"[TimersAndAlarmsViewModel] Добавлена иконка трея для таймера: {id}");
    }
    private void RemoveTimerTray(TimerEntryViewModel timer)
    {
        _trayIconManager.RemoveTrayIcon(GetTimerId(timer));
        Serilog.Log.Information($"[TimersAndAlarmsViewModel] Удалена иконка трея для таймера: {timer.Duration}");
    }
    private void AddAlarmTray(AlarmEntryViewModel alarm)
    {
        if (alarm == null)
        {
            Serilog.Log.Error("[TimersAndAlarmsViewModel] AddAlarmTray: alarm is null");
            return;
        }
        if (_trayIconManager == null)
        {
            Serilog.Log.Error("[TimersAndAlarmsViewModel] AddAlarmTray: _trayIconManager is null");
            return;
        }
        var id = GetAlarmId(alarm);
        var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "alarm.ico");
        if (string.IsNullOrEmpty(iconPath) || !System.IO.File.Exists(iconPath))
        {
            Serilog.Log.Error($"[TimersAndAlarmsViewModel] AddAlarmTray: iconPath is invalid or file does not exist: {iconPath}");
            return;
        }
        var left = alarm.NextTriggerDateTime.HasValue ? alarm.NextTriggerDateTime.Value - DateTime.Now : TimeSpan.Zero;
        string text = left > TimeSpan.Zero ? left.ToString(@"hh\:mm\:ss") : "00:00:00";
        Serilog.Log.Debug($"[TimersAndAlarmsViewModel] AddAlarmTray: id={id}, iconPath={iconPath}, text={text}");
        _trayIconManager.AddOrUpdateTrayIcon(id, iconPath, text);
        Serilog.Log.Information($"[TimersAndAlarmsViewModel] Добавлена иконка трея для будильника: {id}");
    }
    private void RemoveAlarmTray(AlarmEntryViewModel alarm)
    {
        _trayIconManager.RemoveTrayIcon(GetAlarmId(alarm));
        Serilog.Log.Information($"[TimersAndAlarmsViewModel] Удалена иконка трея для будильника: {alarm.AlarmTime}");
    }
    #endregion

    #region Long timer event handlers
    /// <summary>
    /// Обработчик истечения длинного таймера: удаляет из коллекции и трея.
    /// </summary>
    private void OnLongTimerExpired(LongTimerEntryViewModel timer)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _ = RemoveAndPersistLongTimerAsync(timer);
        });
    }

    /// <summary>
    /// Асинхронно удаляет длинный таймер из всех коллекций и сохраняет изменения.
    /// </summary>
    private async Task RemoveAndPersistLongTimerAsync(LongTimerEntryViewModel timer)
    {
        LongTimersVm.LongTimers.Remove(timer);
        // Удаляем из persist-модели
        var persist = LongTimersVm.LongTimerModels.FirstOrDefault(m => m.TargetDateTime == timer.TargetDateTime && m.Name == timer.Name);
        if (persist != null)
            LongTimersVm.LongTimerModels.Remove(persist);
        // Сохраняем изменения
        await _appDataService.SaveAsync();
        Serilog.Log.Information($"[TimersAndAlarmsViewModel] Длинный таймер удалён после истечения: {timer.Name} ({timer.TargetDateTime})");
    }

    #endregion

    private void SubscribeAlarm(AlarmEntryViewModel alarm)
    {
        alarm.OnTrayStateChanged = (a, enabled) =>
        {
            if (enabled)
                AddAlarmTray(a);
            else
                RemoveAlarmTray(a);
        };
        // Если будильник включён при инициализации — отобразить иконку
        if (alarm.IsEnabled)
            alarm.OnTrayStateChanged.Invoke(alarm, true);
    }

    /// <summary>
    /// Генерирует уникальный идентификатор для таймера.
    /// </summary>
    private string GetTimerId(TimerEntryViewModel timer) => $"timer_{timer.GetHashCode()}";

    /// <summary>
    /// Генерирует уникальный идентификатор для будильника.
    /// </summary>
    private string GetAlarmId(AlarmEntryViewModel alarm) => $"alarm_{alarm.GetHashCode()}";

    /// <summary>
    /// Генерирует уникальный идентификатор для длинного таймера.
    /// </summary>
    private string GetLongTimerId(LongTimerEntryViewModel timer) => $"longtimer_{timer.GetHashCode()}";

    /// <summary>
    /// Обработка события StopRequested от TrayIconManager.
    /// </summary>
    private void OnTrayIconStopRequested(string id)
    {
        if (id.StartsWith("timer_"))
        {
            var timer = TimersVm.TimerEntries.FirstOrDefault(t => GetTimerId(t) == id);
            timer?.Reset();
        }
        else if (id.StartsWith("alarm_"))
        {
            var alarm = AlarmsVm.Alarms.FirstOrDefault(a => GetAlarmId(a) == id);
            alarm?.Stop();
        }
        else if (id.StartsWith("longtimer_"))
        {
            var longTimer = LongTimersVm.LongTimers.FirstOrDefault(t => GetLongTimerId(t) == id);
            if (longTimer != null)
            {
                LongTimersVm.LongTimers.Remove(longTimer);
            }
        }
    }

    /// <summary>
    /// Обрабатывает срабатывание будильника: проигрывает звук, показывает уведомление, сбрасывает состояние.
    /// </summary>
    private void OnAlarmTriggered(AlarmEntryViewModel alarm)
    {
        Serilog.Log.Information($"[TimersAndAlarmsViewModel] Сработал будильник: {alarm.AlarmTime}");
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var app = System.Windows.Application.Current as App;
            if (app?.Services is not { } services)
                return;
            if (services.GetService(typeof(ISoundService)) is not ISoundService soundService)
                return;
            var baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(baseDir))
                return;
            string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", "alarm.mp3");
            var soundHandle = soundService.PlaySoundInstance(soundPath, true);
            string dateTimeText = alarm.NextTriggerDateTime.HasValue
                ? alarm.NextTriggerDateTime.Value.ToString("dd.MM.yyyy") + " - " + alarm.NextTriggerDateTime.Value.ToString("HH:mm")
                : alarm.AlarmTime.ToString(@"hh\:mm");
            var notification = Views.TimerNotificationWindow.CreateWithCloseCallback(soundHandle, dateTimeText, "alarm");
            notification.Show();
            alarm.IsEnabled = false;
            alarm.ClearNextTriggerDateTime();
            alarm.OnPropertyChanged(nameof(alarm.IsEnabled));
        });
    }

    /// <summary>
    /// Генерирует тултип для ближайшего длинного таймера и краткую сводку о количестве остальных.
    /// </summary>
    private string GetLongTimersTooltip()
    {
        int count = LongTimersVm.LongTimers.Count;
        if (count == 0)
            return string.Empty;
        // Находим ближайший по времени длинный таймер
        var nextTimer = LongTimersVm.LongTimers
            .OrderBy(t => t.TargetDateTime)
            .FirstOrDefault();
        if (nextTimer == null)
            return string.Empty;
        // Формируем тултип: ближайший таймер + сводка
        string tooltip = nextTimer.TrayTooltip;
        if (count <= 1) return tooltip;
        string moreLabel = LocalizationManager.GetString("LongTimers_Tooltip_More");
        tooltip += $"\n+ {count - 1} {moreLabel}";
        return tooltip;
    }

    /// <summary>
    /// Обновляет или удаляет единую иконку длинных таймеров в трее.
    /// </summary>
    private void UpdateLongTimersTrayIcon()
    {
        const string id = "longtimers";
        var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "long.ico");
        var tooltip = GetLongTimersTooltip();
        if (LongTimersVm.LongTimers.Count == 0)
        {
            _trayIconManager.RemoveTrayIcon(id);
        }
        else
        {
            _trayIconManager.AddOrUpdateTrayIcon(id, iconPath, tooltip);
        }
    }

    #endregion
}