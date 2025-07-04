using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp;

/// <summary>
/// Окно настроек приложения.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsWindowViewModel _viewModel = null!;
    private readonly ILogger<SettingsWindow> _logger;

    /// <summary>
    /// Создаёт новое окно настроек.
    /// </summary>
    /// <param name="viewModel">ViewModel окна настроек.</param>
    /// <param name="logger">ILogger<SettingsWindow> для логирования.</param>
    public SettingsWindow(SettingsWindowViewModel viewModel, ILogger<SettingsWindow> logger)
        : base()
    {
        _viewModel = viewModel;
        _logger = logger;
        try
        {
            _logger.LogDebug("[SettingsWindow] Initializing settings window");
            InitializeComponent();
            DataContext = _viewModel;
            _logger.LogDebug($"[SettingsWindow] DataContext type: {DataContext?.GetType().FullName}");
            LocalizationManager.LanguageChanged += (s, e) =>
            {
                DataContext = null;
                DataContext = _viewModel;
            };
            // Добавляем обработчик закрытия окна
            Closing += SettingsWindow_Closing;
            _logger.LogDebug("[SettingsWindow] Settings window initialized");
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.LogError(ex, "[SettingsWindow] Error initializing settings window");
            throw;
        }
    }

    private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogDebug("[SettingsWindow] Settings window closing");
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.IsSettingsWindowOpen = false;
            }
            // Вместо уничтожения окна скрываем его, чтобы оно оставалось в памяти
            e.Cancel = true;
            this.Hide();
            _logger.LogDebug("[SettingsWindow] Settings window hidden (not closed)");
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
            _logger.LogDebug("[SettingsWindow] Close widget button clicked");
            
            // Закрываем все окна приложения
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                _logger.LogDebug("[SettingsWindow] Closing window: {WindowType}", window.GetType().Name);
                window.Close();
            }
            
            // Завершаем работу приложения
            _logger.LogDebug("[SettingsWindow] Shutting down application");
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error during application shutdown");
            // Даже в случае ошибки пытаемся завершить работу
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void ShowLogsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string logsDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget",
                "logs");
            if (!System.IO.Directory.Exists(logsDir))
            {
                System.Windows.MessageBox.Show(_viewModel.Localized.SettingsWindow_LogsNotFound, _viewModel.Localized.SettingsWindow_Logs, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var logFiles = System.IO.Directory.GetFiles(logsDir, "clock-widget-*.log");
            if (logFiles.Length == 0)
            {
                System.Windows.MessageBox.Show(_viewModel.Localized.SettingsWindow_LogsNotFound, _viewModel.Localized.SettingsWindow_Logs, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var lastLog = logFiles
                .Select(f => new System.IO.FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .First().FullName;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = lastLog,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Ошибка при открытии файла логов");
            System.Windows.MessageBox.Show($"Ошибка при открытии файла логов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
} 