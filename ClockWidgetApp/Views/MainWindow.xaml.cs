using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

/// <summary>
/// Главное окно приложения (цифровые часы).
/// </summary>
public partial class MainWindow : Window
{
    #region Private Fields
    /// <summary>ViewModel главного окна.</summary>
    private readonly MainWindowViewModel _viewModel;
    /// <summary>Логгер для событий окна.</summary>
    private readonly ILogger<MainWindow> _logger;
    /// <summary>Стартовая позиция мыши при drag&drop (в координатах экрана).</summary>
    private System.Windows.Point _dragStartMouseScreen;
    /// <summary>Стартовая позиция окна по X.</summary>
    private double _dragStartWindowLeft;
    /// <summary>Стартовая позиция окна по Y.</summary>
    private double _dragStartWindowTop;
    /// <summary>Флаг активного перетаскивания окна.</summary>
    private bool _isDragging;
    #endregion

    /// <summary>
    /// ViewModel для биндинга в XAML и других окон.
    /// </summary>
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

    #region Event Handlers & Private Methods

    /// <summary>
    /// Начало перетаскивания окна мышью.
    /// </summary>
    private void MainWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _isDragging = true;
        _dragStartMouseScreen = PointToScreen(e.GetPosition(this));
        _dragStartWindowLeft = Left;
        _dragStartWindowTop = Top;
        CaptureMouse();
        e.Handled = true;
    }

    /// <summary>
    /// Завершение перетаскивания окна мышью.
    /// </summary>
    private void MainWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _isDragging = false;
        ReleaseMouseCapture();
        _viewModel.SaveWindowPosition(Left, Top); // Сохраняем позицию окна при завершении перемещения
        e.Handled = true;
    }

    /// <summary>
    /// Перемещение окна мышью.
    /// </summary>
    private void MainWindow_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isDragging && IsMouseCaptured)
        {
            System.Windows.Point currentMouseScreen = PointToScreen(e.GetPosition(this));
            var delta = currentMouseScreen - _dragStartMouseScreen;
            Left = _dragStartWindowLeft + delta.X;
            Top = _dragStartWindowTop + delta.Y;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Открытие окна настроек по правой кнопке мыши.
    /// </summary>
    private void MainWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!_viewModel.IsSettingsWindowOpen)
        {
            _viewModel.OpenSettingsCommand.Execute(null);
        }
        e.Handled = true;
    }

    /// <summary>
    /// Обработка закрытия окна: скрытие и сохранение позиции.
    /// </summary>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogDebug("[MainWindow] Main window closing");
            e.Cancel = true;
            _viewModel.HideWindowCommand.Execute(null);
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

    #endregion
}