using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для окна с аналоговыми часами.
/// Управляет отображением стрелок, прозрачностью и другими настройками.
/// </summary>
public class AnalogClockViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly TimeService _timeService;
    private readonly SettingsService _settingsService;
    private readonly ILogger<AnalogClockViewModel> _logger = LoggingService.CreateLogger<AnalogClockViewModel>();
    private double _backgroundOpacity;
    private double _textOpacity;
    private TransformGroup _hourHandTransform;
    private TransformGroup _minuteHandTransform;
    private TransformGroup _secondHandTransform;
    private bool _disposed;

    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Получает или устанавливает прозрачность фона.
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
    /// Получает или устанавливает прозрачность текста и стрелок.
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
    /// Получает трансформацию для часовой стрелки.
    /// </summary>
    public TransformGroup HourHandTransform
    {
        get => _hourHandTransform;
        private set
        {
            _hourHandTransform = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Получает трансформацию для минутной стрелки.
    /// </summary>
    public TransformGroup MinuteHandTransform
    {
        get => _minuteHandTransform;
        private set
        {
            _minuteHandTransform = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Получает трансформацию для секундной стрелки.
    /// </summary>
    public TransformGroup SecondHandTransform
    {
        get => _secondHandTransform;
        private set
        {
            _secondHandTransform = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AnalogClockViewModel"/>.
    /// </summary>
    public AnalogClockViewModel()
    {
        try
        {
            _logger.LogInformation("Initializing analog clock view model");
            
            _timeService = new TimeService();
            _settingsService = App.SettingsService;
            
            // Инициализируем трансформации стрелок
            _hourHandTransform = new TransformGroup();
            _minuteHandTransform = new TransformGroup();
            _secondHandTransform = new TransformGroup();
            
            // Загружаем настройки
            var settings = _settingsService.CurrentSettings;
            _logger.LogInformation("Loading settings for analog clock: {Settings}", 
                System.Text.Json.JsonSerializer.Serialize(settings));
                
            _backgroundOpacity = settings.BackgroundOpacity;
            _textOpacity = settings.TextOpacity;
            
            // Подписываемся на обновление времени
            _timeService.TimeUpdated += OnTimeUpdated;
            _timeService.Start();
            
            _logger.LogInformation("Analog clock view model initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing analog clock view model");
            throw;
        }
    }

    /// <summary>
    /// Получает сохраненную позицию окна.
    /// </summary>
    public (double Left, double Top) GetWindowPosition()
    {
        var settings = _settingsService.CurrentSettings;
        return (
            settings.AnalogClockLeft ?? Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_LEFT,
            settings.AnalogClockTop ?? Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_TOP
        );
    }

    /// <summary>
    /// Сохраняет позицию окна.
    /// </summary>
    public void SaveWindowPosition(double left, double top)
    {
        _settingsService.UpdateSettings(s =>
        {
            s.AnalogClockLeft = left;
            s.AnalogClockTop = top;
        });
    }

    /// <summary>
    /// Обработчик события обновления времени.
    /// Обновляет углы поворота стрелок.
    /// </summary>
    private void OnTimeUpdated(object? sender, DateTime time)
    {
        try
        {
            if (_disposed)
            {
                _logger.LogWarning("TimeUpdated received after disposal");
                return;
            }

            _logger.LogDebug("Time updated: {Time}", time);
            
            // Вычисляем углы для стрелок
            double hourAngle = (time.Hour % 12 + time.Minute / 60.0) * 30; // 30 градусов на час
            double minuteAngle = time.Minute * 6; // 6 градусов на минуту
            double secondAngle = time.Second * 6; // 6 градусов на секунду

            // Обновляем трансформации через Dispatcher (UI-поток)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HourHandTransform = new System.Windows.Media.TransformGroup
                {
                    Children = { new System.Windows.Media.RotateTransform(hourAngle, 125, 125) }
                };

                MinuteHandTransform = new System.Windows.Media.TransformGroup
                {
                    Children = { new System.Windows.Media.RotateTransform(minuteAngle, 125, 125) }
                };

                SecondHandTransform = new System.Windows.Media.TransformGroup
                {
                    Children = { new System.Windows.Media.RotateTransform(secondAngle, 125, 125) }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating time");
        }
    }

    /// <summary>
    /// Проверяет и корректирует значение прозрачности.
    /// </summary>
    private double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        return Math.Round(value / Constants.WindowSettings.OPACITY_STEP) * Constants.WindowSettings.OPACITY_STEP;
    }

    /// <summary>
    /// Вызывает событие изменения свойства.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Освобождает ресурсы, используемые экземпляром класса <see cref="AnalogClockViewModel"/>.
    /// </summary>
    public void Dispose()
    {
        try
        {
            _logger.LogInformation("Disposing analog clock view model");
            Dispose(true);
            GC.SuppressFinalize(this);
            _logger.LogInformation("Analog clock view model disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing analog clock view model");
        }
    }

    /// <summary>
    /// Освобождает неуправляемые ресурсы и опционально освобождает управляемые ресурсы.
    /// </summary>
    /// <param name="disposing">Значение true позволяет освободить управляемые и неуправляемые ресурсы; значение false позволяет освободить только неуправляемые ресурсы.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    _logger.LogDebug("Disposing managed resources");
                    // Останавливаем сервис времени
                    _timeService.Stop();
                    _timeService.TimeUpdated -= OnTimeUpdated;
                    if (_timeService is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    _logger.LogDebug("Managed resources disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing managed resources");
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Деструктор для освобождения неуправляемых ресурсов.
    /// </summary>
    ~AnalogClockViewModel()
    {
        Dispose(false);
    }
} 