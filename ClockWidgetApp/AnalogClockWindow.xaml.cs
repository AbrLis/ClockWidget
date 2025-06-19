using System.Windows;
using ClockWidgetApp.ViewModels;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

/// <summary>
/// Окно с аналоговыми часами.
/// </summary>
public partial class AnalogClockWindow : Window
{
    private readonly AnalogClockViewModel _viewModel;
    private readonly ILogger<AnalogClockWindow> _logger = LoggingService.CreateLogger<AnalogClockWindow>();
    private System.Windows.Point _dragStartPoint;
    private bool _isDragging;

    public AnalogClockWindow()
    {
        try
        {
            _logger.LogInformation("Initializing analog clock window");
            
            InitializeComponent();
            
            // Инициализируем ViewModel
            _viewModel = new AnalogClockViewModel();
            DataContext = _viewModel;
            
            // Устанавливаем позицию окна
            var (left, top) = _viewModel.GetWindowPosition();
            Left = left;
            Top = top;
            
            // Отключаем контекстное меню
            ContextMenu = null;
            
            // Добавляем обработчики событий мыши
            PreviewMouseLeftButtonDown += AnalogClockWindow_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += AnalogClockWindow_PreviewMouseLeftButtonUp;
            PreviewMouseMove += AnalogClockWindow_PreviewMouseMove;
            MouseRightButtonDown += AnalogClockWindow_MouseRightButtonDown;

            // Добавляем обработчик закрытия окна
            Closing += AnalogClockWindow_Closing;
            
            // Добавляем обработчик загрузки окна
            Loaded += AnalogClockWindow_Loaded;
            
            _logger.LogInformation("Analog clock window initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing analog clock window");
            throw;
        }
    }

    private void AnalogClockWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Analog clock window loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in window loaded event");
        }
    }

    private void AnalogClockWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        try
        {
            _dragStartPoint = e.GetPosition(this);
            _isDragging = true;
            CaptureMouse();
            _logger.LogDebug("Mouse left button down, started dragging");
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse left button down event");
        }
    }

    private void AnalogClockWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        try
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                _logger.LogDebug("Mouse left button up, stopped dragging");
                e.Handled = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse left button up event");
        }
    }

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
                
                _logger.LogDebug("Window moved: Left={Left}, Top={Top}", Left, Top);
                e.Handled = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse move event");
        }
    }

    private void AnalogClockWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            _logger.LogInformation("Opening settings window");
            // Открываем окно настроек при правом клике
            var settingsWindow = new SettingsWindow(App.MainViewModel);
            settingsWindow.Owner = this;
            settingsWindow.Show();
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening settings window");
        }
    }

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
            _logger.LogError(ex, "Error during window closing");
        }
    }
} 