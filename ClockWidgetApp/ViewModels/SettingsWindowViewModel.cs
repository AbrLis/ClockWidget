using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для окна настроек. Связывает настройки главного окна с UI.
/// </summary>
public class SettingsWindowViewModel : INotifyPropertyChanged
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly ILogger<SettingsWindowViewModel> _logger;
    private double _backgroundOpacity;
    private double _textOpacity;
    private double _fontSize;
    private double _analogClockSize;
    private bool _showSeconds;

    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Получает или задает прозрачность фона. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double BackgroundOpacity
    {
        get => _mainViewModel.BackgroundOpacity;
        set
        {
            if (_mainViewModel.BackgroundOpacity != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating background opacity: {Value}", value);
                _mainViewModel.BackgroundOpacity = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Получает или задает прозрачность текста. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double TextOpacity
    {
        get => _mainViewModel.TextOpacity;
        set
        {
            if (_mainViewModel.TextOpacity != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating text opacity: {Value}", value);
                _mainViewModel.TextOpacity = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Получает или задает размер шрифта. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double FontSize
    {
        get => _mainViewModel.FontSize;
        set
        {
            if (_mainViewModel.FontSize != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating font size: {Value}", value);
                _mainViewModel.FontSize = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Получает или задает отображение секунд. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowSeconds
    {
        get => _mainViewModel.ShowSeconds;
        set
        {
            if (_mainViewModel.ShowSeconds != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating show seconds: {Value}", value);
                _mainViewModel.ShowSeconds = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Получает или задает отображение цифровых часов. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowDigitalClock
    {
        get => _mainViewModel.ShowDigitalClock;
        set
        {
            if (_mainViewModel.ShowDigitalClock != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating show digital clock: {Value}", value);
                _mainViewModel.ShowDigitalClock = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Получает или задает отображение аналоговых часов. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool ShowAnalogClock
    {
        get => _mainViewModel.ShowAnalogClock;
        set
        {
            if (_mainViewModel.ShowAnalogClock != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating show analog clock: {Value}", value);
                _mainViewModel.ShowAnalogClock = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Получает или устанавливает размер окна аналоговых часов. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public double AnalogClockSize
    {
        get => _mainViewModel.AnalogClockSize;
        set
        {
            if (Math.Abs(_mainViewModel.AnalogClockSize - value) > 0.001)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating analog clock size: {Value}", value);
                _mainViewModel.AnalogClockSize = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Воспроизводить звук кукушки каждый час. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool CuckooEveryHour
    {
        get => _mainViewModel.CuckooEveryHour;
        set
        {
            if (_mainViewModel.CuckooEveryHour != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating CuckooEveryHour: {Value}", value);
                _mainViewModel.CuckooEveryHour = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Цифровые часы поверх всех окон. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool DigitalClockTopmost
    {
        get => _mainViewModel.DigitalClockTopmost;
        set
        {
            if (_mainViewModel.DigitalClockTopmost != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating DigitalClockTopmost: {Value}", value);
                _mainViewModel.DigitalClockTopmost = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Аналоговые часы поверх всех окон. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool AnalogClockTopmost
    {
        get => _mainViewModel.AnalogClockTopmost;
        set
        {
            if (_mainViewModel.AnalogClockTopmost != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating AnalogClockTopmost: {Value}", value);
                _mainViewModel.AnalogClockTopmost = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Воспроизводить сигнал каждые полчаса. Изменения сохраняются только в буфере и будут записаны на диск при закрытии приложения.
    /// </summary>
    public bool HalfHourChimeEnabled
    {
        get => _mainViewModel.HalfHourChimeEnabled;
        set
        {
            if (_mainViewModel.HalfHourChimeEnabled != value)
            {
                _logger.LogInformation("[SettingsWindowViewModel] Updating HalfHourChimeEnabled: {Value}", value);
                _mainViewModel.HalfHourChimeEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Текущий язык интерфейса ("ru" или "en").
    /// </summary>
    public string Language
    {
        get => LocalizationManager.CurrentLanguage;
        set
        {
            if (LocalizationManager.CurrentLanguage != value)
            {
                LocalizationManager.SetLanguage(value);
                _mainViewModel.SettingsService.UpdateSettings(s => s.Language = value);
                _mainViewModel.SettingsService.SaveBufferedSettings();
            }
        }
    }
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

    /// <summary>
    /// Создает новый экземпляр <see cref="SettingsWindowViewModel"/>.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel для передачи настроек.</param>
    /// <param name="logger">Логгер для SettingsWindowViewModel.</param>
    public SettingsWindowViewModel(MainWindowViewModel mainViewModel, ILogger<SettingsWindowViewModel> logger)
    {
        _mainViewModel = mainViewModel;
        _logger = logger;
        _logger.LogInformation("[SettingsWindowViewModel] Settings window view model initialized");
        _backgroundOpacity = _mainViewModel.BackgroundOpacity;
        _textOpacity = _mainViewModel.TextOpacity;
        _fontSize = _mainViewModel.FontSize;
        _analogClockSize = _mainViewModel.AnalogClockSize;
        _showSeconds = _mainViewModel.ShowSeconds;
        Localized = LocalizationManager.GetLocalizedStrings();
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Localized));
        };
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocalizationManager.CurrentLanguage))
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Localized));
        }
        else
        {
            OnPropertyChanged(e.PropertyName);
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 