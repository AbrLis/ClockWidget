using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для главного окна виджета часов.
/// Управляет отображением времени, прозрачностью и другими настройками виджета.
/// </summary>
public partial class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    #region Private Fields
    /// <summary>Сервис времени.</summary>
    private readonly ITimeService _timeService;
    /// <summary>Сервис настроек.</summary>
    private readonly IAppDataService _appDataService;
    /// <summary>Сервис звука.</summary>
    private readonly ISoundService _soundService;
    /// <summary>Сервис управления окнами.</summary>
    private readonly IWindowService _windowService;
    /// <summary>Логгер для событий ViewModel.</summary>
    private readonly ILogger<MainWindowViewModel> _logger;
    /// <summary>Флаг освобождения ресурсов.</summary>
    private bool _disposed;
    /// <summary>Флаг открытия окна настроек.</summary>
    private bool _isSettingsWindowOpen;
    private bool _isDragging;
    private System.Windows.Point _dragStartPoint;
    #endregion

    /// <summary>
    /// Событие, возникающее при изменении значения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Показывает, открыто ли окно настроек.
    /// </summary>
    public bool IsSettingsWindowOpen
    {
        get => _isSettingsWindowOpen;
        set { _isSettingsWindowOpen = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Команда для открытия окна настроек.
    /// </summary>
    public RelayCommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsWindow());

    /// <summary>
    /// Команда для скрытия главного окна.
    /// </summary>
    public RelayCommand HideWindowCommand => new RelayCommand(_ => HideWindow());

    /// <summary>
    /// Метод для начала перетаскивания окна. Сохраняет исходную точку нажатия мыши.
    /// </summary>
    public void StartDrag(System.Windows.Point startPoint)
    {
        _dragStartPoint = startPoint;
        _isDragging = true;
    }

    /// <summary>
    /// Метод для завершения перетаскивания окна. Сбрасывает флаг перетаскивания.
    /// </summary>
    public void EndDrag()
    {
        _isDragging = false;
    }

    /// <summary>
    /// Возвращает смещение мыши относительно исходной точки нажатия (dragStartPoint), не обновляя dragStartPoint.
    /// </summary>
    public (double deltaX, double deltaY)? MoveDrag(System.Windows.Point currentPosition)
    {
        if (_isDragging)
        {
            var diff = currentPosition - _dragStartPoint;
            return (diff.X, diff.Y);
        }
        return null;
    }

    /// <summary>
    /// Сохраняет позицию окна и уведомляет об изменении настроек.
    /// </summary>
    public void SaveWindowPosition(double left, double top)
    {
        WindowPositionHelper.SaveWindowPosition(_appDataService.Data.WidgetSettings, left, top, false);
        _appDataService.NotifySettingsChanged();
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindowViewModel"/>.
    /// Загружает сохранённые настройки и запускает сервис обновления времени.
    /// </summary>
    public MainWindowViewModel(ITimeService timeService, IAppDataService appDataService, ISoundService soundService, IWindowService windowService, ILogger<MainWindowViewModel> logger)
    {
        System.Diagnostics.Debug.Assert(logger != null, "_logger is null");
        System.Diagnostics.Debug.Assert(timeService != null, "_timeService is null");
        System.Diagnostics.Debug.Assert(appDataService != null, "_appDataService is null");
        System.Diagnostics.Debug.Assert(soundService != null, "_soundService is null");
        System.Diagnostics.Debug.Assert(windowService != null, "_windowService is null");
        _logger = logger;
        _timeService = timeService;
        _appDataService = appDataService;
        _soundService = soundService;
        _windowService = windowService;
        try
        {
            _logger.LogDebug("[MainWindowViewModel] Initializing main window view model");
            var settings = _appDataService.Data.WidgetSettings;
            InitializeFromSettings(settings);
            SubscribeToLanguageChanges();
            _timeService.TimeUpdated += OnTimeUpdated;
            OnTimeUpdated(this, DateTime.Now);
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateWindowsVisibility();
                    _logger.LogDebug("[MainWindowViewModel] Windows visibility updated after initialization");
                }));
            }
            _logger.LogDebug("[MainWindowViewModel] Main window view model initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel] Error initializing main window view model");
            throw;
        }
    }

    #region Public Methods

    /// <summary>
    /// Вызывает событие <see cref="PropertyChanged"/>.
    /// </summary>
    /// <param name="propertyName">Имя изменившегося свойства.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Освобождает ресурсы, используемые экземпляром класса <see cref="MainWindowViewModel"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            _logger.LogDebug("[MainWindowViewModel] Disposing main window view model");
            _timeService.TimeUpdated -= OnTimeUpdated;
            _disposed = true;
            _logger.LogDebug("[MainWindowViewModel] Main window view model disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel] Error disposing main window view model");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Открывает окно настроек через сервис.
    /// </summary>
    private void OpenSettingsWindow()
    {
        _logger.LogDebug("[MainWindowViewModel] OpenSettingsWindow called");
        _windowService.OpenSettingsWindow();
        IsSettingsWindowOpen = true;
    }

    /// <summary>
    /// Скрывает главное окно через сервис и сохраняет позицию.
    /// </summary>
    private void HideWindow()
    {
        _logger.LogDebug("[MainWindowViewModel] HideWindow called");
        SaveWindowPosition(_windowService.GetMainWindowLeft(), _windowService.GetMainWindowTop());
        _windowService.HideMainWindow();
    }

    #endregion
}