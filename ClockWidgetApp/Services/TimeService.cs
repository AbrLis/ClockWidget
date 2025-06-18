using System;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для обновления и предоставления текущего времени.
/// Использует таймер для периодического обновления времени.
/// </summary>
public class TimeService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly ILogger<TimeService> _logger = LoggingService.CreateLogger<TimeService>();
    private DateTime _currentTime;
    private bool _isDisposed;
    private DateTime _lastSecondUpdate;

    /// <summary>
    /// Событие, возникающее при обновлении времени.
    /// </summary>
    public event EventHandler<DateTime>? TimeUpdated;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TimeService"/>.
    /// Создает таймер с интервалом в 100мс для более точного обновления.
    /// </summary>
    public TimeService()
    {
        try
        {
            _logger.LogDebug("TimeService: Constructor started");
            _timer = new System.Timers.Timer(100); // Обновление каждые 100мс для точности
            _timer.Elapsed += OnTimerElapsed;
            _currentTime = DateTime.Now;
            _lastSecondUpdate = _currentTime;
            _logger.LogDebug("TimeService: Constructor completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TimeService: Constructor error");
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
                _logger.LogWarning("TimeService: Start called after disposal");
                return;
            }

            _logger.LogDebug("TimeService: Starting timer");
            _timer.Start();
            OnTimerElapsed(this, EventArgs.Empty); // Немедленное обновление при старте
            _logger.LogDebug("TimeService: Timer started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TimeService: Start error");
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
                _logger.LogWarning("TimeService: Stop called after disposal");
                return;
            }

            _logger.LogDebug("TimeService: Stopping timer");
            _timer.Stop();
            _logger.LogDebug("TimeService: Timer stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TimeService: Stop error");
            throw;
        }
    }

    /// <summary>
    /// Обработчик события таймера.
    /// Обновляет текущее время и вызывает событие <see cref="TimeUpdated"/> только при изменении секунды.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события.</param>
    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        try
        {
            if (_isDisposed)
            {
                _logger.LogWarning("TimeService: Timer elapsed after disposal");
                return;
            }

            _currentTime = DateTime.Now;
            
            // Проверяем, изменилась ли секунда с последнего обновления
            if (_currentTime.Second != _lastSecondUpdate.Second)
            {
                _lastSecondUpdate = _currentTime;
                _logger.LogTrace("TimeService: Second changed - {Time}", _currentTime);
                
                var handler = TimeUpdated;
                if (handler != null)
                {
                    handler(this, _currentTime);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TimeService: Timer elapsed error");
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
                _logger.LogDebug("TimeService: Disposing");
                Stop();
                _timer.Dispose();
                _isDisposed = true;
                _logger.LogDebug("TimeService: Disposed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TimeService: Dispose error");
        }
    }
} 