using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private string _timeText = string.Empty;
    private double _backgroundOpacity;
    private double _textOpacity;
    private double _fontSize;
    private bool _showSeconds;
    private bool _showDigitalClock = true;
    private bool _showAnalogClock = true;
    private double _analogClockSize;
    private AnalogClockWindow? _analogClockWindow;
    private bool _analogClockTopmost = true;
    private bool _digitalClockTopmost = true;

    public string TimeText
    {
        get => _timeText;
        set { _timeText = value; OnPropertyChanged(); }
    }
    public double BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            var validatedValue = ValidateOpacity(value, 
                Constants.WindowSettings.MIN_WINDOW_OPACITY, 
                Constants.WindowSettings.MAX_WINDOW_OPACITY, 
                Constants.WindowSettings.DEFAULT_WINDOW_OPACITY);
            if (_backgroundOpacity != validatedValue)
            {
                _backgroundOpacity = validatedValue;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.BackgroundOpacity = validatedValue);
            }
        }
    }
    public double TextOpacity
    {
        get => _textOpacity;
        set
        {
            var validatedValue = ValidateOpacity(value, 
                Constants.TextSettings.MIN_TEXT_OPACITY, 
                Constants.TextSettings.MAX_TEXT_OPACITY, 
                Constants.TextSettings.DEFAULT_TEXT_OPACITY);
            if (_textOpacity != validatedValue)
            {
                _textOpacity = validatedValue;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.TextOpacity = validatedValue);
            }
        }
    }
    public double FontSize
    {
        get => _fontSize;
        set
        {
            var validatedValue = ValidateFontSize(value);
            if (_fontSize != validatedValue)
            {
                _fontSize = validatedValue;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.FontSize = validatedValue);
            }
        }
    }
    public bool ShowSeconds
    {
        get => _showSeconds;
        set { _showSeconds = value; OnPropertyChanged(); _settingsService.UpdateSettings(s => s.ShowSeconds = _showSeconds); }
    }
    public bool ShowDigitalClock
    {
        get => _showDigitalClock;
        set
        {
            if (_showDigitalClock != value)
            {
                try
                {
                    if (!value && !_showAnalogClock)
                    {
                        _logger.LogWarning("Cannot hide both windows, keeping digital clock visible");
                        return;
                    }
                    _logger.LogInformation($"Updating show digital clock: {value}");
                    _showDigitalClock = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.ShowDigitalClock = value);
                    UpdateWindowsVisibility();
                }
                catch (Exception)
                {
                    _logger.LogError("Error updating show digital clock setting");
                    _showDigitalClock = !value;
                    OnPropertyChanged();
                }
            }
        }
    }
    public bool ShowAnalogClock
    {
        get => _showAnalogClock;
        set
        {
            if (_showAnalogClock != value)
            {
                try
                {
                    if (!value && !_showDigitalClock)
                    {
                        _logger.LogWarning("Cannot hide both windows, keeping analog clock visible");
                        return;
                    }
                    _logger.LogInformation($"Updating show analog clock: {value}");
                    _showAnalogClock = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.ShowAnalogClock = value);
                    UpdateWindowsVisibility();
                }
                catch (Exception)
                {
                    _logger.LogError("Error updating show analog clock setting");
                    _showAnalogClock = !value;
                    OnPropertyChanged();
                }
            }
        }
    }
    public double AnalogClockSize
    {
        get => _analogClockSize;
        set
        {
            if (Math.Abs(_analogClockSize - value) > 0.001)
            {
                try
                {
                    _logger.LogInformation($"Updating analog clock size: {value}");
                    _analogClockSize = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.AnalogClockSize = value);
                    UpdateAnalogClockSize();
                }
                catch (Exception)
                {
                    _logger.LogError("Error updating analog clock size");
                    _analogClockSize = value;
                    OnPropertyChanged();
                }
            }
        }
    }
    public bool AnalogClockTopmost
    {
        get => _analogClockTopmost;
        set
        {
            if (_analogClockTopmost != value)
            {
                _analogClockTopmost = value;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.AnalogClockTopmost = value);
                UpdateAnalogClockTopmost();
            }
        }
    }
    public bool DigitalClockTopmost
    {
        get => _digitalClockTopmost;
        set
        {
            if (_digitalClockTopmost != value)
            {
                _digitalClockTopmost = value;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.DigitalClockTopmost = value);
                UpdateDigitalClockTopmost();
            }
        }
    }

    private void UpdateAnalogClockTopmost()
    {
        if (_analogClockWindow != null)
        {
            _analogClockWindow.Topmost = _analogClockTopmost;
        }
    }
    private void UpdateDigitalClockTopmost()
    {
        if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.Topmost = _digitalClockTopmost;
        }
    }
} 