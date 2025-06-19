using System.Windows;
using ClockWidgetApp.ViewModels;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

public partial class SettingsWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ILogger<SettingsWindow> _logger = LoggingService.CreateLogger<SettingsWindow>();

    public SettingsWindow(MainWindowViewModel viewModel)
    {
        try
        {
            _logger.LogInformation("[SettingsWindow] Initializing settings window");
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            // Добавляем обработчик закрытия окна
            Closing += SettingsWindow_Closing;
            _logger.LogInformation("[SettingsWindow] Settings window initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error initializing settings window");
            throw;
        }
    }

    private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogInformation("[SettingsWindow] Settings window closing");
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.IsSettingsWindowOpen = false;
            }
            _logger.LogInformation("[SettingsWindow] Settings window closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error during settings window closing");
        }
    }

    private void CloseWidgetButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("[SettingsWindow] Close widget button clicked");
            
            // Закрываем все окна приложения
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                _logger.LogDebug("[SettingsWindow] Closing window: {WindowType}", window.GetType().Name);
                window.Close();
            }
            
            // Завершаем работу приложения
            _logger.LogInformation("[SettingsWindow] Shutting down application");
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error during application shutdown");
            // Даже в случае ошибки пытаемся завершить работу
            System.Windows.Application.Current.Shutdown();
        }
    }
} 