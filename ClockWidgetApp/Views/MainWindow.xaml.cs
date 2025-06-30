using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

namespace ClockWidgetApp;

/// <summary>
/// Главное окно приложения (цифровые часы).
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ILogger<MainWindow> _logger;
    private System.Windows.Point _dragStartPoint;
    private bool _isDragging;
    public bool IsSettingsWindowOpen { get; set; }
    public MainWindowViewModel ViewModel => _viewModel;

    /// <summary>
    /// Создаёт главное окно приложения.
    /// </summary>
    public MainWindow(MainWindowViewModel viewModel, ILogger<MainWindow> logger)
    {
        try
        {
            _logger = logger;
            _logger.LogDebug("[MainWindow] Initializing main window");
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            ClockWidgetApp.Helpers.LocalizationManager.LanguageChanged += (s, e) =>
            {
                // Принудительно обновляем DataContext, чтобы обновить все привязки
                DataContext = null;
                DataContext = _viewModel;
            };
            var (left, top) = _viewModel.GetWindowPosition();
            Left = left;
            Top = top;
            Topmost = _viewModel.DigitalClockTopmost;
            ContextMenu = null;
            PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
            PreviewMouseMove += MainWindow_PreviewMouseMove;
            MouseRightButtonDown += MainWindow_MouseRightButtonDown;
            Closing += MainWindow_Closing;
            _logger.LogDebug("[MainWindow] Main window initialized");
        }
        catch (Exception ex)
        {
            if (_logger != null)
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
        // Открываем окно настроек по правой кнопке мыши через сервис
        if (!IsSettingsWindowOpen)
        {
            var windowService = ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) as IWindowService;
            windowService?.OpenSettingsWindow();
        }
        e.Handled = true; // Предотвращаем появление контекстного меню
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogDebug("[MainWindow] Main window closing");
            // Вместо закрытия — скрываем окно, чтобы оно могло быть показано повторно
            e.Cancel = true;
            this.Hide();
            // Сохраняем текущую позицию окна
            _viewModel.SaveWindowPosition(Left, Top);
            // Очищаем DataContext и освобождаем ресурсы ViewModel
            if (_viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
            DataContext = null;
            _logger.LogDebug("[MainWindow] Main window hidden (not closed)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindow] Error during window closing");
        }
    }
}