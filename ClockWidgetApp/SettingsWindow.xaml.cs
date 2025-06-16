using System;
using System.Windows;
using ClockWidgetApp.ViewModels;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

public partial class SettingsWindow : Window
{
    private readonly ISettingsViewModel _viewModel;
    private readonly ILogger<SettingsWindow> _logger = LoggingService.CreateLogger<SettingsWindow>();

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
        try
        {
            _logger.LogInformation("Settings window closing");
            ((MainWindow)Application.Current.MainWindow!).IsSettingsWindowOpen = false;
            _logger.LogInformation("Settings window closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during settings window closing");
        }
    }

    private void CloseWidgetButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Close widget button clicked");
            
            // Закрываем все окна приложения
            foreach (Window window in Application.Current.Windows)
            {
                _logger.LogDebug("Closing window: {WindowType}", window.GetType().Name);
                window.Close();
            }
            
            // Завершаем работу приложения
            _logger.LogInformation("Shutting down application");
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application shutdown");
            // Даже в случае ошибки пытаемся завершить работу
            Application.Current.Shutdown();
        }
    }
} 