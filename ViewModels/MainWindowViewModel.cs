using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly TimeService _timeService;
    private readonly SettingsService _settingsService;
    private string _timeText = string.Empty;
    private double _backgroundOpacity;
    private double _textOpacity;
    private double _fontSize;
    private bool _showSeconds;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string TimeText
    {
        get => _timeText;
        set
        {
            _timeText = value;
            OnPropertyChanged();
        }
    }

    public double BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            _backgroundOpacity = ValidateOpacity(value, 
                Constants.MIN_WINDOW_OPACITY, 
                Constants.MAX_WINDOW_OPACITY, 
                Constants.DEFAULT_WINDOW_OPACITY);
            OnPropertyChanged();
            _settingsService.UpdateSettings(s => s.BackgroundOpacity = _backgroundOpacity);
        }
    }

    public double TextOpacity
    {
        get => _textOpacity;
        set
        {
            _textOpacity = ValidateOpacity(value, 
                Constants.MIN_TEXT_OPACITY, 
                Constants.MAX_TEXT_OPACITY, 
                Constants.DEFAULT_TEXT_OPACITY);
            OnPropertyChanged();
            _settingsService.UpdateSettings(s => s.TextOpacity = _textOpacity);
        }
    }

    public double FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = ValidateFontSize(value);
            OnPropertyChanged();
            _settingsService.UpdateSettings(s => s.FontSize = _fontSize);
        }
    }

    public bool ShowSeconds
    {
        get => _showSeconds;
        set
        {
            _showSeconds = value;
            OnPropertyChanged();
            _settingsService.UpdateSettings(s => s.ShowSeconds = _showSeconds);
        }
    }

    public MainWindowViewModel()
    {
        _timeService = new TimeService();
        _settingsService = new SettingsService();
        
        // Загружаем настройки
        var settings = _settingsService.CurrentSettings;
        _backgroundOpacity = settings.BackgroundOpacity;
        _textOpacity = settings.TextOpacity;
        _fontSize = settings.FontSize;
        _showSeconds = settings.ShowSeconds;
        
        _timeService.TimeUpdated += OnTimeUpdated;
        _timeService.Start();
    }

    private void OnTimeUpdated(object? sender, DateTime time)
    {
        TimeText = time.ToString(_showSeconds ? 
            Constants.TIME_FORMAT_WITH_SECONDS : 
            Constants.TIME_FORMAT_WITHOUT_SECONDS);
    }

    private double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        return Math.Round(value / Constants.OPACITY_STEP) * Constants.OPACITY_STEP;
    }

    private double ValidateFontSize(double value)
    {
        if (value < Constants.MIN_FONT_SIZE || value > Constants.MAX_FONT_SIZE)
        {
            return Constants.DEFAULT_FONT_SIZE;
        }
        return Math.Round(value / Constants.FONT_SIZE_STEP) * Constants.FONT_SIZE_STEP;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SaveWindowPosition(double left, double top)
    {
        _settingsService.UpdateSettings(s =>
        {
            s.WindowLeft = left;
            s.WindowTop = top;
        });
    }

    public (double Left, double Top) GetWindowPosition()
    {
        var settings = _settingsService.CurrentSettings;
        return (
            settings.WindowLeft ?? Constants.DEFAULT_WINDOW_LEFT,
            settings.WindowTop ?? Constants.DEFAULT_WINDOW_TOP
        );
    }
} 