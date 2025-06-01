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
    private bool _showSeconds = true;
    public bool IsSettingsWindowOpen { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        
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
    }

    public void SetBackgroundOpacity(double opacity)
    {
        // Устанавливаем прозрачность только для фона
        BackgroundBorder.Opacity = opacity;
    }

    public void SetTextOpacity(double opacity)
    {
        // Устанавливаем прозрачность только для текста
        TimeTextBlock.Opacity = opacity;
    }

    public void SetFontSize(double size)
    {
        // Устанавливаем размер шрифта
        TimeTextBlock.FontSize = size;
    }

    public void SetShowSeconds(bool show)
    {
        _showSeconds = show;
        UpdateTime();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        TimeTextBlock.Text = DateTime.Now.ToString(_showSeconds ? "HH:mm:ss" : "HH:mm");
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
}