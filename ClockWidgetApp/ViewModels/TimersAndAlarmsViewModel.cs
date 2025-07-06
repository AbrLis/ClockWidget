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
    private readonly AlarmMonitorService _alarmMonitorService;
    private readonly TimersAndAlarmsPersistenceService _persistenceService;
    private readonly TrayIconManager _trayIconManager;
    private readonly System.Windows.Threading.DispatcherTimer _trayUpdateTimer;
    #endregion

    #region Public properties
    /// <summary>
    /// Singleton-экземпляр ViewModel для вкладки 'Будильники и таймеры'.
    /// </summary>
    public static TimersAndAlarmsViewModel Instance { get; } = new TimersAndAlarmsViewModel();

    /// <summary>
    /// ViewModel для управления таймерами.
    /// </summary>
    public TimersViewModel TimersVM { get; }
    /// <summary>
    /// ViewModel для управления будильниками.
    /// </summary>
    public AlarmsViewModel AlarmsVM { get; }
    /// <summary>
    /// Локализованные строки для UI.
    /// </summary>
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();
    #endregion

    #region Constructor
    public TimersAndAlarmsViewModel()
    {
        TimersVM = new TimersViewModel();
        AlarmsVM = new AlarmsViewModel();
        _alarmMonitorService = new AlarmMonitorService(AlarmsVM.Alarms);
        _alarmMonitorService.AlarmTriggered += OnAlarmTriggered;
        var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClockWidget", "timers_alarms.json");
        _persistenceService = new TimersAndAlarmsPersistenceService(filePath);
        _trayIconManager = ((App)System.Windows.Application.Current).TrayIconManager;
        _trayIconManager.StopRequested += OnTrayIconStopRequested;
        TimersVM.Timers.CollectionChanged += Timers_CollectionChanged;
        AlarmsVM.Alarms.CollectionChanged += Alarms_CollectionChanged;
        foreach (var timer in TimersVM.Timers)
            SubscribeTimer(timer);
        foreach (var alarm in AlarmsVM.Alarms)
            SubscribeAlarm(alarm);
        _trayUpdateTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _trayUpdateTimer.Tick += (s, e) => UpdateTrayTooltips();
        _trayUpdateTimer.Start();
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Сохраняет таймеры и будильники.
    /// </summary>
    public void SaveTimersAndAlarms()
    {
        var persist = new Models.TimersAndAlarmsPersistModel
        {
            Timers = TimersVM.Timers.Select(t => new Models.TimerPersistModel { Duration = t.Duration }).ToList(),
            Alarms = AlarmsVM.Alarms.Select(a => new Models.AlarmPersistModel { AlarmTime = a.AlarmTime, IsEnabled = a.IsEnabled, NextTriggerDateTime = a.NextTriggerDateTime }).ToList()
        };
        _persistenceService.Save(persist);
    }

    /// <summary>
    /// Загружает таймеры и будильники.
    /// </summary>
    public void LoadTimersAndAlarms()
    {
        var persist = _persistenceService.Load();
        if (persist == null) return;
        TimersVM.Timers.Clear();
        foreach (var t in persist.Timers)
        {
            var timer = new TimerEntryViewModel(t.Duration);
            timer.RequestDelete += tt => { tt.Dispose(); TimersVM.Timers.Remove(tt); };
            timer.RequestDeactivate += tt => tt.IsActive = false;
            TimersVM.Timers.Add(timer);
        }
        AlarmsVM.Alarms.Clear();
        foreach (var a in persist.Alarms)
        {
            bool isEnabled = a.IsEnabled;
            DateTime? nextTrigger = a.NextTriggerDateTime;
            if (isEnabled && nextTrigger.HasValue && nextTrigger.Value <= DateTime.Now)
            {
                isEnabled = false;
                nextTrigger = null;
            }
            var alarm = new AlarmEntryViewModel(a.AlarmTime, isEnabled, nextTrigger);
            alarm.RequestDelete += aa => AlarmsVM.Alarms.Remove(aa);
            AlarmsVM.Alarms.Add(alarm);
        }
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
        foreach (var timer in TimersVM.Timers)
        {
            if (timer.IsRunning)
                _trayIconManager.UpdateTooltip(GetTimerId(timer), timer.DisplayTime);
        }
        foreach (var alarm in AlarmsVM.Alarms)
        {
            if (alarm.IsEnabled && alarm.NextTriggerDateTime.HasValue)
            {
                var left = alarm.NextTriggerDateTime.Value - DateTime.Now;
                string text = left > TimeSpan.Zero ? left.ToString(@"hh\:mm\:ss") : "00:00:00";
                _trayIconManager.UpdateTooltip(GetAlarmId(alarm), text);
            }
        }
    }

    private void Timers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (TimerEntryViewModel t in e.NewItems) SubscribeTimer(t);
        if (e.OldItems != null)
            foreach (TimerEntryViewModel t in e.OldItems) RemoveTimerTray(t);
    }

    private void Alarms_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (AlarmEntryViewModel a in e.NewItems) SubscribeAlarm(a);
        if (e.OldItems != null)
            foreach (AlarmEntryViewModel a in e.OldItems) RemoveAlarmTray(a);
    }

    /// <summary>
    /// Подписывает таймер на события для отображения/удаления иконки в трее.
    /// </summary>
    private void SubscribeTimer(TimerEntryViewModel timer)
    {
        timer.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(timer.IsRunning) || e.PropertyName == nameof(timer.IsStartAvailable))
            {
                if (timer.IsRunning)
                    AddTimerTray(timer);
                else
                    RemoveTimerTray(timer);
            }
        };
        if (timer.IsRunning) AddTimerTray(timer);
    }

    /// <summary>
    /// Подписывает будильник на события для отображения/удаления иконки в трее.
    /// </summary>
    private void SubscribeAlarm(AlarmEntryViewModel alarm)
    {
        alarm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(alarm.IsEnabled) || e.PropertyName == nameof(alarm.IsStartAvailable))
            {
                if (alarm.IsEnabled)
                    AddAlarmTray(alarm);
                else
                    RemoveAlarmTray(alarm);
            }
        };
        if (alarm.IsEnabled) AddAlarmTray(alarm);
    }

    /// <summary>
    /// Добавляет иконку трея для таймера.
    /// </summary>
    private void AddTimerTray(TimerEntryViewModel timer)
    {
        string id = GetTimerId(timer);
        string iconPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "timer.ico");
        _trayIconManager.AddOrUpdateTrayIcon(id, iconPath, timer.DisplayTime);
    }

    /// <summary>
    /// Удаляет иконку трея для таймера.
    /// </summary>
    private void RemoveTimerTray(TimerEntryViewModel timer)
    {
        _trayIconManager.RemoveTrayIcon(GetTimerId(timer));
    }

    /// <summary>
    /// Добавляет иконку трея для будильника.
    /// </summary>
    private void AddAlarmTray(AlarmEntryViewModel alarm)
    {
        string id = GetAlarmId(alarm);
        string iconPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "alarm.ico");
        var left = alarm.NextTriggerDateTime.HasValue ? alarm.NextTriggerDateTime.Value - DateTime.Now : TimeSpan.Zero;
        string text = left > TimeSpan.Zero ? left.ToString(@"hh\:mm\:ss") : "00:00:00";
        _trayIconManager.AddOrUpdateTrayIcon(id, iconPath, text);
    }

    /// <summary>
    /// Удаляет иконку трея для будильника.
    /// </summary>
    private void RemoveAlarmTray(AlarmEntryViewModel alarm)
    {
        _trayIconManager.RemoveTrayIcon(GetAlarmId(alarm));
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
    /// Обработка события StopRequested от TrayIconManager.
    /// </summary>
    private void OnTrayIconStopRequested(string id)
    {
        if (id.StartsWith("timer_"))
        {
            var timer = TimersVM.Timers.FirstOrDefault(t => GetTimerId(t) == id);
            timer?.Stop();
        }
        else if (id.StartsWith("alarm_"))
        {
            var alarm = AlarmsVM.Alarms.FirstOrDefault(a => GetAlarmId(a) == id);
            alarm?.Stop();
        }
    }

    /// <summary>
    /// Обрабатывает срабатывание будильника: проигрывает звук, показывает уведомление, сбрасывает состояние.
    /// </summary>
    private void OnAlarmTriggered(AlarmEntryViewModel alarm)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var app = System.Windows.Application.Current as App;
            if (app?.Services is not { } services)
                return;
            var soundService = services.GetService(typeof(ISoundService)) as ISoundService;
            if (soundService == null)
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
    #endregion
} 