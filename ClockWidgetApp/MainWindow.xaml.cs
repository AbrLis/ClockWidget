using System.Windows;
using ClockWidgetApp.ViewModels;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

/// <summary>
/// Главное окно приложения (цифровые часы).
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ILogger<MainWindow> _logger = LoggingService.CreateLogger<MainWindow>();
    private System.Windows.Point _dragStartPoint;
    private bool _isDragging;
    private SettingsWindow? _settingsWindow;
    public bool IsSettingsWindowOpen { get; set; }
    public MainWindowViewModel ViewModel => _viewModel;

    /// <summary>
    /// Создаёт главное окно приложения.
    /// </summary>
    public MainWindow()
    {
        try
        {
            _logger.LogInformation("[MainWindow] Initializing main window");
            
            InitializeComponent();
            
            // Инициализируем ViewModel
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            
            // Устанавливаем позицию окна
            var (left, top) = _viewModel.GetWindowPosition();
            Left = left;
            Top = top;
            
            // Применяем свойство Topmost из ViewModel
            Topmost = _viewModel.DigitalClockTopmost;
            
            // Отключаем контекстное меню
            ContextMenu = null;
            
            // Добавляем обработчики событий мыши
            PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
            PreviewMouseMove += MainWindow_PreviewMouseMove;
            MouseRightButtonDown += MainWindow_MouseRightButtonDown;
            Closing += MainWindow_Closing;
            
            _logger.LogInformation("[MainWindow] Main window initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindow] Error initializing main window");
            throw;
        }
    }

    private void MainWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
        _isDragging = true;
        CaptureMouse();
        e.Handled = true;
    }

    private void MainWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void MainWindow_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isDragging && IsMouseCaptured)
        {
            System.Windows.Point currentPosition = e.GetPosition(this);
            Vector diff = currentPosition - _dragStartPoint;
            Left += diff.X;
            Top += diff.Y;
            e.Handled = true;
        }
    }

    private void MainWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Открываем окно настроек по правой кнопке мыши
        if (!IsSettingsWindowOpen)
        {
            OpenSettingsWindow();
        }
        e.Handled = true; // Предотвращаем появление контекстного меню
    }

    public void OpenSettingsWindow()
    {
        if (_settingsWindow == null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new SettingsWindow(_viewModel);
            IsSettingsWindowOpen = true;
            _settingsWindow.Show();
        }
        else
        {
            _settingsWindow.Activate();
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogInformation("[MainWindow] Main window closing");
            
            // Отписываемся от событий
            PreviewMouseLeftButtonDown -= MainWindow_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp -= MainWindow_PreviewMouseLeftButtonUp;
            PreviewMouseMove -= MainWindow_PreviewMouseMove;
            MouseRightButtonDown -= MainWindow_MouseRightButtonDown;
            Closing -= MainWindow_Closing;
            
            // Сохраняем текущую позицию окна
            _viewModel.SaveWindowPosition(Left, Top);
            
            // Закрываем окно настроек, если оно открыто
            if (_settingsWindow != null && _settingsWindow.IsVisible)
            {
                _settingsWindow.Close();
            }
            
            // Очищаем DataContext и освобождаем ресурсы ViewModel
            if (_viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
            DataContext = null;
            
            _logger.LogInformation("[MainWindow] Main window closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindow] Error during window closing");
        }
    }
}