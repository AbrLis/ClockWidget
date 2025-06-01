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
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private Point _dragStartPoint;
    private bool _isDragging;
    private SettingsWindow? _settingsWindow;
    public bool IsSettingsWindowOpen { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        
        // Инициализируем ViewModel
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        // Устанавливаем позицию окна
        var (left, top) = _viewModel.GetWindowPosition();
        Left = left;
        Top = top;
        
        // Отключаем контекстное меню
        ContextMenu = null;
        
        // Добавляем обработчики событий мыши
        PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
        PreviewMouseMove += MainWindow_PreviewMouseMove;
        MouseRightButtonDown += MainWindow_MouseRightButtonDown;

        // Добавляем обработчик закрытия окна
        Closing += MainWindow_Closing;
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
        // Сохраняем текущую позицию окна
        _viewModel.SaveWindowPosition(Left, Top);
        
        // Закрываем окно настроек, если оно открыто
        if (_settingsWindow != null && _settingsWindow.IsVisible)
        {
            _settingsWindow.Close();
        }
    }
}