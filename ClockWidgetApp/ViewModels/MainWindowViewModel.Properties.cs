using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

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
    private bool _analogClockTopmost = true;
    private bool _digitalClockTopmost = true;

    private MainWindow? MainWindow => ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) is IWindowService ws ? ws.GetMainWindow() : null;
    private AnalogClockWindow? AnalogClockWindow => ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) is IWindowService ws ? ws.GetAnalogClockWindow() : null;

    /// <summary>
    /// Текст времени для отображения.
    /// </summary>
    public string TimeText
    {
        get => _timeText;
        set { _timeText = value; OnPropertyChanged(); }
    }
    /// <summary>
    /// Прозрачность фона. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
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
    /// Прозрачность текста. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
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
    /// Размер шрифта. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
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
    /// Показывать секунды. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowSeconds
    {
        get => _showSeconds;
        set { _showSeconds = value; OnPropertyChanged(); _settingsService.UpdateSettings(s => s.ShowSeconds = _showSeconds); }
    }
    /// <summary>
    /// Показывать цифровые часы. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowDigitalClock
    {
        get => _showDigitalClock;
        set
        {
            _logger.LogDebug("[ShowDigitalClock SET] old={0}, new={1} (property changed)", _showDigitalClock, value);
            if (_showDigitalClock != value)
            {
                _showDigitalClock = value;
                OnPropertyChanged(nameof(ShowDigitalClock));
                _settingsService.UpdateSettings(s => s.ShowDigitalClock = _showDigitalClock);
                UpdateWindowsVisibility();
            }
        }
    }
    /// <summary>
    /// Показывать аналоговые часы. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowAnalogClock
    {
        get => _showAnalogClock;
        set
        {
            if (_showAnalogClock != value)
            {
                _showAnalogClock = value;
                OnPropertyChanged(nameof(ShowAnalogClock));
                _settingsService.UpdateSettings(s => s.ShowAnalogClock = _showAnalogClock);
                UpdateWindowsVisibility();
            }
        }
    }
    /// <summary>
    /// Размер аналоговых часов. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
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
                    _logger.LogDebug($"[MainWindowViewModel.Properties] Updating analog clock size: {value}");
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
    /// Аналоговые часы поверх всех окон. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
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
    /// Цифровые часы поверх всех окон. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
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
    /// Воспроизводить звук кукушки каждый час. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool CuckooEveryHour
    {
        get => _settingsService.CurrentSettings.CuckooEveryHour;
        set
        {
            if (_settingsService.CurrentSettings.CuckooEveryHour != value)
            {
                _logger.LogDebug($"[MainWindowViewModel.Properties] Updating CuckooEveryHour: {value}");
                _settingsService.UpdateSettings(s => s.CuckooEveryHour = value);
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Воспроизводить сигнал каждые полчаса. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool HalfHourChimeEnabled
    {
        get => _settingsService.CurrentSettings.HalfHourChimeEnabled;
        set
        {
            if (_settingsService.CurrentSettings.HalfHourChimeEnabled != value)
            {
                _logger.LogDebug($"[MainWindowViewModel.Properties] Updating HalfHourChimeEnabled: {value}");
                _settingsService.UpdateSettings(s => s.HalfHourChimeEnabled = value);
                OnPropertyChanged();
            }
        }
    }
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

    public ISettingsService SettingsService => _settingsService;

    private void UpdateAnalogClockTopmost()
    {
        if (AnalogClockWindow != null)
        {
            AnalogClockWindow.Topmost = _analogClockTopmost;
        }
    }
    private void UpdateDigitalClockTopmost()
    {
        var window = MainWindow;
        if (window != null)
        {
            window.Topmost = _digitalClockTopmost;
        }
    }

    private void SubscribeToLanguageChanges()
    {
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
    }
} 