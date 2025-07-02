using System.Collections.ObjectModel;
using System.Timers;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для отслеживания срабатывания будильников и уведомления UI.
/// </summary>
public class AlarmMonitorService : IDisposable
{
    private readonly ObservableCollection<AlarmEntryViewModel> _alarms;
    private readonly System.Timers.Timer _timer;

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
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;
        foreach (var alarm in _alarms)
        {
            if (alarm.IsRunning && alarm.IsActive)
            {
                var alarmTime = new TimeSpan(alarm.AlarmTime.Hours, alarm.AlarmTime.Minutes, 0);
                var nowTime = new TimeSpan(now.Hour, now.Minute, 0);
                if (nowTime >= alarmTime)
                {
                    AlarmTriggered?.Invoke(alarm);
                    alarm.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Останавливает таймер и освобождает ресурсы.
    /// </summary>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
} 