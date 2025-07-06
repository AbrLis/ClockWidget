using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp;

/// <summary>
/// Главный класс WPF-приложения. Точка входа и управление жизненным циклом.
/// </summary>
public partial class App : System.Windows.Application
{
    #region Private Fields

    /// <summary>DI-контейнер приложения. Инициализируется в OnStartup.</summary>
    private ServiceProvider _serviceProvider = default!;
    /// <summary>Логгер приложения.</summary>
    private ILogger<App>? _logger;
    /// <summary>Менеджер иконок трея (через DI).</summary>
    private TrayIconManager? _trayIconManager;
    /// <summary>Временный логгер для single instance событий (до инициализации DI/основного логгера).</summary>
    private static Serilog.ILogger? _earlyLogger;
    private ApplicationLifecycleService? _lifecycleService;

    #endregion

    #region Public Properties

    /// <summary>Глобальный менеджер иконок трея для таймеров и будильников.</summary>
    public TrayIconManager TrayIconManager => _trayIconManager!;

    /// <summary>Возвращает DI-контейнер сервисов приложения.</summary>
    public IServiceProvider Services => _serviceProvider;

    #endregion

    #region Public Methods

    /// <summary>
    /// Открывает окно настроек приложения.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel для передачи в окно настроек.</param>
    public void ShowSettingsWindow(MainWindowViewModel mainViewModel)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var ws = ((App)Current).Services.GetService(typeof(IWindowService)) as IWindowService;
            ws?.OpenSettingsWindow();
        });
    }

    /// <summary>
    /// Открывает окно аналоговых часов, если оно ещё не открыто.
    /// </summary>
    public void ShowAnalogClockWindow()
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("DI ServiceProvider is not initialized.");
        var windowService = _serviceProvider.GetRequiredService<IWindowService>();
        windowService.OpenAnalogClockWindow();
    }

    #endregion

    #region Application Lifecycle

    /// <summary>
    /// Обработчик запуска приложения.
    /// </summary>
    /// <param name="e">Аргументы запуска.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // Временный логгер для single instance событий
        _earlyLogger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget", "logs", "clock-widget-singleinstance.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // Проверка на единственный экземпляр
        if (!SingleInstance.Start())
        {
            _earlyLogger.Information("[App] Application instance already running (early logger)");
            var current = System.Diagnostics.Process.GetCurrentProcess();
            foreach (var process in System.Diagnostics.Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                {
                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                    _earlyLogger.Information($"[App] Focus transferred to main window (pid={process.Id}) (early logger)");
                    break;
                }
            }
            _earlyLogger.Information("[App] Exiting duplicate instance (early logger)");
            Serilog.Log.CloseAndFlush();
            Shutdown();
            return;
        }

        var logLevel = LoggingConfigurator.ParseLogLevelFromArgs(Environment.GetCommandLineArgs());
        LoggingConfigurator.ConfigureLogging(logLevel);
        _serviceProvider = ServiceConfigurator.ConfigureServices();
        _trayIconManager = _serviceProvider.GetRequiredService<TrayIconManager>();
        _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        _lifecycleService = _serviceProvider.GetRequiredService<ApplicationLifecycleService>();
        _lifecycleService.RegisterLifecycleHandlers(this);

        // Запуск сервиса времени для обновления виджетов
        var timeService = _serviceProvider.GetRequiredService<ITimeService>();
        timeService.Start();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
        };
        this.DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
            args.Handled = true;
        };

        // Восстанавливаем коллекции таймеров и будильников до инициализации UI
        TimersAndAlarmsViewModel.Instance.LoadTimersAndAlarms();

        // Прогрев сервисов и ViewModel для главного окна
        _serviceProvider.GetRequiredService<ISettingsService>();
        var mainVm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        var mainLogger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
        var settingsVm = _serviceProvider.GetRequiredService<SettingsWindowViewModel>();
        var logger = _serviceProvider.GetRequiredService<ILogger<SettingsWindow>>();
        var prewarmedSettingsWindow = new SettingsWindow(settingsVm, logger);
        prewarmedSettingsWindow.Hide();
        var windowService = _serviceProvider.GetRequiredService<IWindowService>();
        if (windowService is WindowService wsImpl)
            wsImpl.SetSettingsWindow(prewarmedSettingsWindow);
        if (mainVm.ShowDigitalClock)
            windowService.OpenMainWindow();
        _trayIconManager.InitializeMainTrayIcon(mainVm, _serviceProvider, _logger);
        if (mainVm.ShowAnalogClock)
            windowService.OpenAnalogClockWindow();

        base.OnStartup(e);
        _logger?.LogInformation("[App] Application starting");
    }

    /// <summary>
    /// Обработчик завершения приложения.
    /// </summary>
    /// <param name="e">Аргументы завершения.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        SingleInstance.Stop();
        try
        {
            _logger?.LogInformation("[App] Application shutting down (DI)");
            _lifecycleService?.GracefulShutdown();
            base.OnExit(e);
            _logger?.LogInformation("[App] Application shutdown completed (DI)");
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[App] Error during application shutdown (DI)");
            throw;
        }
    }

    #endregion
}

