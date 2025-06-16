using System;
using System.Configuration;
using System.Data;
using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private MainWindow? _mainWindow;
    private AnalogClockWindow? _analogClockWindow;
    private readonly ILogger<App> _logger;

    public App()
    {
        // Инициализируем логирование
        LoggingService.Initialize(logToFile: true);
        _logger = LoggingService.CreateLogger<App>();

        // Глобальный обработчик необработанных исключений
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger.LogError(args.ExceptionObject as Exception, "Unhandled exception (AppDomain)");
        };
        DispatcherUnhandledException += (sender, args) =>
        {
            _logger.LogError(args.Exception, "Unhandled exception (Dispatcher)");
            args.Handled = true;
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            _logger.LogInformation("Starting application");
            base.OnStartup(e);

            // Проверяем, не создано ли уже окно с цифровыми часами
            if (Application.Current.MainWindow == null)
            {
                // Создаем и показываем основное окно
                _mainWindow = new MainWindow();
                Application.Current.MainWindow = _mainWindow;
                _mainWindow.Show();
                _logger.LogInformation("Main window created and shown");
            }
            else
            {
                _logger.LogWarning("Main window already exists, skipping creation");
            }

            // Создаем и показываем окно с аналоговыми часами
            _analogClockWindow = new AnalogClockWindow();
            _analogClockWindow.Show();
            _logger.LogInformation("Analog clock window created and shown");

            // Устанавливаем ShutdownMode в OnLastWindowClose
            // чтобы приложение закрывалось только когда все окна закрыты
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            _logger.LogInformation("Application startup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application startup");
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger.LogInformation("Application shutting down");
            base.OnExit(e);
            _logger.LogInformation("Application shutdown completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application shutdown");
            throw;
        }
    }
}

