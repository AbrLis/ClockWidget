using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using System;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    private const double ANIMATION_STEP = 0.05;
    private const int ANIMATION_INTERVAL = 16;

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
                        _logger.Log<string>(Microsoft.Extensions.Logging.LogLevel.Warning, new EventId(), string.Empty, null, (s, e) => "Cannot hide both windows, keeping digital clock visible");
                        return;
                    }
                    _logger.Log<bool>(Microsoft.Extensions.Logging.LogLevel.Information, new EventId(), value, null, (v, e) => $"Updating show digital clock: {v}");
                    _showDigitalClock = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.ShowDigitalClock = value);
                    UpdateWindowsVisibility();
                }
                catch (Exception ex)
                {
                    _logger.Log<string>(Microsoft.Extensions.Logging.LogLevel.Error, new EventId(), string.Empty, ex, (s, e) => "Error updating show digital clock setting");
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
                        _logger.Log<string>(Microsoft.Extensions.Logging.LogLevel.Warning, new EventId(), string.Empty, null, (s, e) => "Cannot hide both windows, keeping analog clock visible");
                        return;
                    }
                    _logger.Log<bool>(Microsoft.Extensions.Logging.LogLevel.Information, new EventId(), value, null, (v, e) => $"Updating show analog clock: {v}");
                    _showAnalogClock = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.ShowAnalogClock = value);
                    UpdateWindowsVisibility();
                }
                catch (Exception ex)
                {
                    _logger.Log<string>(Microsoft.Extensions.Logging.LogLevel.Error, new EventId(), string.Empty, ex, (s, e) => "Error updating show analog clock setting");
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
                    _logger.Log<double>(Microsoft.Extensions.Logging.LogLevel.Information, new EventId(), value, null, (v, e) => $"Updating analog clock size: {v}");
                    _analogClockSize = value;
                    OnPropertyChanged();
                    _settingsService.UpdateSettings(s => s.AnalogClockSize = value);
                    UpdateAnalogClockSize();
                }
                catch (Exception ex)
                {
                    _logger.Log<string>(Microsoft.Extensions.Logging.LogLevel.Error, new EventId(), string.Empty, ex, (s, e) => "Error updating analog clock size");
                    _analogClockSize = value;
                    OnPropertyChanged();
                }
            }
        }
    }
} 