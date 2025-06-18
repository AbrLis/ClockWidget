using System;
using System.Configuration;
using System.Data;
using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static SettingsService? _settingsService;
    private static MainWindowViewModel? _mainViewModel;
    private static TimeService? _timeService;
    private MainWindow? _mainWindow;
    private ILogger<App>? _logger;

    /// <summary>
    /// Получает сервис настроек приложения.
    /// </summary>
    public static SettingsService SettingsService
    {
        get => _settingsService ?? throw new InvalidOperationException("SettingsService is not initialized");
        private set => _settingsService = value;
    }

    /// <summary>
    /// Получает ViewModel главного окна.
    /// </summary>
    public static MainWindowViewModel MainViewModel
    {
        get => _mainViewModel ?? throw new InvalidOperationException("MainViewModel is not initialized");
        private set => _mainViewModel = value;
    }

    /// <summary>
    /// Получает общий сервис времени приложения.
    /// </summary>
    public static TimeService TimeService
    {
        get => _timeService ?? throw new InvalidOperationException("TimeService is not initialized");
        private set => _timeService = value;
    }

    public App()
    {
        // Инициализируем логирование
        LoggingService.Initialize();
        _logger = LoggingService.CreateLogger<App>();

        // Глобальный обработчик необработанных исключений
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.ExceptionObject as Exception, "Unhandled exception (AppDomain)");
        };
        DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "Unhandled exception (Dispatcher)");
            args.Handled = true;
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            _logger?.LogInformation("Starting application");
            
            // Инициализируем сервисы
            SettingsService = new SettingsService();
            TimeService = new TimeService();
            _logger?.LogInformation("Services initialized");
            
            // Загружаем настройки
            var settings = SettingsService.CurrentSettings;
            _logger?.LogInformation("Settings loaded: {Settings}", 
                System.Text.Json.JsonSerializer.Serialize(settings));

            // Создаем и показываем основное окно
            _mainWindow = new MainWindow();
            Application.Current.MainWindow = _mainWindow;
            MainViewModel = _mainWindow.ViewModel;
            
            // Запускаем общий сервис времени
            TimeService.Start();
            _logger?.LogInformation("Time service started");
            
            // Явно показываем и активируем окно
            _mainWindow.Show();
            _mainWindow.Activate();
            _logger?.LogInformation("Main window created, shown and activated");

            // Устанавливаем режим завершения приложения
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            
            _logger?.LogInformation("Application started successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during application startup");
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger?.LogInformation("Application shutting down");
            
            // Останавливаем и освобождаем TimeService
            if (_timeService != null)
            {
                _timeService.Stop();
                _timeService.Dispose();
                _logger?.LogInformation("Time service disposed");
            }
            
            base.OnExit(e);
            _logger?.LogInformation("Application shutdown completed");
            
            // Освобождаем ресурсы логгера
            LoggingService.Dispose();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during application shutdown");
            throw;
        }
    }
}

