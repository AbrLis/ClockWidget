using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

/// <summary>
/// Окно с аналоговыми часами.
/// </summary>
public partial class AnalogClockWindow : Window
{
    /// <summary>ViewModel для аналоговых часов.</summary>
    private readonly AnalogClockViewModel _viewModel;
    /// <summary>Логгер для событий окна.</summary>
    private readonly ILogger<AnalogClockWindow> _logger;
    /// <summary>Главный ViewModel приложения.</summary>
    private readonly MainWindowViewModel _mainViewModel;

    /// <summary>
    /// Создаёт окно с аналоговыми часами и инициализирует все компоненты и обработчики событий.
    /// </summary>
    public AnalogClockWindow(AnalogClockViewModel viewModel, MainWindowViewModel mainViewModel, ILogger<AnalogClockWindow> logger)
    {
        _logger = logger;
        _mainViewModel = mainViewModel;
        _logger.LogDebug("[AnalogClockWindow] Initializing analog clock window");
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Устанавливаем позицию окна из ViewModel
        var (left, top) = _viewModel.GetWindowPosition();
        Left = left;
        Top = top;

        ContextMenu = null;

        // Подписка на события мыши и закрытия окна
        PreviewMouseLeftButtonDown += AnalogClockWindow_PreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += AnalogClockWindow_PreviewMouseLeftButtonUp;
        PreviewMouseMove += AnalogClockWindow_PreviewMouseMove;
        MouseRightButtonDown += AnalogClockWindow_MouseRightButtonDown;
        Closing += AnalogClockWindow_Closing;

        // Перепривязка DataContext при смене языка
        ClockWidgetApp.Helpers.LocalizationManager.LanguageChanged += (s, e) =>
        {
            DataContext = null;
            DataContext = _viewModel;
        };

        _logger.LogDebug("[AnalogClockWindow] Analog clock window initialized");
    }

    #region Event Handlers

    /// <summary>
    /// Начало перемещения окна мышью (запускает drag в ViewModel).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _viewModel.StartDrag(e.GetPosition(this));
        CaptureMouse();
        e.Handled = true;
    }

    /// <summary>
    /// Завершение перемещения окна мышью (завершает drag в ViewModel).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _viewModel.StopDrag();
        ReleaseMouseCapture();
        _viewModel.SaveWindowPosition(Left, Top); // Сохраняем позицию окна при завершении перемещения
        e.Handled = true;
    }

    /// <summary>
    /// Перемещение окна мышью (вызывает DragMove в ViewModel и применяет delta к Left/Top).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var delta = _viewModel.DragMove(e.GetPosition(this));
        if (delta.HasValue)
        {
            Left += delta.Value.deltaX;
            Top += delta.Value.deltaY;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Открытие окна настроек по правому клику мыши (через команду ViewModel).
    /// </summary>
    private void AnalogClockWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_viewModel.OpenSettingsCommand.CanExecute(null))
            _viewModel.OpenSettingsCommand.Execute(null);
        e.Handled = true;
    }

    /// <summary>
    /// Обработчик закрытия окна: сохраняет позицию, отписывается от событий и освобождает ресурсы.
    /// </summary>
    private void AnalogClockWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.SaveWindowPosition(Left, Top);
        PreviewMouseLeftButtonDown -= AnalogClockWindow_PreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp -= AnalogClockWindow_PreviewMouseLeftButtonUp;
        PreviewMouseMove -= AnalogClockWindow_PreviewMouseMove;
        MouseRightButtonDown -= AnalogClockWindow_MouseRightButtonDown;
        Closing -= AnalogClockWindow_Closing;
        if (_viewModel is IDisposable disposable)
            disposable.Dispose();
        DataContext = null;
    }

    #endregion
} 