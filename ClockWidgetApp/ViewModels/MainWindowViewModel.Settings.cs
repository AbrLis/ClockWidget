using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        return Math.Round(value / Constants.WindowSettings.OPACITY_STEP) * Constants.WindowSettings.OPACITY_STEP;
    }
    private double ValidateFontSize(double value)
    {
        if (value < Constants.TextSettings.MIN_FONT_SIZE || value > Constants.TextSettings.MAX_FONT_SIZE)
        {
            return Constants.TextSettings.DEFAULT_FONT_SIZE;
        }
        return Math.Round(value / Constants.TextSettings.FONT_SIZE_STEP) * Constants.TextSettings.FONT_SIZE_STEP;
    }
    public void UpdateSettings(WidgetSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }
        settings = WidgetSettings.ValidateSettings(settings);
        if (settings.ShowDigitalClock)
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Show();
                mainWindow.Activate();
            }
        }
        else
        {
            System.Windows.Application.Current.MainWindow?.Hide();
        }
        UpdateAnalogClockSettings(settings);
        _settingsService.SaveSettings(settings);
    }
    private void OnTimeUpdated(object? sender, DateTime time)
    {
        TimeText = time.ToString(_showSeconds ? 
            Constants.DisplaySettings.TIME_FORMAT_WITH_SECONDS : 
            Constants.DisplaySettings.TIME_FORMAT_WITHOUT_SECONDS);
    }
    private void InitializeFromSettings(WidgetSettings settings)
    {
        _backgroundOpacity = settings.BackgroundOpacity;
        _textOpacity = settings.TextOpacity;
        _fontSize = settings.FontSize;
        _showSeconds = settings.ShowSeconds;
        _showDigitalClock = settings.ShowDigitalClock;
        _showAnalogClock = settings.ShowAnalogClock;
        _analogClockSize = settings.AnalogClockSize;
    }
} 