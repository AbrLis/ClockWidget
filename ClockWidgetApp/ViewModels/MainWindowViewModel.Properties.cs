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

    /// <summary>
    /// Текст времени для отображения.
    /// </summary>
    public string TimeText
    {
        get => _timeText;
        set { _timeText = value; OnPropertyChanged(); }
    }
    /// <summary>
    /// Прозрачность фона.
    /// </summary>
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
    /// <summary>
    /// Прозрачность текста.
    /// </summary>
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
    /// <summary>
    /// Размер шрифта.
    /// </summary>
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
    /// <summary>
    /// Показывать секунды.
    /// </summary>
    public bool ShowSeconds
    {
        get => _showSeconds;
        set { _showSeconds = value; OnPropertyChanged(); _settingsService.UpdateSettings(s => s.ShowSeconds = _showSeconds); }
    }
    /// <summary>
    /// Показывать цифровые часы.
    /// </summary>
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
                        _logger.LogWarning("[MainWindowViewModel.Properties] Cannot hide both windows, keeping digital clock visible");
                        return;
                    }
                    _logger.LogInformation($"[MainWindowViewModel.Properties] Updating show digital clock: {value}");
                    _showDigitalClock = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.ShowDigitalClock = value);
                    if (value)
                    {
                        if (System.Windows.Application.Current is App app)
                        {
                            app.ShowMainWindowIfNeeded();
                        }
                    }
                    UpdateWindowsVisibility();
                }
                catch (Exception)
                {
                    _logger.LogError("[MainWindowViewModel.Properties] Error updating show digital clock setting");
                    _showDigitalClock = !value;
                    OnPropertyChanged();
                }
            }
        }
    }
    /// <summary>
    /// Показывать аналоговые часы.
    /// </summary>
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
                        _logger.LogWarning("[MainWindowViewModel.Properties] Cannot hide both windows, keeping analog clock visible");
                        return;
                    }
                    _logger.LogInformation($"[MainWindowViewModel.Properties] Updating show analog clock: {value}");
                    _showAnalogClock = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.ShowAnalogClock = value);
                    UpdateWindowsVisibility();
                }
                catch (Exception)
                {
                    _logger.LogError("[MainWindowViewModel.Properties] Error updating show analog clock setting");
                    _showAnalogClock = !value;
                    OnPropertyChanged();
                }
            }
        }
    }
    /// <summary>
    /// Размер аналоговых часов.
    /// </summary>
    public double AnalogClockSize
    {
        get => _analogClockSize;
        set
        {
            if (Math.Abs(_analogClockSize - value) > 0.001)
            {
                try
                {
                    _logger.LogInformation($"[MainWindowViewModel.Properties] Updating analog clock size: {value}");
                    _analogClockSize = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.AnalogClockSize = value);
                    UpdateAnalogClockSize();
                }
                catch (Exception)
                {
                    _logger.LogError("[MainWindowViewModel.Properties] Error updating analog clock size");
                    _analogClockSize = value;
                    OnPropertyChanged();
                }
            }
        }
    }
    /// <summary>
    /// Аналоговые часы поверх всех окон.
    /// </summary>
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
    /// <summary>
    /// Цифровые часы поверх всех окон.
    /// </summary>
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
    /// <summary>
    /// Воспроизводить звук кукушки каждый час.
    /// </summary>
    public bool CuckooEveryHour
    {
        get => _settingsService.CurrentSettings.CuckooEveryHour;
        set
        {
            if (_settingsService.CurrentSettings.CuckooEveryHour != value)
            {
                _logger.LogInformation($"[MainWindowViewModel.Properties] Updating CuckooEveryHour: {value}");
                _settingsService.UpdateSettings(s => s.CuckooEveryHour = value);
                OnPropertyChanged();
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
        // Получаем текущее активное окно типа MainWindow
        var window = System.Windows.Application.Current.Windows
            .OfType<MainWindow>()
            .FirstOrDefault();
        if (window != null)
        {
            window.Topmost = _digitalClockTopmost;
        }
    }
} 