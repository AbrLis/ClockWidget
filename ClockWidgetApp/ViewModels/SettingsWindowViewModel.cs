using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

public class SettingsWindowViewModel : INotifyPropertyChanged
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly ILogger<SettingsWindowViewModel> _logger = LoggingService.CreateLogger<SettingsWindowViewModel>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public double BackgroundOpacity
    {
        get => _mainViewModel.BackgroundOpacity;
        set
        {
            if (_mainViewModel.BackgroundOpacity != value)
            {
                _logger.LogInformation("Updating background opacity: {Value}", value);
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
                _logger.LogInformation("Updating text opacity: {Value}", value);
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
                _logger.LogInformation("Updating font size: {Value}", value);
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
                _logger.LogInformation("Updating show seconds: {Value}", value);
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
                _logger.LogInformation("Updating show digital clock: {Value}", value);
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
                _logger.LogInformation("Updating show analog clock: {Value}", value);
                _mainViewModel.ShowAnalogClock = value;
                OnPropertyChanged();
            }
        }
    }

    public SettingsWindowViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _logger.LogInformation("Settings window view model initialized");
        
        // Подписываемся на изменения свойств MainWindowViewModel
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("Property changed in main view model: {Property}", e.PropertyName);
        // Пробрасываем уведомления об изменениях свойств
        OnPropertyChanged(e.PropertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _logger.LogDebug("Property changed in settings view model: {Property}", propertyName);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 