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
    /// <param name="settingsService">Сервис настроек.</param>
    /// <param name="isAnalogClock">Признак для аналоговых часов.</param>
    /// <returns>Кортеж с координатами Left и Top.</returns>
    public static (double Left, double Top) GetWindowPosition(ISettingsService settingsService, bool isAnalogClock)
    {
        var settings = settingsService.CurrentSettings;
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
    /// Сохраняет позицию окна.
    /// </summary>
    /// <param name="settingsService">Сервис настроек.</param>
    /// <param name="left">Координата Left.</param>
    /// <param name="top">Координата Top.</param>
    /// <param name="isAnalogClock">Признак для аналоговых часов.</param>
    public static void SaveWindowPosition(ISettingsService settingsService, double left, double top, bool isAnalogClock)
    {
        settingsService.UpdateSettings(s =>
        {
            if (isAnalogClock)
            {
                s.AnalogClockLeft = left;
                s.AnalogClockTop = top;
            }
            else
            {
                s.WindowLeft = left;
                s.WindowTop = top;
            }
        });
    }
} 