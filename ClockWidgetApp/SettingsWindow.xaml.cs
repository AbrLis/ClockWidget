using System.Windows;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp;

public partial class SettingsWindow : Window
{
    private readonly SettingsWindowViewModel _viewModel;

    public SettingsWindow(MainWindowViewModel mainViewModel)
    {
        InitializeComponent();
        _viewModel = new SettingsWindowViewModel(mainViewModel);
        DataContext = _viewModel;
        
        // Добавляем обработчик закрытия окна
        Closing += SettingsWindow_Closing;
    }

    private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow!).IsSettingsWindowOpen = false;
    }

    private void CloseWidgetButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow?.Close();
        Close();
    }
} 