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

    /// <summary>
    /// Создаёт окно с аналоговыми часами и инициализирует все компоненты и обработчики событий.
    /// </summary>
    public AnalogClockWindow(AnalogClockViewModel viewModel, MainWindowViewModel mainViewModel, ILogger<AnalogClockWindow> logger)
    {
        try
        {
            _logger = logger;
            _mainViewModel = mainViewModel;
            _logger.LogDebug("[AnalogClockWindow] Initializing analog clock window");
            
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
            
            _logger.LogDebug("[AnalogClockWindow] Analog clock window initialized");
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
        _logger.LogDebug("[AnalogClockWindow] Analog clock window loaded");
    }

    /// <summary>
    /// Начало перемещения окна мышью (запускает drag в ViewModel).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _viewModel.StartDrag(e.GetPosition(this));
        CaptureMouse();
        _logger.LogDebug("[AnalogClockWindow] Mouse left button down, started dragging");
        e.Handled = true;
    }

    /// <summary>
    /// Завершение перемещения окна мышью (завершает drag в ViewModel).
    /// </summary>
    private void AnalogClockWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _viewModel.StopDrag();
        ReleaseMouseCapture();
        _logger.LogDebug("[AnalogClockWindow] Mouse left button up, stopped dragging");
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
            _logger.LogDebug("[AnalogClockWindow] Window moved: Left={Left}, Top={Top}", Left, Top);
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
        _logger.LogDebug("[AnalogClockWindow] Opening settings window");
        e.Handled = true;
    }

    /// <summary>
    /// Обработчик закрытия окна: сохраняет позицию, отписывается от событий и освобождает ресурсы.
    /// </summary>
    private void AnalogClockWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _logger.LogDebug("Analog clock window closing");
        _viewModel.SaveWindowPosition(Left, Top);
        PreviewMouseLeftButtonDown -= AnalogClockWindow_PreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp -= AnalogClockWindow_PreviewMouseLeftButtonUp;
        PreviewMouseMove -= AnalogClockWindow_PreviewMouseMove;
        MouseRightButtonDown -= AnalogClockWindow_MouseRightButtonDown;
        Closing -= AnalogClockWindow_Closing;
        Loaded -= AnalogClockWindow_Loaded;
        if (_viewModel is IDisposable disposable)
            disposable.Dispose();
        DataContext = null;
        _logger.LogDebug("Analog clock window closed");
    }
} 