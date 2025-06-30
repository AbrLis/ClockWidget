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
    private readonly ITimeService _timeService;
    private readonly ISettingsService _settingsService;
    private readonly MainWindowViewModel _mainViewModel;
    private readonly ILogger<AnalogClockViewModel> _logger;
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
    public double BackgroundOpacity => _mainViewModel.BackgroundOpacity;

    /// <summary>
    /// Получает прозрачность текста и стрелок.
    /// </summary>
    public double TextOpacity => _mainViewModel.TextOpacity;

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
    public AnalogClockViewModel(ITimeService timeService, ISettingsService settingsService, MainWindowViewModel mainViewModel, ILogger<AnalogClockViewModel> logger)
    {
        try
        {
            _logger = logger;
            _logger.LogDebug("[AnalogClockViewModel] Initializing analog clock view model");
            _timeService = timeService;
            _settingsService = settingsService;
            _mainViewModel = mainViewModel;
            _hourHandTransform = new TransformGroup();
            _minuteHandTransform = new TransformGroup();
            _secondHandTransform = new TransformGroup();
            GenerateClockTicks();
            _timeService.TimeUpdated += OnTimeUpdated;
            OnTimeUpdated(this, DateTime.Now);
            _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            _logger.LogDebug("[AnalogClockViewModel] Subscribed to MainViewModel property changes");
            _logger.LogDebug("[AnalogClockViewModel] Analog clock view model initialized");
        }
        catch (Exception ex)
        {
            if (_logger != null)
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
            _logger.LogDebug("[AnalogClockViewModel] Generating clock ticks");
            
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
            _logger.LogDebug("[AnalogClockViewModel] Generated {Count} clock ticks", ticks.Count);
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

        if (e.PropertyName == nameof(_mainViewModel.BackgroundOpacity) ||
            e.PropertyName == nameof(_mainViewModel.TextOpacity))
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
            _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
            _disposed = true;
        }
    }
} 