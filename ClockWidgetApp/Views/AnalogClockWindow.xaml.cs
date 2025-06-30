using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

namespace ClockWidgetApp;

/// <summary>
/// Окно с аналоговыми часами.
/// </summary>
public partial class AnalogClockWindow : Window
{
    // ViewModel для аналоговых часов
    private readonly AnalogClockViewModel _viewModel;
    // Логгер для событий окна
    private readonly ILogger<AnalogClockWindow> _logger;
    private readonly MainWindowViewModel _mainViewModel;
    // Переменные для логики перемещения окна мышью
    private System.Windows.Point _dragStartPoint;
    private bool _isDragging;

    /// <summary>
    /// Создаёт окно с аналоговыми часами и инициализирует все компоненты и обработчики событий.
    /// </summary>
    public AnalogClockWindow(AnalogClockViewModel viewModel, MainWindowViewModel mainViewModel, ILogger<AnalogClockWindow> logger)
    {
        try
        {
            _logger = logger;
            _mainViewModel = mainViewModel;
            _logger.LogInformation("[AnalogClockWindow] Initializing analog clock window");
            
            InitializeComponent();
            
            // Инициализируем ViewModel и связываем с DataContext
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            // Устанавливаем позицию окна из ViewModel
            var (left, top) = _viewModel.GetWindowPosition();
            Left = left;
            Top = top;
            
            // Отключаем контекстное меню
            ContextMenu = null;
            
            // Добавляем обработчики событий мыши для перемещения окна
            PreviewMouseLeftButtonDown += AnalogClockWindow_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += AnalogClockWindow_PreviewMouseLeftButtonUp;
            PreviewMouseMove += AnalogClockWindow_PreviewMouseMove;
            MouseRightButtonDown += AnalogClockWindow_MouseRightButtonDown;

            // Добавляем обработчик закрытия окна
            Closing += AnalogClockWindow_Closing;
            
            // Добавляем обработчик загрузки окна
            Loaded += AnalogClockWindow_Loaded;
            
            ClockWidgetApp.Helpers.LocalizationManager.LanguageChanged += (s, e) =>
            {
                DataContext = null;
                DataContext = _viewModel;
            };
            
            _logger.LogInformation("[AnalogClockWindow] Analog clock window initialized");
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[AnalogClockWindow] Error initializing analog clock window");
            throw;
        }
    }

    /// <summary>
    /// Обработчик события загрузки окна.
    /// </summary>
    private void AnalogClockWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("[AnalogClockWindow] Analog clock window loaded");
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[AnalogClockWindow] Error in window loaded event");
        }
    }

    /// <summary>
    /// Начало перемещения окна мышью (запоминаем стартовую точку).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        try
        {
            _dragStartPoint = e.GetPosition(this);
            _isDragging = true;
            CaptureMouse();
            _logger.LogDebug("[AnalogClockWindow] Mouse left button down, started dragging");
            e.Handled = true;
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[AnalogClockWindow] Error in mouse left button down event");
        }
    }

    /// <summary>
    /// Завершение перемещения окна мышью.
    /// </summary>
    private void AnalogClockWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        try
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                _logger.LogDebug("[AnalogClockWindow] Mouse left button up, stopped dragging");
                e.Handled = true;
            }
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[AnalogClockWindow] Error in mouse left button up event");
        }
    }

    /// <summary>
    /// Перемещение окна мышью (реализация drag & drop).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        try
        {
            if (_isDragging)
            {
                var currentPosition = e.GetPosition(this);
                var delta = currentPosition - _dragStartPoint;
                
                Left += delta.X;
                Top += delta.Y;
                
                _logger.LogDebug("[AnalogClockWindow] Window moved: Left={Left}, Top={Top}", Left, Top);
                e.Handled = true;
            }
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[AnalogClockWindow] Error in mouse move event");
        }
    }

    /// <summary>
    /// Открытие окна настроек по правому клику мыши.
    /// </summary>
    private void AnalogClockWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            _logger.LogInformation("[AnalogClockWindow] Opening settings window");
            var windowService = ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) as IWindowService;
            windowService?.OpenSettingsWindow();
            e.Handled = true;
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[AnalogClockWindow] Error opening settings window");
        }
    }

    /// <summary>
    /// Обработчик закрытия окна: сохраняет позицию, отписывается от событий и освобождает ресурсы.
    /// </summary>
    private void AnalogClockWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogInformation("Analog clock window closing");
            
            // Сохраняем позицию окна при закрытии
            _viewModel.SaveWindowPosition(Left, Top);
            
            // Отписываемся от событий
            PreviewMouseLeftButtonDown -= AnalogClockWindow_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp -= AnalogClockWindow_PreviewMouseLeftButtonUp;
            PreviewMouseMove -= AnalogClockWindow_PreviewMouseMove;
            MouseRightButtonDown -= AnalogClockWindow_MouseRightButtonDown;
            Closing -= AnalogClockWindow_Closing;
            Loaded -= AnalogClockWindow_Loaded;
            
            // Очищаем DataContext и освобождаем ресурсы ViewModel
            if (_viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
            DataContext = null;
            
            _logger.LogInformation("Analog clock window closed");
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "Error during window closing");
        }
    }
} 