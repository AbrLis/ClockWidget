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
        var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClockWidget", "timers_alarms.json");
        _persistenceService = new TimersAndAlarmsPersistenceService(filePath);
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
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
            var alarm = new AlarmEntryViewModel(a.AlarmTime, a.IsEnabled, a.NextTriggerDateTime);
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