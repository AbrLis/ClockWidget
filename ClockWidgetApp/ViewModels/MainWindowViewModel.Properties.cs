using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel главного окна приложения. Содержит свойства и методы для управления отображением и настройками виджетов часов.
/// </summary>
public partial class MainWindowViewModel
{
    #region Private Fields
    /// <summary>Текст времени для отображения.</summary>
    private string _timeText = string.Empty;
    /// <summary>Флаг отображения секунд.</summary>
    private bool _showSeconds;
    /// <summary>Флаг отображения цифровых часов.</summary>
    private bool _showDigitalClock = true;
    /// <summary>Флаг отображения аналоговых часов.</summary>
    private bool _showAnalogClock = true;
    /// <summary>Размер аналоговых часов.</summary>
    private double _analogClockSize;
    /// <summary>Флаг "поверх всех окон" для аналоговых часов.</summary>
    private bool _analogClockTopmost = true;
    /// <summary>Флаг "поверх всех окон" для цифровых часов.</summary>
    private bool _digitalClockTopmost = true;
    #endregion

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
                _logger.LogInformation($"[BackgroundOpacity] Changed: {validatedValue}");
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
                _logger.LogInformation($"[TextOpacity] Changed: {validatedValue}");
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
                _logger.LogInformation($"[FontSize] Changed: {validatedValue}");
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
            _logger.LogInformation($"[ShowSeconds] Changed: {value}");
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
            _logger.LogInformation($"[ShowDigitalClock] Changed: {value}");
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
            _logger.LogInformation($"[ShowAnalogClock] Changed: {value}");
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
            var validatedValue = WidgetSettings.ValidateAnalogClockSize(value);
            if (_appDataService.Data.WidgetSettings.AnalogClockSize != validatedValue)
            {
                _logger.LogInformation($"[AnalogClockSize] Changed: {validatedValue}");
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
            _logger.LogInformation($"[AnalogClockTopmost] Changed: {value}");
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
            _logger.LogInformation($"[DigitalClockTopmost] Changed: {value}");
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
            _logger.LogInformation($"[CuckooEveryHour] Changed: {value}");
            if (_appDataService.Data.WidgetSettings.CuckooEveryHour != value)
            {
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
            _logger.LogInformation($"[HalfHourChimeEnabled] Changed: {value}");
            if (_appDataService.Data.WidgetSettings.HalfHourChimeEnabled != value)
            {
                _appDataService.Data.WidgetSettings.HalfHourChimeEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Локализованные строки для UI.
    /// </summary>
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

    #region Private Methods & Event Handlers
    /// <summary>
    /// Обновляет свойство Topmost для окна аналоговых часов.
    /// </summary>
    private void UpdateAnalogClockTopmost()
    {
        var analogWindow = _windowService.GetAnalogClockWindow();
        if (analogWindow != null)
        {
            analogWindow.Topmost = AnalogClockTopmost;
        }
    }

    /// <summary>
    /// Обновляет свойство Topmost для главного окна (цифровых часов).
    /// </summary>
    private void UpdateDigitalClockTopmost()
    {
        _windowService.SetMainWindowTopmost(DigitalClockTopmost);
    }

    /// <summary>
    /// Подписка на событие смены языка.
    /// </summary>
    private void SubscribeToLanguageChanges()
    {
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
    }
    #endregion
} 