using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private string _timeText = string.Empty;
    private bool _showSeconds;
    private bool _showDigitalClock = true;
    private bool _showAnalogClock = true;
    private double _analogClockSize;
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
    /// Прозрачность фона. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double BackgroundOpacity
    {
        get => _appDataService.Data.WidgetSettings.BackgroundOpacity;
        set
        {
            var validatedValue = ValidateOpacity(value, 
                Constants.WindowSettings.MIN_WINDOW_OPACITY, 
                Constants.WindowSettings.MAX_WINDOW_OPACITY, 
                Constants.WindowSettings.DEFAULT_WINDOW_OPACITY);
            if (_appDataService.Data.WidgetSettings.BackgroundOpacity != validatedValue)
            {
                _appDataService.Data.WidgetSettings.BackgroundOpacity = validatedValue;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Прозрачность текста. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double TextOpacity
    {
        get => _appDataService.Data.WidgetSettings.TextOpacity;
        set
        {
            var validatedValue = ValidateOpacity(value, 
                Constants.TextSettings.MIN_TEXT_OPACITY, 
                Constants.TextSettings.MAX_TEXT_OPACITY, 
                Constants.TextSettings.DEFAULT_TEXT_OPACITY);
            if (_appDataService.Data.WidgetSettings.TextOpacity != validatedValue)
            {
                _appDataService.Data.WidgetSettings.TextOpacity = validatedValue;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Размер шрифта. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double FontSize
    {
        get => _appDataService.Data.WidgetSettings.FontSize;
        set
        {
            var validatedValue = ValidateFontSize(value);
            if (_appDataService.Data.WidgetSettings.FontSize != validatedValue)
            {
                _appDataService.Data.WidgetSettings.FontSize = validatedValue;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Показывать секунды. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowSeconds
    {
        get => _appDataService.Data.WidgetSettings.ShowSeconds;
        set
        {
            _appDataService.Data.WidgetSettings.ShowSeconds = value;
            _showSeconds = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// Показывать цифровые часы. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowDigitalClock
    {
        get => _appDataService.Data.WidgetSettings.ShowDigitalClock;
        set
        {
            _logger.LogDebug("[ShowDigitalClock SET] old={0}, new={1} (property changed)", _appDataService.Data.WidgetSettings.ShowDigitalClock, value);
            if (_appDataService.Data.WidgetSettings.ShowDigitalClock != value)
            {
                _appDataService.Data.WidgetSettings.ShowDigitalClock = value;
                OnPropertyChanged(nameof(ShowDigitalClock));
                UpdateWindowsVisibility();
            }
        }
    }
    /// <summary>
    /// Показывать аналоговые часы. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowAnalogClock
    {
        get => _appDataService.Data.WidgetSettings.ShowAnalogClock;
        set
        {
            if (_appDataService.Data.WidgetSettings.ShowAnalogClock != value)
            {
                _appDataService.Data.WidgetSettings.ShowAnalogClock = value;
                OnPropertyChanged(nameof(ShowAnalogClock));
                UpdateWindowsVisibility();
            }
        }
    }
    /// <summary>
    /// Размер аналоговых часов. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double AnalogClockSize
    {
        get => _appDataService.Data.WidgetSettings.AnalogClockSize;
        set
        {
            var validatedValue = ValidateFontSize(value);
            if (_appDataService.Data.WidgetSettings.AnalogClockSize != validatedValue)
            {
                _appDataService.Data.WidgetSettings.AnalogClockSize = validatedValue;
                _analogClockSize = validatedValue;
                OnPropertyChanged();
                UpdateAnalogClockSize();
            }
        }
    }
    /// <summary>
    /// Аналоговые часы поверх всех окон. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool AnalogClockTopmost
    {
        get => _appDataService.Data.WidgetSettings.AnalogClockTopmost;
        set
        {
            if (_appDataService.Data.WidgetSettings.AnalogClockTopmost != value)
            {
                _appDataService.Data.WidgetSettings.AnalogClockTopmost = value;
                OnPropertyChanged();
                UpdateAnalogClockTopmost();
            }
        }
    }
    /// <summary>
    /// Цифровые часы поверх всех окон. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool DigitalClockTopmost
    {
        get => _appDataService.Data.WidgetSettings.DigitalClockTopmost;
        set
        {
            if (_appDataService.Data.WidgetSettings.DigitalClockTopmost != value)
            {
                _appDataService.Data.WidgetSettings.DigitalClockTopmost = value;
                OnPropertyChanged();
                UpdateDigitalClockTopmost();
            }
        }
    }
    /// <summary>
    /// Воспроизводить звук кукушки каждый час. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool CuckooEveryHour
    {
        get => _appDataService.Data.WidgetSettings.CuckooEveryHour;
        set
        {
            if (_appDataService.Data.WidgetSettings.CuckooEveryHour != value)
            {
                _logger.LogDebug($"[MainWindowViewModel.Properties] Updating CuckooEveryHour: {value}");
                _appDataService.Data.WidgetSettings.CuckooEveryHour = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Воспроизводить сигнал каждые полчаса. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool HalfHourChimeEnabled
    {
        get => _appDataService.Data.WidgetSettings.HalfHourChimeEnabled;
        set
        {
            if (_appDataService.Data.WidgetSettings.HalfHourChimeEnabled != value)
            {
                _logger.LogDebug($"[MainWindowViewModel.Properties] Updating HalfHourChimeEnabled: {value}");
                _appDataService.Data.WidgetSettings.HalfHourChimeEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

    private void UpdateAnalogClockTopmost()
    {
        var analogWindow = _windowService.GetAnalogClockWindow();
        if (analogWindow != null)
        {
            analogWindow.Topmost = AnalogClockTopmost;
        }
    }
    private void UpdateDigitalClockTopmost()
    {
        _windowService.SetMainWindowTopmost(DigitalClockTopmost);
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