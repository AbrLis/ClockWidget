using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

public class SettingsWindowViewModel : INotifyPropertyChanged
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly ILogger<SettingsWindowViewModel> _logger = LoggingService.CreateLogger<SettingsWindowViewModel>();
    private double _backgroundOpacity;
    private double _textOpacity;
    private double _fontSize;
    private double _analogClockSize;
    private bool _showSeconds;

    public event PropertyChangedEventHandler? PropertyChanged;

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
    /// Получает или устанавливает размер окна аналоговых часов.
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

    public SettingsWindowViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _logger.LogInformation("[SettingsWindowViewModel] Settings window view model initialized");
        
        // Инициализируем значения из MainWindowViewModel
        _backgroundOpacity = _mainViewModel.BackgroundOpacity;
        _textOpacity = _mainViewModel.TextOpacity;
        _fontSize = _mainViewModel.FontSize;
        _analogClockSize = _mainViewModel.AnalogClockSize;
        _showSeconds = _mainViewModel.ShowSeconds;
        
        // Подписываемся на изменения свойств MainWindowViewModel
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("[SettingsWindowViewModel] Property changed in main view model: {Property}", e.PropertyName);
        // Пробрасываем уведомления об изменениях свойств
        OnPropertyChanged(e.PropertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _logger.LogDebug("[SettingsWindowViewModel] Property changed in settings view model: {Property}", propertyName);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 