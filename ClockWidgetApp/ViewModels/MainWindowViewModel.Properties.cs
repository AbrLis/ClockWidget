using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

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
    /// <summary>Размер аналоговых часов.</summary>
    private double _analogClockSize;
    #endregion

    /// <summary>
    /// Сравнивает два значения double с заданной точностью (эпсилон).
    /// </summary>
    private static bool AreClose(double a, double b, double epsilon = 1e-6)
        => Math.Abs(a - b) < epsilon;

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
            if (AreClose(_appDataService.Data.WidgetSettings.BackgroundOpacity, validatedValue)) return;
            _logger.LogInformation($"[BackgroundOpacity] Changed: {validatedValue}");
            _appDataService.Data.WidgetSettings.BackgroundOpacity = validatedValue;
            OnPropertyChanged();
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
            if (AreClose(_appDataService.Data.WidgetSettings.TextOpacity, validatedValue)) return;
            _logger.LogInformation($"[TextOpacity] Changed: {validatedValue}");
            _appDataService.Data.WidgetSettings.TextOpacity = validatedValue;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Размер шрифта.
    /// </summary>
    public double FontSize
    {
        get => _appDataService.Data.WidgetSettings.FontSize;
        set
        {
            var validatedValue = ValidateFontSize(value);
            if (AreClose(_appDataService.Data.WidgetSettings.FontSize, validatedValue)) return;
            _logger.LogInformation($"[FontSize] Changed: {validatedValue}");
            _appDataService.Data.WidgetSettings.FontSize = validatedValue;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Показывать секунды.
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
    /// Показывать цифровые часы.
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
                OnPropertyChanged();
                UpdateWindowsVisibility();
            }
        }
    }

    /// <summary>
    /// Показывать аналоговые часы.
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
                OnPropertyChanged();
                UpdateWindowsVisibility();
            }
        }
    }

    /// <summary>
    /// Размер аналоговых часов.
    /// </summary>
    public double AnalogClockSize
    {
        get => _appDataService.Data.WidgetSettings.AnalogClockSize;
        set
        {
            var validatedValue = WidgetSettings.ValidateAnalogClockSize(value);
            if (AreClose(_appDataService.Data.WidgetSettings.AnalogClockSize, validatedValue)) return;
            _logger.LogInformation($"[AnalogClockSize] Changed: {validatedValue}");
            _appDataService.Data.WidgetSettings.AnalogClockSize = validatedValue;
            _analogClockSize = validatedValue;
            OnPropertyChanged();
            UpdateAnalogClockSize();
        }
    }

    /// <summary>
    /// Аналоговые часы поверх всех окон.
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
    /// Цифровые часы поверх всех окон.
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
    /// Воспроизводить звук кукушки каждый час.
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
    /// Воспроизводить сигнал каждые полчаса.
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
        LocalizationManager.LanguageChanged += (_, _) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
        };
    }
    #endregion
} 