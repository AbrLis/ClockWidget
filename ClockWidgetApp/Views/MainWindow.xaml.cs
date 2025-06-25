using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ClockWidgetApp;

/// <summary>
/// Главное окно приложения (цифровые часы).
/// </summary>
public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
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
            _logger.LogInformation("[MainWindow] Initializing main window");
            Instance = this;
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
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
            this.Closed += (s, e) => { Instance = null; };
            _logger.LogInformation("[MainWindow] Main window initialized");
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
        // Открываем окно настроек по правой кнопке мыши
        if (!IsSettingsWindowOpen)
        {
            OpenSettingsWindow();
        }
        e.Handled = true; // Предотвращаем появление контекстного меню
    }

    public void OpenSettingsWindow()
    {
        if (App.SettingsWindowInstance == null || !App.SettingsWindowInstance.IsVisible)
        {
            var settingsVm = ((App)System.Windows.Application.Current).Services.GetRequiredService<SettingsWindowViewModel>();
            var logger = ((App)System.Windows.Application.Current).Services.GetRequiredService<ILogger<SettingsWindow>>();
            App.SettingsWindowInstance = new SettingsWindow(settingsVm, logger);
            App.SettingsWindowInstance.Closed += (s, e) => App.SettingsWindowInstance = null;
            App.SettingsWindowInstance.Show();
        }
        else
        {
            App.SettingsWindowInstance.Activate();
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