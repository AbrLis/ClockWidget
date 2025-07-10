using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using ClockWidgetApp.Helpers;
using System.Threading;

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
    /// <summary>
    /// Потокобезопасный флаг наличия несохранённых изменений настроек виджетов.
    /// </summary>
    private static int _widgetSettingsDirty = 0;
    /// <summary>
    /// Потокобезопасный флаг наличия несохранённых изменений таймеров и будильников.
    /// </summary>
    private static int _timersAlarmsDirty = 0;
    /// <summary>
    /// Таймер для периодического сохранения настроек и таймеров.
    /// </summary>
    private System.Windows.Threading.DispatcherTimer? _periodicSaveTimer;

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

    /// <summary>
    /// Устанавливает флаг наличия несохранённых изменений настроек виджетов.
    /// </summary>
    public static void MarkWidgetSettingsDirty() => Interlocked.Exchange(ref _widgetSettingsDirty, 1);
    /// <summary>
    /// Устанавливает флаг наличия несохранённых изменений таймеров и будильников.
    /// </summary>
    public static void MarkTimersAlarmsDirty() => Interlocked.Exchange(ref _timersAlarmsDirty, 1);

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

        // Загрузка таймеров и будильников
        ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance.LoadTimersAndAlarms();

        // Запуск сервиса времени для обновления виджетов
        var timeService = _serviceProvider.GetRequiredService<ITimeService>();
        timeService.Start();

        // Запуск глобального таймера для периодического сохранения
        _periodicSaveTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _periodicSaveTimer.Tick += PeriodicSaveTimer_Tick;
        _periodicSaveTimer.Start();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
        };
        this.DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
            args.Handled = true;
        };

        _serviceProvider.GetRequiredService<ISettingsService>();
        var mainVm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        var mainLogger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
        var settingsVm = _serviceProvider.GetRequiredService<SettingsWindowViewModel>();
        var logger = _serviceProvider.GetRequiredService<ILogger<SettingsWindow>>();
        var prewarmedSettingsWindow = new SettingsWindow(settingsVm, logger);
        prewarmedSettingsWindow.Hide();
        var windowService = _serviceProvider.GetRequiredService<IWindowService>();
        if (windowService is WindowService wsImpl)
        {
            wsImpl.SetSettingsWindow(prewarmedSettingsWindow);
        }
        if (mainVm.ShowDigitalClock)
        {
            windowService.OpenMainWindow();
        }
        _trayIconManager.InitializeMainTrayIcon(mainVm, _serviceProvider, _logger);
        if (mainVm.ShowAnalogClock)
        {
            windowService.OpenAnalogClockWindow();
        }

        base.OnStartup(e);
        _logger?.LogInformation("[App] Application starting");
    }

    /// <summary>
    /// Обработчик периодического таймера сохранения.
    /// </summary>
    private void PeriodicSaveTimer_Tick(object? sender, EventArgs e)
    {
        // Потокобезопасно проверяем и сбрасываем флаги
        if (Interlocked.Exchange(ref _widgetSettingsDirty, 0) == 1)
        {
            try
            {
                var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
                settingsService.SaveBufferedSettings();
                _logger?.LogInformation("[App] Widget settings saved by periodic timer");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[App] Error saving widget settings by periodic timer");
            }
        }
        if (Interlocked.Exchange(ref _timersAlarmsDirty, 0) == 1)
        {
            try
            {
                var timersVm = ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance;
                timersVm.SaveTimersAndAlarms();
                _logger?.LogInformation("[App] Timers/alarms saved by periodic timer");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[App] Error saving timers/alarms by periodic timer");
            }
        }
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

