using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClockWidgetApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private Point _dragStartPoint;
    private bool _isDragging;
    private SettingsWindow? _settingsWindow;
    internal WidgetSettings _settings;
    public bool IsSettingsWindowOpen { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        
        // Загружаем настройки
        _settings = WidgetSettings.Load();
        
        // Устанавливаем позицию окна
        if (_settings.WindowLeft.HasValue && _settings.WindowTop.HasValue)
        {
            Left = _settings.WindowLeft.Value;
            Top = _settings.WindowTop.Value;
        }
        else
        {
            // Если позиция не сохранена, используем значения по умолчанию
            Left = Constants.DEFAULT_WINDOW_LEFT;
            Top = Constants.DEFAULT_WINDOW_TOP;
        }
        
        ApplySettings();
        
        // Отключаем контекстное меню
        ContextMenu = null;
        
        // Настройка таймера для обновления времени
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        // Добавляем обработчики событий мыши
        PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
        PreviewMouseMove += MainWindow_PreviewMouseMove;
        MouseRightButtonDown += MainWindow_MouseRightButtonDown;

        // Обновляем время сразу при запуске
        UpdateTime();

        // Добавляем обработчик закрытия окна
        Closing += MainWindow_Closing;
    }

    private void ApplySettings()
    {
        // Применяем настройки с валидацией
        SetBackgroundOpacity(ValidateOpacity(_settings.BackgroundOpacity, 
            Constants.MIN_WINDOW_OPACITY, 
            Constants.MAX_WINDOW_OPACITY, 
            Constants.DEFAULT_WINDOW_OPACITY));
        
        SetTextOpacity(ValidateOpacity(_settings.TextOpacity, 
            Constants.MIN_TEXT_OPACITY, 
            Constants.MAX_TEXT_OPACITY, 
            Constants.DEFAULT_TEXT_OPACITY));
        
        SetFontSize(ValidateFontSize(_settings.FontSize));
        SetShowSeconds(_settings.ShowSeconds);
    }

    private double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        // Округляем значение до ближайшего шага
        return Math.Round(value / Constants.OPACITY_STEP) * Constants.OPACITY_STEP;
    }

    private double ValidateFontSize(double value)
    {
        if (value < Constants.MIN_FONT_SIZE || value > Constants.MAX_FONT_SIZE)
        {
            return Constants.DEFAULT_FONT_SIZE;
        }
        // Округляем значение до ближайшего шага
        return Math.Round(value / Constants.FONT_SIZE_STEP) * Constants.FONT_SIZE_STEP;
    }

    public void SetBackgroundOpacity(double opacity)
    {
        // Устанавливаем прозрачность только для фона
        BackgroundBorder.Opacity = ValidateOpacity(opacity, 
            Constants.MIN_WINDOW_OPACITY, 
            Constants.MAX_WINDOW_OPACITY, 
            Constants.DEFAULT_WINDOW_OPACITY);
        _settings.BackgroundOpacity = BackgroundBorder.Opacity;
        _settings.Save();
    }

    public void SetTextOpacity(double opacity)
    {
        // Устанавливаем прозрачность только для текста
        TimeTextBlock.Opacity = ValidateOpacity(opacity, 
            Constants.MIN_TEXT_OPACITY, 
            Constants.MAX_TEXT_OPACITY, 
            Constants.DEFAULT_TEXT_OPACITY);
        _settings.TextOpacity = TimeTextBlock.Opacity;
        _settings.Save();
    }

    public void SetFontSize(double size)
    {
        // Устанавливаем размер шрифта
        TimeTextBlock.FontSize = ValidateFontSize(size);
        _settings.FontSize = TimeTextBlock.FontSize;
        _settings.Save();
    }

    public void SetShowSeconds(bool show)
    {
        _settings.ShowSeconds = show;
        UpdateTime();
        _settings.Save();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        TimeTextBlock.Text = DateTime.Now.ToString(_settings.ShowSeconds ? 
            Constants.TIME_FORMAT_WITH_SECONDS : 
            Constants.TIME_FORMAT_WITHOUT_SECONDS);
    }

    private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(this);
        _isDragging = true;
        CaptureMouse();
        e.Handled = true;
    }

    private void MainWindow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && IsMouseCaptured)
        {
            Point currentPosition = e.GetPosition(this);
            Vector diff = currentPosition - _dragStartPoint;
            Left += diff.X;
            Top += diff.Y;
            e.Handled = true;
        }
    }

    private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Открываем окно настроек по правой кнопке мыши
        if (!IsSettingsWindowOpen)
        {
            OpenSettingsWindow();
        }
        e.Handled = true; // Предотвращаем появление контекстного меню
    }

    private void OpenSettingsWindow()
    {
        if (_settingsWindow == null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new SettingsWindow(this);
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
        // Останавливаем таймер
        _timer.Stop();
        
        // Сохраняем текущую позицию окна
        _settings.WindowLeft = Left;
        _settings.WindowTop = Top;
        
        // Сохраняем текущие настройки
        _settings.Save();
        
        // Закрываем окно настроек, если оно открыто
        if (_settingsWindow != null && _settingsWindow.IsVisible)
        {
            _settingsWindow.Close();
        }
    }
}