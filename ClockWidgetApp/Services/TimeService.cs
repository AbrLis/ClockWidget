using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для обновления и предоставления текущего времени.
/// Использует таймер для периодического обновления времени.
/// </summary>
public class TimeService : IDisposable, ITimeService
{
    #region Private fields
    /// <summary>Таймер для обновления времени.</summary>
    private readonly System.Timers.Timer _timer;
    /// <summary>Логгер для событий сервиса.</summary>
    private readonly ILogger<TimeService> _logger;
    /// <summary>Текущее время.</summary>
    private DateTime _currentTime;
    /// <summary>Флаг, указывающий, что сервис был освобождён.</summary>
    private bool _isDisposed;
    /// <summary>Последнее обновлённое значение секунды.</summary>
    private DateTime _lastSecondUpdate;
    #endregion

    #region Events
    /// <summary>
    /// Событие, возникающее при обновлении времени.
    /// </summary>
    public event EventHandler<DateTime>? TimeUpdated;
    #endregion

    #region Constructors
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TimeService"/>.
    /// Создаёт таймер с интервалом 100мс для точного обновления.
    /// </summary>
    public TimeService(ILogger<TimeService> logger)
    {
        try
        {
            _logger = logger;
            _logger.LogDebug("[TimeService] Constructor started");
            _timer = new System.Timers.Timer(100); // Обновление каждые 100мс для точности
            _timer.Elapsed += OnTimerElapsed;
            _currentTime = DateTime.Now;
            _lastSecondUpdate = _currentTime;
            _logger.LogDebug("[TimeService] Constructor completed");
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[TimeService] Constructor error");
            throw;
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Запускает сервис обновления времени.
    /// </summary>
    public void Start()
    {
        try
        {
            if (_isDisposed)
            {
                _logger.LogWarning("[TimeService] Start called after disposal");
                return;
            }

            _logger.LogDebug("[TimeService] Starting timer");
            _timer.Start();
            OnTimerElapsed(this, EventArgs.Empty); // Немедленное обновление при старте
            _logger.LogDebug("[TimeService] Timer started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TimeService] Start error");
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
                _logger.LogWarning("[TimeService] Stop called after disposal");
                return;
            }

            _logger.LogDebug("[TimeService] Stopping timer");
            _timer.Stop();
            _logger.LogDebug("[TimeService] Timer stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TimeService] Stop error");
            throw;
        }
    }

    /// <summary>
    /// Освобождает ресурсы, используемые сервисом времени.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (!_isDisposed)
            {
                _logger.LogDebug("[TimeService] Disposing");
                Stop();
                _timer.Dispose();
                _isDisposed = true;
                _logger.LogDebug("[TimeService] Disposed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TimeService] Dispose error");
        }
    }
    #endregion

    #region Private methods
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
                _logger.LogWarning("[TimeService] Timer elapsed after disposal");
                return;
            }

            _currentTime = DateTime.Now;

            // Проверяем, изменилась ли секунда с последнего обновления
            if (_currentTime.Second != _lastSecondUpdate.Second)
            {
                _lastSecondUpdate = _currentTime;

                var handler = TimeUpdated;
                if (handler != null)
                {
                    handler(this, _currentTime);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TimeService] Timer elapsed error");
        }
    }
    #endregion
}