using System;
using System.Timers;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для обновления и предоставления текущего времени.
/// Использует таймер для периодического обновления времени.
/// </summary>
public class TimeService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private DateTime _currentTime;
    private bool _isDisposed;

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
        try
        {
            DebugHelper.WriteLine("TimeService: Constructor started");
            _timer = new System.Timers.Timer(1000); // Обновление каждую секунду
            _timer.Elapsed += OnTimerElapsed;
            _currentTime = DateTime.Now;
            DebugHelper.WriteLine("TimeService: Constructor completed");
        }
        catch (Exception ex)
        {
            DebugHelper.WriteError("TimeService: Constructor error", ex);
            throw;
        }
    }

    /// <summary>
    /// Запускает сервис обновления времени.
    /// </summary>
    public void Start()
    {
        try
        {
            if (_isDisposed)
            {
                DebugHelper.WriteLine("TimeService: Start called after disposal");
                return;
            }

            DebugHelper.WriteLine("TimeService: Starting timer");
            _timer.Start();
            OnTimerElapsed(this, EventArgs.Empty); // Немедленное обновление при старте
            DebugHelper.WriteLine("TimeService: Timer started");
        }
        catch (Exception ex)
        {
            DebugHelper.WriteError("TimeService: Start error", ex);
            throw;
        }
    }

    /// <summary>
    /// Останавливает сервис обновления времени.
    /// </summary>
    public void Stop()
    {
        try
        {
            if (_isDisposed)
            {
                DebugHelper.WriteLine("TimeService: Stop called after disposal");
                return;
            }

            DebugHelper.WriteLine("TimeService: Stopping timer");
            _timer.Stop();
            DebugHelper.WriteLine("TimeService: Timer stopped");
        }
        catch (Exception ex)
        {
            DebugHelper.WriteError("TimeService: Stop error", ex);
            throw;
        }
    }

    /// <summary>
    /// Обработчик события таймера.
    /// Обновляет текущее время и вызывает событие <see cref="TimeUpdated"/>.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        try
        {
            if (_isDisposed)
            {
                DebugHelper.WriteLine("TimeService: Timer elapsed after disposal");
                return;
            }

            _currentTime = DateTime.Now;
            DebugHelper.WriteLine($"TimeService: Timer elapsed - {_currentTime}");
            
            var handler = TimeUpdated;
            if (handler != null)
            {
                handler(this, _currentTime);
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteError("TimeService: Timer elapsed error", ex);
        }
    }

    /// <summary>
    /// Освобождает ресурсы, используемые экземпляром класса <see cref="TimeService"/>.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (!_isDisposed)
            {
                DebugHelper.WriteLine("TimeService: Disposing");
                Stop();
                _timer.Dispose();
                _isDisposed = true;
                DebugHelper.WriteLine("TimeService: Disposed");
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteError("TimeService: Dispose error", ex);
        }
    }
} 