using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using System.Windows;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для главного окна виджета часов.
/// Управляет отображением времени, прозрачностью и другими настройками виджета.
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged, ISettingsViewModel
{
    private readonly TimeService _timeService;
    private readonly SettingsService _settingsService;
    private readonly ILogger<MainWindowViewModel> _logger = LoggingService.CreateLogger<MainWindowViewModel>();
    private string _timeText = string.Empty;
    private double _backgroundOpacity;
    private double _textOpacity;
    private double _fontSize;
    private bool _showSeconds;
    private bool _showDigitalClock = true;
    private bool _showAnalogClock = true;
    private AnalogClockWindow? _analogClockWindow;
    private const double ANIMATION_STEP = 0.05; // Шаг анимации
    private const int ANIMATION_INTERVAL = 16; // Интервал анимации (приблизительно 60 FPS)

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
    /// Получает или устанавливает прозрачность текста виджета.
    /// Значение должно быть в диапазоне от <see cref="Constants.TextSettings.MIN_TEXT_OPACITY"/> до <see cref="Constants.TextSettings.MAX_TEXT_OPACITY"/>.
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
    /// Получает или устанавливает размер шрифта текста.
    /// Значение должно быть в диапазоне от <see cref="Constants.TextSettings.MIN_FONT_SIZE"/> до <see cref="Constants.TextSettings.MAX_FONT_SIZE"/>.
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
                try
                {
                    // Не позволяем скрыть оба окна
                    if (!value && !_showAnalogClock)
                    {
                        _logger.LogWarning("Cannot hide both windows, keeping digital clock visible");
                        return;
                    }

                    _logger.LogInformation("Updating show digital clock: {Value}", value);
                    _showDigitalClock = value;
                    OnPropertyChanged();

                    // Сохраняем настройки
                    _settingsService.UpdateSettings(s => s.ShowDigitalClock = value);
                    
                    // Обновляем видимость окон
                    UpdateWindowsVisibility();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating show digital clock setting");
                    // Восстанавливаем предыдущее значение
                    _showDigitalClock = !value;
                    OnPropertyChanged();
                }
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
                try
                {
                    // Не позволяем скрыть оба окна
                    if (!value && !_showDigitalClock)
                    {
                        _logger.LogWarning("Cannot hide both windows, keeping analog clock visible");
                        return;
                    }

                    _logger.LogInformation("Updating show analog clock: {Value}", value);
                    _showAnalogClock = value;
                    OnPropertyChanged();

                    // Сохраняем настройки
                    _settingsService.UpdateSettings(s => s.ShowAnalogClock = value);
                    
                    // Обновляем видимость окон
                    UpdateWindowsVisibility();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating show analog clock setting");
                    // Восстанавливаем предыдущее значение
                    _showAnalogClock = !value;
                    OnPropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindowViewModel"/>.
    /// Загружает сохраненные настройки и запускает сервис обновления времени.
    /// </summary>
    public MainWindowViewModel()
    {
        try
        {
            _logger.LogInformation("Initializing main window view model");
            
            _timeService = new TimeService();
            _settingsService = App.SettingsService;
            
            // Загружаем настройки
            var settings = _settingsService.CurrentSettings;
            _logger.LogInformation("Loading settings for main window: {Settings}", 
                JsonSerializer.Serialize(settings));
                
            // Устанавливаем начальные значения
            _backgroundOpacity = settings.BackgroundOpacity;
            _textOpacity = settings.TextOpacity;
            _fontSize = settings.FontSize;
            _showSeconds = settings.ShowSeconds;
            
            // Устанавливаем значения видимости окон
            _showDigitalClock = settings.ShowDigitalClock;
            _showAnalogClock = settings.ShowAnalogClock;
            
            // Запускаем сервис обновления времени и подписываемся на события
            _timeService.TimeUpdated += OnTimeUpdated;
            _timeService.Start();
            
            // Немедленно обновляем время
            OnTimeUpdated(this, DateTime.Now);
            
            // Обновляем видимость окон в соответствии с настройками
            _logger.LogInformation("Initial windows visibility: Digital={ShowDigital}, Analog={ShowAnalog}", 
                _showDigitalClock, _showAnalogClock);
            
            // Отложенное обновление видимости окон, чтобы дать время на инициализацию
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateWindowsVisibility();
                _logger.LogInformation("Windows visibility updated after initialization");
            }));
            
            _logger.LogInformation("Main window view model initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing main window view model");
            throw;
        }
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
    /// Получает сохраненную позицию окна.
    /// </summary>
    /// <returns>Кортеж с координатами X и Y окна.</returns>
    public (double Left, double Top) GetWindowPosition()
    {
        return WindowPositionHelper.GetWindowPosition(_settingsService, false);
    }

    /// <summary>
    /// Сохраняет текущую позицию окна.
    /// </summary>
    /// <param name="left">Координата X окна.</param>
    /// <param name="top">Координата Y окна.</param>
    public void SaveWindowPosition(double left, double top)
    {
        WindowPositionHelper.SaveWindowPosition(_settingsService, left, top, false);
    }

    /// <summary>
    /// Обновляет видимость окон в соответствии с настройками.
    /// </summary>
    private void UpdateWindowsVisibility()
    {
        try
        {
            _logger.LogInformation("Updating windows visibility: Digital={ShowDigital}, Analog={ShowAnalog}", 
                _showDigitalClock, _showAnalogClock);

            // Обновляем видимость основного окна
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var newVisibility = _showDigitalClock ? Visibility.Visible : Visibility.Hidden;
                if (mainWindow.Visibility != newVisibility)
                {
                    mainWindow.Visibility = newVisibility;
                    if (newVisibility == Visibility.Visible)
                    {
                        mainWindow.Show();
                        mainWindow.Activate();
                        _logger.LogInformation("Main window shown and activated");
                    }
                    else
                    {
                        mainWindow.Hide();
                        _logger.LogInformation("Main window hidden");
                    }
                }
            }
            else
            {
                _logger.LogWarning("Main window is not of type MainWindow");
            }

            // Обновляем видимость окна аналоговых часов
            if (_showAnalogClock)
            {
                if (_analogClockWindow == null)
                {
                    _logger.LogInformation("Creating analog clock window");
                    _analogClockWindow = new AnalogClockWindow();
                    var (left, top) = GetAnalogClockPosition();
                    _analogClockWindow.Left = left;
                    _analogClockWindow.Top = top;
                    _analogClockWindow.Show();
                }
                else if (!_analogClockWindow.IsVisible)
                {
                    _analogClockWindow.Show();
                }
                
                _analogClockWindow.Activate();
                _logger.LogInformation("Analog clock window shown and activated at position: Left={Left}, Top={Top}", 
                    _analogClockWindow.Left, _analogClockWindow.Top);
            }
            else if (_analogClockWindow != null && _analogClockWindow.IsVisible)
            {
                _logger.LogInformation("Hiding analog clock window");
                _analogClockWindow.Hide();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating windows visibility");
        }
    }

    private (double Left, double Top) GetAnalogClockPosition()
    {
        return WindowPositionHelper.GetWindowPosition(_settingsService, true);
    }
} 