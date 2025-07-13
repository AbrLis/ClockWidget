using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Serilog;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для отслеживания срабатывания будильников с динамическим управлением таймером, потокобезопасностью и обработкой аномалий времени.
/// </summary>
public class AlarmMonitorService : IDisposable
{
    private readonly ObservableCollection<AlarmEntryViewModel> _alarms;
    private readonly Lock _lock = new();
    private System.Threading.Timer? _timer;
    private AlarmEntryViewModel? _nextScheduledAlarm;
    private DateTime? _nextScheduledTime;
    private DateTime _lastSystemTime;
    private bool _disposed;

    /// <summary>
    /// Событие, возникающее при срабатывании будильника.
    /// </summary>
    public event Action<AlarmEntryViewModel>? AlarmTriggered;

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
        Log.Information("[AlarmMonitorService] Сервис запущен. Количество будильников: {Count}", _alarms.Count);
        RecalculateNextAlarmAndRestartTimer();
    }

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
                Log.Information("[AlarmMonitorService] Следующий будильник: {Time}", _nextScheduledTime);
            }
            else
            {
                // Нет активных будильников — проверяем раз в минуту
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
            // Проверка на аномалию времени (например, перевод часов)
            if (Math.Abs((now - _lastSystemTime).TotalMinutes) > 5)
            {
                Log.Information("[AlarmMonitorService] Обнаружена аномалия времени. Пересчёт всех будильников.");
                // Аномалия — пересчитываем все будильники
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
                    Log.Information("[AlarmMonitorService] Сработал будильник: {Alarm}", alarm);
                    // Гарантируем вызов события в UI-потоке
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
                    Log.Error(ex, "[AlarmMonitorService] Ошибка при обработке будильника: {Alarm}", alarm);
                }

                try
                {
                    alarm.UpdateNextTrigger();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[AlarmMonitorService] Ошибка при обновлении времени будильника: {Alarm}", alarm);
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
        // При любом изменении — пересчёт ближайшего будильника
        RecalculateNextAlarmAndRestartTimer();
    }

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
            Log.Information("[AlarmMonitorService] Сервис остановлен и ресурсы освобождены.");
        }
    }
}