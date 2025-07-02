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

    private readonly AlarmMonitorService _alarmMonitorService;
    private readonly TimersAndAlarmsPersistenceService _persistenceService;

    /// <summary>
    /// Локализованные строки для UI.
    /// </summary>
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

    public TimersAndAlarmsViewModel()
    {
        TimersVM = new TimersViewModel();
        AlarmsVM = new AlarmsViewModel();
        _alarmMonitorService = new AlarmMonitorService(AlarmsVM.Alarms);
        _alarmMonitorService.AlarmTriggered += OnAlarmTriggered;
        var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClockWidget", "timers_alarms.json");
        _persistenceService = new TimersAndAlarmsPersistenceService(filePath);
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
    }

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
            var notification = new Views.TimerNotificationWindow(soundHandle, dateTimeText, "alarm");
            notification.Show();
            alarm.IsEnabled = false;
            alarm.ClearNextTriggerDateTime();
            alarm.OnPropertyChanged(nameof(alarm.IsEnabled));
        });
    }

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
            // Если время следующего срабатывания будильника истекло — выключаем будильник при загрузке
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

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 