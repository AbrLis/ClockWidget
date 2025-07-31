using System.IO;
using System.Windows;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

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
    /// Обработчик запуска приложения (асинхронный).
    /// </summary>
    /// <param name="e">Аргументы запуска.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        System.Console.WriteLine("=== App OnStartup: begin ===");
        // Временный логгер для single instance событий
        _earlyLogger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget", "logs", "clock-widget-singleinstance.log"),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        System.Console.WriteLine("[OnStartup] Early logger created");

        // Проверка на единственный экземпляр
        if (!SingleInstance.Start())
        {
            System.Console.WriteLine("[OnStartup] Duplicate instance detected, shutting down");
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
        System.Console.WriteLine("[OnStartup] Single instance check passed");

        var logsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockWidget",
            "logs");
        if (!Directory.Exists(logsDir))
            Directory.CreateDirectory(logsDir);
        System.Console.WriteLine("[OnStartup] Logs directory ensured");

        var logLevel = LoggingConfigurator.ParseLogLevelFromArgs(Environment.GetCommandLineArgs());
        System.Console.WriteLine($"[OnStartup] Parsed log level: {logLevel}");
        LoggingConfigurator.ConfigureLogging(logLevel);
        System.Console.WriteLine("[OnStartup] Logging configured");
        try
        {
            _serviceProvider = ServiceConfigurator.ConfigureServices();
            System.Console.WriteLine("[OnStartup] DI ServiceProvider configured");
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"[OnStartup] DI error: {ex}");
            throw;
        }
        _trayIconManager = _serviceProvider.GetRequiredService<TrayIconManager>();
        System.Console.WriteLine("[OnStartup] TrayIconManager resolved");
        _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        System.Console.WriteLine("[OnStartup] App logger resolved");
        _lifecycleService = _serviceProvider.GetRequiredService<ApplicationLifecycleService>();
        System.Console.WriteLine("[OnStartup] LifecycleService resolved");
        _lifecycleService.RegisterLifecycleHandlers(this);
        System.Console.WriteLine("[OnStartup] Lifecycle handlers registered");

        // Загрузка всех данных приложения через единый сервис (асинхронно)
        var appDataService = _serviceProvider.GetRequiredService<IAppDataService>();
        System.Console.WriteLine("[OnStartup] AppDataService resolved");
        try
        {
            await appDataService.LoadAsync();
            System.Console.WriteLine("[OnStartup] AppDataService loaded (async)");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[OnStartup] AppDataService load error: {ex}");
            _logger?.LogError(ex, "[App] Error loading AppDataService on startup");
            throw;
        }

        // Запуск сервиса времени для обновления виджетов
        var timeService = _serviceProvider.GetRequiredService<ITimeService>();
        System.Console.WriteLine("[OnStartup] TimeService resolved");
        timeService.Start();
        System.Console.WriteLine("[OnStartup] TimeService started");

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
        };
        this.DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
            args.Handled = true;
        };
        System.Console.WriteLine("[OnStartup] Exception handlers registered");

        var mainVm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        System.Console.WriteLine("[OnStartup] MainWindowViewModel resolved");
        var mainLogger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
        System.Console.WriteLine("[OnStartup] MainWindow logger resolved");
        var settingsVm = _serviceProvider.GetRequiredService<SettingsWindowViewModel>();
        System.Console.WriteLine("[OnStartup] SettingsWindowViewModel resolved");
        var logger = _serviceProvider.GetRequiredService<ILogger<SettingsWindow>>();
        System.Console.WriteLine("[OnStartup] SettingsWindow logger resolved");
        var timersAndAlarmsVm = _serviceProvider.GetRequiredService<TimersAndAlarmsViewModel>();
        var prewarmedSettingsWindow = new SettingsWindow(settingsVm, timersAndAlarmsVm, logger);
        prewarmedSettingsWindow.Hide();
        System.Console.WriteLine("[OnStartup] Prewarmed SettingsWindow created and hidden");
        var windowService = _serviceProvider.GetRequiredService<IWindowService>();
        System.Console.WriteLine("[OnStartup] WindowService resolved");
        if (windowService is WindowService wsImpl)
        {
            wsImpl.SetSettingsWindow(prewarmedSettingsWindow);
            System.Console.WriteLine("[OnStartup] Prewarmed SettingsWindow injected into WindowService");
        }
        if (mainVm.ShowDigitalClock)
        {
            windowService.OpenMainWindow();
            System.Console.WriteLine("[OnStartup] MainWindow opened");
        }
        _trayIconManager.InitializeMainTrayIcon(mainVm, _serviceProvider, _logger);
        System.Console.WriteLine("[OnStartup] Tray icon initialized");
        if (mainVm.ShowAnalogClock)
        {
            windowService.OpenAnalogClockWindow();
            System.Console.WriteLine("[OnStartup] AnalogClockWindow opened");
        }

        base.OnStartup(e);
        _logger?.LogInformation("[App] Application starting");
        System.Console.WriteLine("=== App OnStartup: end ===");
    }

    /// <summary>
    /// Обработчик завершения приложения (синхронный, блокирующий).
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        SingleInstance.Stop();
        try
        {
            _logger?.LogInformation("[App] Application shutting down (DI)");
            
            // Принудительное сохранение всех данных перед закрытием
            var appDataService = _serviceProvider?.GetService(typeof(IAppDataService)) as IAppDataService;
            if (appDataService != null)
            {
                _logger?.LogInformation("[App] Принудительное сохранение данных при закрытии");
                appDataService.FlushPendingSaves();
                _logger?.LogInformation("[App] Данные успешно сохранены");
            }
            
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
