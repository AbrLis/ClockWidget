using System;
using System.Timers;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для обновления и предоставления текущего времени.
/// Использует таймер для периодического обновления времени.
/// </summary>
public class TimeService
{
    private readonly System.Timers.Timer _timer;
    private DateTime _currentTime;

    /// <summary>
    /// Событие, возникающее при обновлении времени.
    /// </summary>
    public event EventHandler<DateTime>? TimeUpdated;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TimeService"/>.
    /// Создает таймер с интервалом в 1 секунду.
    /// </summary>
    public TimeService()
    {
        _timer = new System.Timers.Timer(1000); // Обновление каждую секунду
        _timer.Elapsed += OnTimerElapsed;
        _currentTime = DateTime.Now;
    }

    /// <summary>
    /// Запускает сервис обновления времени.
    /// </summary>
    public void Start()
    {
        _timer.Start();
        OnTimerElapsed(this, EventArgs.Empty); // Немедленное обновление при старте
    }

    /// <summary>
    /// Останавливает сервис обновления времени.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
    }

    /// <summary>
    /// Обработчик события таймера.
    /// Обновляет текущее время и вызывает событие <see cref="TimeUpdated"/>.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        _currentTime = DateTime.Now;
        TimeUpdated?.Invoke(this, _currentTime);
    }
} 