using ClockWidgetApp.Models;

namespace ClockWidgetApp.Services;

/// <summary>
/// Интерфейс для сервиса управления настройками.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Текущие настройки виджета.
    /// </summary>
    WidgetSettings CurrentSettings { get; }

    /// <summary>
    /// Обновляет настройки с помощью указанного действия (только в буфере).
    /// </summary>
    void UpdateSettings(Action<WidgetSettings> updateAction);

    /// <summary>
    /// Сохраняет буферизированные настройки в файл.
    /// </summary>
    void SaveBufferedSettings();

    /// <summary>
    /// Загружает настройки из файла.
    /// </summary>
    WidgetSettings LoadSettings();
} 