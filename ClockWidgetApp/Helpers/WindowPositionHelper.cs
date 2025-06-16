using System;
using ClockWidgetApp.Services;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.Helpers;

/// <summary>
/// Helper class for managing window positions.
/// </summary>
public static class WindowPositionHelper
{
    /// <summary>
    /// Gets the saved window position.
    /// </summary>
    /// <param name="settingsService">The settings service.</param>
    /// <param name="isAnalogClock">Whether this is for the analog clock window.</param>
    /// <returns>Tuple with left and top coordinates.</returns>
    public static (double Left, double Top) GetWindowPosition(SettingsService settingsService, bool isAnalogClock)
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
    /// Saves the window position.
    /// </summary>
    /// <param name="settingsService">The settings service.</param>
    /// <param name="left">Left coordinate.</param>
    /// <param name="top">Top coordinate.</param>
    /// <param name="isAnalogClock">Whether this is for the analog clock window.</param>
    public static void SaveWindowPosition(SettingsService settingsService, double left, double top, bool isAnalogClock)
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