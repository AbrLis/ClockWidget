using System;

namespace ClockWidgetApp.Services;

/// <summary>
/// Интерфейс для сервиса времени.
/// </summary>
public interface ITimeService : IDisposable
{
    /// <summary>
    /// Событие, возникающее при обновлении времени.
    /// </summary>
    event EventHandler<DateTime>? TimeUpdated;

    /// <summary>
    /// Запускает сервис обновления времени.
    /// </summary>
    void Start();

    /// <summary>
    /// Останавливает сервис обновления времени.
    /// </summary>
    void Stop();
} 