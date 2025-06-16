using System.Windows;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp;

public partial class SettingsWindow : Window
{
    private readonly ISettingsViewModel _viewModel;

    public SettingsWindow(ISettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
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