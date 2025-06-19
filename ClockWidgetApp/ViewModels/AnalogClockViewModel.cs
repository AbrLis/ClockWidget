using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;
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
    private TransformGroup _hourHandTransform;
    private TransformGroup _minuteHandTransform;
    private TransformGroup _secondHandTransform;
    private List<ClockTick> _clockTicks = new List<ClockTick>();
    private bool _disposed;

    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Получает прозрачность фона.
    /// </summary>
    public double BackgroundOpacity => App.MainViewModel.BackgroundOpacity;

    /// <summary>
    /// Получает прозрачность текста и стрелок.
    /// </summary>
    public double TextOpacity => App.MainViewModel.TextOpacity;

    /// <summary>
    /// Получает коллекцию рисок на циферблате.
    /// </summary>
    public List<ClockTick> ClockTicks
    {
        get => _clockTicks;
        private set
        {
            _clockTicks = value;
            OnPropertyChanged();
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
            _logger.LogInformation("[AnalogClockViewModel] Initializing analog clock view model");
            
            // Используем общий TimeService из App вместо создания нового
            _timeService = App.TimeService;
            _settingsService = App.SettingsService;
            
            // Инициализируем трансформации стрелок
            _hourHandTransform = new TransformGroup();
            _minuteHandTransform = new TransformGroup();
            _secondHandTransform = new TransformGroup();
            
            // Генерируем риски на циферблате
            GenerateClockTicks();
            
            // Подписываемся на обновление времени
            _timeService.TimeUpdated += OnTimeUpdated;

            // Немедленно обновляем стрелки текущим временем
            OnTimeUpdated(this, DateTime.Now);

            // Подписываемся на изменения настроек
            if (App.MainViewModel != null)
            {
                App.MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                _logger.LogInformation("[AnalogClockViewModel] Subscribed to MainViewModel property changes");
            }
            else
            {
                _logger.LogWarning("[AnalogClockViewModel] MainViewModel is not initialized");
            }
            
            _logger.LogInformation("[AnalogClockViewModel] Analog clock view model initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AnalogClockViewModel] Error initializing analog clock view model");
            throw;
        }
    }

    /// <summary>
    /// Генерирует риски на циферблате.
    /// </summary>
    private void GenerateClockTicks()
    {
        try
        {
            _logger.LogInformation("[AnalogClockViewModel] Generating clock ticks");
            
            var ticks = new List<ClockTick>();
            
            // Генерируем риски для каждой минуты (60 рисок)
            for (int minute = 0; minute < 60; minute++)
            {
                double angleInRadians = (minute * 6 - 90) * Math.PI / 180; // -90 для начала с 12 часов
                
                // Определяем длину и толщину риски
                double tickLength = (minute % 5 == 0) ? AnalogClockConstants.TickSizes.HOUR_TICK_LENGTH : AnalogClockConstants.TickSizes.MINUTE_TICK_LENGTH;
                double tickThickness = (minute % 5 == 0) ? AnalogClockConstants.TickSizes.HOUR_TICK_THICKNESS : AnalogClockConstants.TickSizes.MINUTE_TICK_THICKNESS;
                
                // Вычисляем координаты начальной и конечной точек риски
                double startRadius = AnalogClockConstants.Positioning.CLOCK_RADIUS - tickLength;
                double endRadius = AnalogClockConstants.Positioning.CLOCK_RADIUS;
                
                double startX = AnalogClockConstants.Positioning.CLOCK_CENTER_X + startRadius * Math.Cos(angleInRadians);
                double startY = AnalogClockConstants.Positioning.CLOCK_CENTER_Y + startRadius * Math.Sin(angleInRadians);
                double endX = AnalogClockConstants.Positioning.CLOCK_CENTER_X + endRadius * Math.Cos(angleInRadians);
                double endY = AnalogClockConstants.Positioning.CLOCK_CENTER_Y + endRadius * Math.Sin(angleInRadians);
                
                ticks.Add(new ClockTick(startX, startY, endX, endY, tickThickness));
            }
            
            ClockTicks = ticks;
            _logger.LogInformation("[AnalogClockViewModel] Generated {Count} clock ticks", ticks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AnalogClockViewModel] Error generating clock ticks");
        }
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_disposed)
        {
            _logger.LogWarning("[AnalogClockViewModel] PropertyChanged received after disposal");
            return;
        }

        if (e.PropertyName == nameof(App.MainViewModel.BackgroundOpacity) ||
            e.PropertyName == nameof(App.MainViewModel.TextOpacity))
        {
            _logger.LogDebug("[AnalogClockViewModel] Settings changed in main view model: {Property}", e.PropertyName);
            OnPropertyChanged(e.PropertyName);
        }
    }

    /// <summary>
    /// Получает сохраненную позицию окна.
    /// </summary>
    public (double Left, double Top) GetWindowPosition()
    {
        return WindowPositionHelper.GetWindowPosition(_settingsService, true);
    }

    /// <summary>
    /// Сохраняет позицию окна аналоговых часов.
    /// </summary>
    /// <param name="left">Координата Left.</param>
    /// <param name="top">Координата Top.</param>
    public void SaveWindowPosition(double left, double top)
    {
        WindowPositionHelper.SaveWindowPosition(_settingsService, left, top, true);
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
                _logger.LogWarning("[AnalogClockViewModel] TimeUpdated received after disposal");
                return;
            }

            _logger.LogDebug("[AnalogClockViewModel] Time updated: {Time}", time);
            
            // Вычисляем углы для стрелок
            double hourAngle = (time.Hour % 12 + time.Minute / 60.0) * 30; // 30 градусов на час
            double minuteAngle = time.Minute * 6; // 6 градусов на минуту
            double secondAngle = time.Second * 6; // 6 градусов на секунду

            // Обновляем трансформации через Dispatcher (UI-поток)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Обновляем только углы поворота, не создавая новые объекты
                if (_hourHandTransform.Children.Count > 0 && _hourHandTransform.Children[0] is System.Windows.Media.RotateTransform hourRotate)
                {
                    hourRotate.Angle = hourAngle;
                }
                else
                {
                    HourHandTransform = new System.Windows.Media.TransformGroup
                    {
                        Children = { new System.Windows.Media.RotateTransform(hourAngle, AnalogClockConstants.Positioning.CLOCK_CENTER_X, AnalogClockConstants.Positioning.CLOCK_CENTER_Y) }
                    };
                }

                if (_minuteHandTransform.Children.Count > 0 && _minuteHandTransform.Children[0] is System.Windows.Media.RotateTransform minuteRotate)
                {
                    minuteRotate.Angle = minuteAngle;
                }
                else
                {
                    MinuteHandTransform = new System.Windows.Media.TransformGroup
                    {
                        Children = { new System.Windows.Media.RotateTransform(minuteAngle, AnalogClockConstants.Positioning.CLOCK_CENTER_X, AnalogClockConstants.Positioning.CLOCK_CENTER_Y) }
                    };
                }

                if (_secondHandTransform.Children.Count > 0 && _secondHandTransform.Children[0] is System.Windows.Media.RotateTransform secondRotate)
                {
                    secondRotate.Angle = secondAngle;
                }
                else
                {
                    SecondHandTransform = new System.Windows.Media.TransformGroup
                    {
                        Children = { new System.Windows.Media.RotateTransform(secondAngle, AnalogClockConstants.Positioning.CLOCK_CENTER_X, AnalogClockConstants.Positioning.CLOCK_CENTER_Y) }
                    };
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AnalogClockViewModel] Error updating time");
        }
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
        if (!_disposed)
        {
            _timeService.TimeUpdated -= OnTimeUpdated;
            App.MainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
            _disposed = true;
        }
    }
} 