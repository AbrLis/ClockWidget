using ClockWidgetApp.Models;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.Helpers;

/// <summary>
/// Вспомогательный класс для управления позициями окон.
/// </summary>
public static class WindowPositionHelper
{
    /// <summary>
    /// Получает сохранённую позицию окна.
    /// </summary>
    /// <param name="settings">Настройки виджета.</param>
    /// <param name="isAnalogClock">Признак для аналоговых часов.</param>
    /// <returns>Кортеж с координатами Left и Top.</returns>
    public static (double Left, double Top) GetWindowPosition(WidgetSettings settings, bool isAnalogClock)
    {
        if (isAnalogClock)
        {
            return (
                settings.AnalogClockLeft ?? Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_LEFT,
                settings.AnalogClockTop ?? Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_TOP
            );
        }
        else
        {
            return (
                settings.WindowLeft ?? Constants.WindowSettings.DEFAULT_WINDOW_LEFT,
                settings.WindowTop ?? Constants.WindowSettings.DEFAULT_WINDOW_TOP
            );
        }
    }

    /// <summary>
    /// Перегрузка: Получает позицию окна через IAppDataService.
    /// </summary>
    public static (double Left, double Top) GetWindowPosition(IAppDataService appDataService, bool isAnalogClock)
        => GetWindowPosition(appDataService.Data.WidgetSettings, isAnalogClock);

    /// <summary>
    /// Сохраняет позицию окна.
    /// </summary>
    /// <param name="settings">Настройки виджета.</param>
    /// <param name="left">Координата Left.</param>
    /// <param name="top">Координата Top.</param>
    /// <param name="isAnalogClock">Признак для аналоговых часов.</param>
    public static void SaveWindowPosition(WidgetSettings settings, double left, double top, bool isAnalogClock)
    {
        if (isAnalogClock)
        {
            settings.AnalogClockLeft = left;
            settings.AnalogClockTop = top;
        }
        else
        {
            settings.WindowLeft = left;
            settings.WindowTop = top;
        }
    }

    /// <summary>
    /// Перегрузка: Сохраняет позицию окна через IAppDataService.
    /// </summary>
    public static void SaveWindowPosition(IAppDataService appDataService, double left, double top, bool isAnalogClock)
        => SaveWindowPosition(appDataService.Data.WidgetSettings, left, top, isAnalogClock);
} 