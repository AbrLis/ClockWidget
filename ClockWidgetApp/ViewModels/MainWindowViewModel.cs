using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using System.Windows;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для главного окна виджета часов.
/// Управляет отображением времени, прозрачностью и другими настройками виджета.
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged, ISettingsViewModel
{
    private readonly TimeService _timeService;
    private readonly SettingsService _settingsService;
    private readonly ILogger<MainWindowViewModel> _logger;
    private string _timeText = string.Empty;
    private double _backgroundOpacity;
    private double _textOpacity;
    private double _fontSize;
    private bool _showSeconds;
    private bool _showDigitalClock = true;
    private bool _showAnalogClock = true;

    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Получает или устанавливает текст времени, отображаемый в виджете.
    /// </summary>
    public string TimeText
    {
        get => _timeText;
        set
        {
            _timeText = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Получает или устанавливает прозрачность фона виджета.
    /// Значение должно быть в диапазоне от <see cref="Constants.WindowSettings.MIN_WINDOW_OPACITY"/> до <see cref="Constants.WindowSettings.MAX_WINDOW_OPACITY"/>.
    /// </summary>
    public double BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            _backgroundOpacity = ValidateOpacity(value, 
                Constants.WindowSettings.MIN_WINDOW_OPACITY, 
                Constants.WindowSettings.MAX_WINDOW_OPACITY, 
                Constants.WindowSettings.DEFAULT_WINDOW_OPACITY);
            OnPropertyChanged();
            _settingsService.UpdateSettings(s => s.BackgroundOpacity = _backgroundOpacity);
        }
    }

    /// <summary>
    /// Получает или устанавливает прозрачность текста виджета.
    /// Значение должно быть в диапазоне от <see cref="Constants.TextSettings.MIN_TEXT_OPACITY"/> до <see cref="Constants.TextSettings.MAX_TEXT_OPACITY"/>.
    /// </summary>
    public double TextOpacity
    {
        get => _textOpacity;
        set
        {
            _textOpacity = ValidateOpacity(value, 
                Constants.TextSettings.MIN_TEXT_OPACITY, 
                Constants.TextSettings.MAX_TEXT_OPACITY, 
                Constants.TextSettings.DEFAULT_TEXT_OPACITY);
            OnPropertyChanged();
            _settingsService.UpdateSettings(s => s.TextOpacity = _textOpacity);
        }
    }

    /// <summary>
    /// Получает или устанавливает размер шрифта текста.
    /// Значение должно быть в диапазоне от <see cref="Constants.TextSettings.MIN_FONT_SIZE"/> до <see cref="Constants.TextSettings.MAX_FONT_SIZE"/>.
    /// </summary>
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

    /// <summary>
    /// Получает или устанавливает флаг отображения секунд в виджете.
    /// </summary>
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

    /// <summary>
    /// Получает или устанавливает флаг отображения цифровых часов.
    /// </summary>
    public bool ShowDigitalClock
    {
        get => _showDigitalClock;
        set
        {
            if (_showDigitalClock != value)
            {
                // Не позволяем скрыть оба окна
                if (!value && !_showAnalogClock)
                {
                    return;
                }
                _showDigitalClock = value;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.ShowDigitalClock = value);
                UpdateWindowsVisibility();
            }
        }
    }

    /// <summary>
    /// Получает или устанавливает флаг отображения аналоговых часов.
    /// </summary>
    public bool ShowAnalogClock
    {
        get => _showAnalogClock;
        set
        {
            if (_showAnalogClock != value)
            {
                // Не позволяем скрыть оба окна
                if (!value && !_showDigitalClock)
                {
                    return;
                }
                _showAnalogClock = value;
                OnPropertyChanged();
                _settingsService.UpdateSettings(s => s.ShowAnalogClock = value);
                UpdateWindowsVisibility();
            }
        }
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindowViewModel"/>.
    /// Загружает сохраненные настройки и запускает сервис обновления времени.
    /// </summary>
    public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
    {
        _timeService = new TimeService();
        _settingsService = App.SettingsService;
        _logger = logger;
        
        // Загружаем настройки
        var settings = _settingsService.CurrentSettings;
        _logger.LogInformation("Loading settings for main window: {Settings}", 
            JsonSerializer.Serialize(settings));
            
        _backgroundOpacity = settings.BackgroundOpacity;
        _textOpacity = settings.TextOpacity;
        _fontSize = settings.FontSize;
        _showSeconds = settings.ShowSeconds;
        _showDigitalClock = settings.ShowDigitalClock;
        _showAnalogClock = settings.ShowAnalogClock;
        
        _timeService.TimeUpdated += OnTimeUpdated;
        _timeService.Start();
    }

    /// <summary>
    /// Обработчик события обновления времени.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="time">Новое значение времени.</param>
    private void OnTimeUpdated(object? sender, DateTime time)
    {
        TimeText = time.ToString(_showSeconds ? 
            Constants.DisplaySettings.TIME_FORMAT_WITH_SECONDS : 
            Constants.DisplaySettings.TIME_FORMAT_WITHOUT_SECONDS);
    }

    /// <summary>
    /// Проверяет и корректирует значение прозрачности.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="minValue">Минимальное допустимое значение.</param>
    /// <param name="maxValue">Максимальное допустимое значение.</param>
    /// <param name="defaultValue">Значение по умолчанию.</param>
    /// <returns>Скорректированное значение прозрачности.</returns>
    private double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        return Math.Round(value / Constants.WindowSettings.OPACITY_STEP) * Constants.WindowSettings.OPACITY_STEP;
    }

    /// <summary>
    /// Проверяет и корректирует значение размера шрифта.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Скорректированное значение размера шрифта.</returns>
    private double ValidateFontSize(double value)
    {
        if (value < Constants.TextSettings.MIN_FONT_SIZE || value > Constants.TextSettings.MAX_FONT_SIZE)
        {
            return Constants.TextSettings.DEFAULT_FONT_SIZE;
        }
        return Math.Round(value / Constants.TextSettings.FONT_SIZE_STEP) * Constants.TextSettings.FONT_SIZE_STEP;
    }

    /// <summary>
    /// Вызывает событие <see cref="PropertyChanged"/>.
    /// </summary>
    /// <param name="propertyName">Имя изменившегося свойства.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Сохраняет текущую позицию окна.
    /// </summary>
    /// <param name="left">Координата X окна.</param>
    /// <param name="top">Координата Y окна.</param>
    public void SaveWindowPosition(double left, double top)
    {
        _settingsService.UpdateSettings(s =>
        {
            s.WindowLeft = left;
            s.WindowTop = top;
        });
    }

    /// <summary>
    /// Получает сохраненную позицию окна.
    /// </summary>
    /// <returns>Кортеж с координатами X и Y окна.</returns>
    public (double Left, double Top) GetWindowPosition()
    {
        var settings = _settingsService.CurrentSettings;
        return (
            settings.WindowLeft ?? Constants.WindowSettings.DEFAULT_WINDOW_LEFT,
            settings.WindowTop ?? Constants.WindowSettings.DEFAULT_WINDOW_TOP
        );
    }

    /// <summary>
    /// Обновляет видимость окон в соответствии с настройками.
    /// </summary>
    private void UpdateWindowsVisibility()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.Visibility = ShowDigitalClock ? Visibility.Visible : Visibility.Hidden;
        }

        // Находим окно аналоговых часов
        foreach (Window window in Application.Current.Windows)
        {
            if (window is AnalogClockWindow analogClockWindow)
            {
                analogClockWindow.Visibility = ShowAnalogClock ? Visibility.Visible : Visibility.Hidden;
                break;
            }
        }
    }
} 