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

        // Добавляем обработчики событий мыши для перетаскивания
        PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
        PreviewMouseMove += MainWindow_PreviewMouseMove;

        // Обновляем время сразу при запуске
        UpdateTime();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        TimeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
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
        _isDragging = false;
        ReleaseMouseCapture();
        e.Handled = true;
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
}