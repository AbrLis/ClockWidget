using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Serilog;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для отслеживания срабатывания будильников с динамическим управлением таймером, потокобезопасностью и обработкой аномалий времени.
/// </summary>
public class AlarmMonitorService : IDisposable
{
    #region Private fields
    /// <summary>Коллекция отслеживаемых будильников.</summary>
    private readonly ObservableCollection<AlarmEntryViewModel> _alarms;
    /// <summary>Объект блокировки для потокобезопасности.</summary>
    private readonly Lock _lock = new();
    /// <summary>Таймер для отслеживания времени срабатывания.</summary>
    private System.Threading.Timer? _timer;
    /// <summary>Кэш ближайшего будильника.</summary>
    private AlarmEntryViewModel? _nextScheduledAlarm;
    /// <summary>Время следующего срабатывания будильника.</summary>
    private DateTime? _nextScheduledTime;
    /// <summary>Последнее зафиксированное системное время.</summary>
    private DateTime _lastSystemTime;
    /// <summary>Флаг освобождения ресурсов.</summary>
    private bool _disposed;
    #endregion

    /// <summary>
    /// Событие, возникающее при срабатывании будильника.
    /// </summary>
    public event Action<AlarmEntryViewModel>? AlarmTriggered;

    #region Constructor
    /// <summary>
    /// Создаёт сервис и начинает отслеживание коллекции будильников.
    /// </summary>
    /// <param name="alarms">Коллекция будильников для отслеживания.</param>
    public AlarmMonitorService(ObservableCollection<AlarmEntryViewModel> alarms)
    {
        _alarms = alarms;
        _alarms.CollectionChanged += OnAlarmsCollectionChanged;
        foreach (var alarm in _alarms)
            SubscribeAlarm(alarm);
        _lastSystemTime = DateTime.Now;
        Log.Information("[AlarmMonitorService] Service started. Alarm count: {Count}", _alarms.Count);
        RecalculateNextAlarmAndRestartTimer();
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Останавливает таймер и освобождает ресурсы.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
            _timer?.Dispose();
            _alarms.CollectionChanged -= OnAlarmsCollectionChanged;
            foreach (var alarm in _alarms)
                UnsubscribeAlarm(alarm);
            Log.Information("[AlarmMonitorService] Service stopped and resources released.");
        }
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Private methods and event handlers
    /// <summary>
    /// Пересчитывает ближайший будильник и перезапускает таймер.
    /// </summary>
    private void RecalculateNextAlarmAndRestartTimer()
    {
        lock (_lock)
        {
            var now = DateTime.Now;
            var enabledAlarms = _alarms.Where(a => a is { IsEnabled: true, NextTriggerDateTime: not null }).ToList();
            _nextScheduledAlarm = enabledAlarms
                .OrderBy(a => a.NextTriggerDateTime!.Value)
                .FirstOrDefault();
            _nextScheduledTime = _nextScheduledAlarm?.NextTriggerDateTime;

            TimeSpan dueTime;
            if (_nextScheduledTime.HasValue)
            {
                dueTime = _nextScheduledTime.Value - now;
                if (dueTime < TimeSpan.Zero)
                    dueTime = TimeSpan.Zero;
            }
            else
            {
                dueTime = TimeSpan.FromMinutes(1);
            }

            _timer?.Dispose();
            _timer = new System.Threading.Timer(OnTimerElapsed, null, dueTime, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Обработчик срабатывания таймера.
    /// </summary>
    /// <param name="state">Не используется.</param>
    private void OnTimerElapsed(object? state)
    {
        lock (_lock)
        {
            if (_disposed) return;
            var now = DateTime.Now;
            // Check for time anomaly (e.g., clock change)
            if (Math.Abs((now - _lastSystemTime).TotalMinutes) > 5)
            {
                Log.Information("[AlarmMonitorService] Time anomaly detected. Recalculating all alarms.");
                _lastSystemTime = now;
                RecalculateNextAlarmAndRestartTimer();
                return;
            }

            _lastSystemTime = now;

            var triggeredAlarms = _alarms
                .Where(a => a is { IsEnabled: true, NextTriggerDateTime: not null } &&
                            now >= a.NextTriggerDateTime.Value)
                .ToList();

            foreach (var alarm in triggeredAlarms)
            {
                try
                {
                    Log.Information("[AlarmMonitorService] Alarm triggered: {Alarm}", alarm);
                    // Ensure event is invoked on UI thread
                    if (System.Windows.Application.Current?.Dispatcher != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            AlarmTriggered?.Invoke(alarm)));
                    }
                    else
                    {
                        AlarmTriggered?.Invoke(alarm);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[AlarmMonitorService] Error while handling alarm: {Alarm}", alarm);
                }

                try
                {
                    alarm.UpdateNextTrigger();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[AlarmMonitorService] Error while updating alarm time: {Alarm}", alarm);
                }
            }

            RecalculateNextAlarmAndRestartTimer();
        }
    }

    /// <summary>
    /// Обработчик изменения коллекции будильников.
    /// </summary>
    private void OnAlarmsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (AlarmEntryViewModel alarm in e.NewItems)
                SubscribeAlarm(alarm);
        if (e.OldItems != null)
            foreach (AlarmEntryViewModel alarm in e.OldItems)
                UnsubscribeAlarm(alarm);
        RecalculateNextAlarmAndRestartTimer();
    }

    /// <summary>
    /// Подписка на события отдельного будильника (например, изменение времени).
    /// </summary>
    private void SubscribeAlarm(AlarmEntryViewModel alarm)
    {
        alarm.PropertyChanged += Alarm_PropertyChanged;
    }

    /// <summary>
    /// Отписка от событий отдельного будильника.
    /// </summary>
    private void UnsubscribeAlarm(AlarmEntryViewModel alarm)
    {
        alarm.PropertyChanged -= Alarm_PropertyChanged;
    }

    /// <summary>
    /// Обработчик изменения свойств будильника (например, времени или статуса).
    /// </summary>
    private void Alarm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RecalculateNextAlarmAndRestartTimer();
    }
    #endregion
}