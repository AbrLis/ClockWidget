using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для окна с аналоговыми часами. Управляет отображением стрелок, прозрачностью и другими настройками.
/// </summary>
public class AnalogClockViewModel : INotifyPropertyChanged, IDisposable
{
    #region Fields
    /// <summary>Сервис времени.</summary>
    private readonly ITimeService _timeService;
    /// <summary>Сервис настроек.</summary>
    private readonly IAppDataService _appDataService;
    /// <summary>Главный ViewModel приложения.</summary>
    private readonly MainWindowViewModel _mainViewModel;
    /// <summary>Трансформация часовой стрелки.</summary>
    private TransformGroup _hourHandTransform;
    /// <summary>Трансформация минутной стрелки.</summary>
    private TransformGroup _minuteHandTransform;
    /// <summary>Трансформация секундной стрелки.</summary>
    private TransformGroup _secondHandTransform;
    /// <summary>Коллекция рисок циферблата.</summary>
    private List<ClockTick> _clockTicks = new();
    /// <summary>Флаг освобождения ресурсов.</summary>
    private bool _disposed;
    /// <summary>Флаг активного перемещения окна.</summary>
    private bool _isDragging;
    /// <summary>Точка начала drag and drop.</summary>
    private System.Windows.Point _dragStartPoint;

    #endregion

    /// <summary>Команда открытия окна настроек.</summary>
    public ICommand OpenSettingsCommand { get; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Прозрачность фона.</summary>
    public double BackgroundOpacity => _mainViewModel.BackgroundOpacity;
    /// <summary>Прозрачность текста и стрелок.</summary>
    public double TextOpacity => _mainViewModel.TextOpacity;

    /// <summary>Коллекция рисок на циферблате.</summary>
    public List<ClockTick> ClockTicks
    {
        get => _clockTicks;
        private set { _clockTicks = value; OnPropertyChanged(); }
    }

    /// <summary>Трансформация для часовой стрелки.</summary>
    public TransformGroup HourHandTransform
    {
        get => _hourHandTransform;
        private set { _hourHandTransform = value; OnPropertyChanged(); }
    }

    /// <summary>Трансформация для минутной стрелки.</summary>
    public TransformGroup MinuteHandTransform
    {
        get => _minuteHandTransform;
        private set { _minuteHandTransform = value; OnPropertyChanged(); }
    }

    /// <summary>Трансформация для секундной стрелки.</summary>
    public TransformGroup SecondHandTransform
    {
        get => _secondHandTransform;
        private set { _secondHandTransform = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AnalogClockViewModel"/>.
    /// </summary>
    public AnalogClockViewModel(ITimeService timeService, IAppDataService appDataService, MainWindowViewModel mainViewModel, IWindowService? windowService = null)
    {
        _timeService = timeService;
        _appDataService = appDataService;
        _mainViewModel = mainViewModel;
        var windowService1 = windowService;
        _hourHandTransform = new TransformGroup();
        _minuteHandTransform = new TransformGroup();
        _secondHandTransform = new TransformGroup();
        GenerateClockTicks();
        _timeService.TimeUpdated += OnTimeUpdated;
        OnTimeUpdated(this, DateTime.Now);
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        OpenSettingsCommand = new RelayCommand(_ => windowService1?.OpenSettingsWindow());
    }

    /// <summary>Получает сохраненную позицию окна.</summary>
    public (double Left, double Top) GetWindowPosition() => WindowPositionHelper.GetWindowPosition(_appDataService, true);

    /// <summary>
    /// Сохраняет позицию окна аналоговых часов
    /// </summary>
    public void SaveWindowPosition(double left, double top)
    {
        WindowPositionHelper.SaveWindowPosition(_appDataService, left, top, true);
    }

    /// <summary>Вызывает событие изменения свойства.</summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>Начать перемещение окна. Запоминает стартовую точку.</summary>
    public void StartDrag(System.Windows.Point startPoint)
    {
        _isDragging = true;
        _dragStartPoint = startPoint;
    }

    /// <summary>Выполнить перемещение окна. Возвращает смещение (delta) относительно стартовой точки, если перемещение активно.</summary>
    public (double deltaX, double deltaY)? DragMove(System.Windows.Point currentPoint)
    {
        if (!_isDragging) return null;
        var delta = currentPoint - _dragStartPoint;
        return (delta.X, delta.Y);
    }

    /// <summary>Завершить перемещение окна.</summary>
    public void StopDrag() => _isDragging = false;

    #region Event Handlers & Private Methods

    /// <summary>Генерирует риски на циферблате.</summary>
    private void GenerateClockTicks()
    {
        var ticks = new List<ClockTick>();
        for (int minute = 0; minute < 60; minute++)
        {
            double angleInRadians = (minute * 6 - 90) * Math.PI / 180;
            double tickLength = (minute % 5 == 0) ? AnalogClockConstants.TickSizes.HourTickLength : AnalogClockConstants.TickSizes.MinuteTickLength;
            double tickThickness = (minute % 5 == 0) ? AnalogClockConstants.TickSizes.HourTickThickness : AnalogClockConstants.TickSizes.MinuteTickThickness;
            double startRadius = AnalogClockConstants.Positioning.ClockRadius - tickLength;
            const double endRadius = AnalogClockConstants.Positioning.ClockRadius;
            double startX = AnalogClockConstants.Positioning.ClockCenterX + (startRadius * Math.Cos(angleInRadians));
            double startY = AnalogClockConstants.Positioning.ClockCenterY + (startRadius * Math.Sin(angleInRadians));
            double endX = AnalogClockConstants.Positioning.ClockCenterX + (endRadius * Math.Cos(angleInRadians));
            double endY = AnalogClockConstants.Positioning.ClockCenterY + (endRadius * Math.Sin(angleInRadians));
            ticks.Add(new ClockTick(startX, startY, endX, endY, tickThickness));
        }
        ClockTicks = ticks;
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_disposed) return;
        if (e.PropertyName == nameof(_mainViewModel.BackgroundOpacity) || e.PropertyName == nameof(_mainViewModel.TextOpacity))
            OnPropertyChanged(e.PropertyName);
    }

    /// <summary>Обработчик события обновления времени. Обновляет углы поворота стрелок.</summary>
    private void OnTimeUpdated(object? sender, DateTime time)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                double hourAngle = ((time.Hour % 12) + (time.Minute / 60.0)) * 30;
                double minuteAngle = time.Minute * 6;
                double secondAngle = time.Second * 6;
                if (_hourHandTransform.Children.Count > 0 && _hourHandTransform.Children[0] is RotateTransform hourRotate)
                    hourRotate.Angle = hourAngle;
                else
                    HourHandTransform = new TransformGroup { Children = { new RotateTransform(hourAngle, AnalogClockConstants.Positioning.ClockCenterX, AnalogClockConstants.Positioning.ClockCenterY) } };
                if (_minuteHandTransform.Children.Count > 0 && _minuteHandTransform.Children[0] is RotateTransform minuteRotate)
                    minuteRotate.Angle = minuteAngle;
                else
                    MinuteHandTransform = new TransformGroup { Children = { new RotateTransform(minuteAngle, AnalogClockConstants.Positioning.ClockCenterX, AnalogClockConstants.Positioning.ClockCenterY) } };
                if (_secondHandTransform.Children.Count > 0 && _secondHandTransform.Children[0] is RotateTransform secondRotate)
                    secondRotate.Angle = secondAngle;
                else
                    SecondHandTransform = new TransformGroup { Children = { new RotateTransform(secondAngle, AnalogClockConstants.Positioning.ClockCenterX, AnalogClockConstants.Positioning.ClockCenterY) } };
            });
    }

    #endregion

    #region IDisposable
    /// <summary>Освобождает ресурсы, используемые экземпляром класса <see cref="AnalogClockViewModel"/>.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _timeService.TimeUpdated -= OnTimeUpdated;
        _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
        _disposed = true;

        GC.SuppressFinalize(this);
    }
    #endregion
}