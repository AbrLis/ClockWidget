using System;
using System.Windows.Threading;

namespace ClockWidgetApp.Services;

public class TimeService
{
    private readonly DispatcherTimer _timer;
    public event EventHandler<DateTime>? TimeUpdated;

    public TimeService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
    }

    public void Start()
    {
        _timer.Start();
        // Обновляем время сразу при запуске
        TimeUpdated?.Invoke(this, DateTime.Now);
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        TimeUpdated?.Invoke(this, DateTime.Now);
    }
} 