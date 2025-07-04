using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;

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
    /// <summary>Иконка приложения в трее.</summary>
    private NotifyIcon? _notifyIcon = null;
    /// <summary>Контекстное меню трея.</summary>
    private ContextMenuStrip? _trayMenu = null;
    /// <summary>Пункт меню "Показать цифровые часы".</summary>
    private ToolStripMenuItem? _showDigitalItem;
    /// <summary>Пункт меню "Показать аналоговые часы".</summary>
    private ToolStripMenuItem? _showAnalogItem;
    /// <summary>Пункт меню "Настройки".</summary>
    private ToolStripMenuItem? _settingsItem;
    /// <summary>Пункт меню "Таймеры и будильники".</summary>
    private ToolStripMenuItem? _timerAlarmSettingsItem;
    /// <summary>Пункт меню "Выход".</summary>
    private ToolStripMenuItem? _exitItem;
    /// <summary>Менеджер иконок трея (через DI).</summary>
    private TrayIconManager? _trayIconManager;
    /// <summary>Временный логгер для single instance событий (до инициализации DI/основного логгера).</summary>
    private static Serilog.ILogger? _earlyLogger;

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

        var logLevel = ParseLogLevelFromArgs(Environment.GetCommandLineArgs());
        ConfigureLogging(logLevel);
        ConfigureServices();
        _logger = _serviceProvider.GetRequiredService<ILogger<App>>();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
        };
        this.DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
            args.Handled = true;
        };
        this.SessionEnding += App_SessionEnding;

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
        InitializeTrayIcon(mainVm);
        if (mainVm.ShowAnalogClock)
            windowService.OpenAnalogClockWindow();

        base.OnStartup(e);
        _logger?.LogInformation("[App] Application starting");
        var timeService = _serviceProvider.GetRequiredService<ITimeService>();
        timeService.Start();
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
            SaveSettingsOnShutdown();
            var timeService = _serviceProvider.GetRequiredService<ITimeService>();
            if (timeService != null)
            {
                timeService.Stop();
                timeService.Dispose();
                _logger?.LogInformation("[App] Time service disposed");
            }
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
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

    #region Private Methods and Event Handlers

    /// <summary>
    /// Обработчик события завершения сессии пользователя (выход из системы, завершение работы и т.д.).
    /// Сохраняет настройки приложения.
    /// </summary>
    private void App_SessionEnding(object? sender, SessionEndingCancelEventArgs e)
    {
        _logger?.LogInformation("[App] Session ending: {Reason}", e.ReasonSessionEnding);
        SaveSettingsOnShutdown();
    }

    /// <summary>
    /// Сохраняет настройки приложения при завершении работы или сессии.
    /// </summary>
    private void SaveSettingsOnShutdown()
    {
        try
        {
            var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            settingsService?.SaveBufferedSettings();
            TimersAndAlarmsViewModel.Instance.SaveTimersAndAlarms();
            _logger?.LogInformation("[App] Settings and timers/alarms saved on shutdown/session ending");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[App] Error saving settings/timers/alarms on shutdown/session ending");
        }
    }

    /// <summary>
    /// Разбирает уровень логирования из аргументов командной строки.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Уровень логирования или null, если не найден.</returns>
    private string? ParseLogLevelFromArgs(string[] args)
    {
        foreach (var arg in args)
        {
            if (arg.StartsWith("--log-level=", StringComparison.OrdinalIgnoreCase))
                return arg.Substring("--log-level=".Length);
        }
        return null;
    }

    /// <summary>
    /// Конфигурирует систему логирования приложения.
    /// </summary>
    /// <param name="logLevelOverride">Переопределение уровня логирования (опционально).</param>
    private void ConfigureLogging(string? logLevelOverride = null)
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockWidget",
            "logs",
            "clock-widget-.log");

        var logLevel = logLevelOverride ?? "Warning";
        var level = logLevel.ToUpperInvariant() switch
        {
            "TRACE" => Serilog.Events.LogEventLevel.Verbose,
            "DEBUG" => Serilog.Events.LogEventLevel.Debug,
            "INFO" => Serilog.Events.LogEventLevel.Information,
            "WARNING" => Serilog.Events.LogEventLevel.Warning,
            "ERROR" => Serilog.Events.LogEventLevel.Error,
            "FATAL" => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Warning
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30,
                restrictedToMinimumLevel: level)
            .CreateLogger();
    }

    /// <summary>
    /// Регистрирует сервисы и ViewModel в DI-контейнере.
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ITimeService, TimeService>();
        services.AddSingleton<ISoundService, SoundService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<TrayIconManager>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<AnalogClockViewModel>(sp =>
            new AnalogClockViewModel(
                sp.GetRequiredService<ITimeService>(),
                sp.GetRequiredService<ISettingsService>(),
                sp.GetRequiredService<MainWindowViewModel>(),
                sp.GetRequiredService<ILogger<AnalogClockViewModel>>(),
                sp.GetRequiredService<IWindowService>()
            )
        );
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });
        _serviceProvider = services.BuildServiceProvider();
        _trayIconManager = _serviceProvider.GetRequiredService<TrayIconManager>();
    }

    /// <summary>
    /// Инициализирует иконку приложения в системном трее и её меню.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel для управления состоянием меню.</param>
    private void InitializeTrayIcon(MainWindowViewModel mainViewModel)
    {
        _trayMenu = new ContextMenuStrip();
        string lang = Helpers.LocalizationManager.CurrentLanguage;
        _showDigitalItem = new ToolStripMenuItem();
        _showAnalogItem = new ToolStripMenuItem();
        _settingsItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_Settings", lang));
        _timerAlarmSettingsItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_TimerAlarmSettings", lang));
        _exitItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_Exit", lang));
        var separator = new ToolStripSeparator();
        var separator2 = new ToolStripSeparator();
        _showDigitalItem.Click += (s, e) =>
        {
            if (mainViewModel != null)
                mainViewModel.ShowDigitalClock = !mainViewModel.ShowDigitalClock;
        };
        _showAnalogItem.Click += (s, e) =>
        {
            if (mainViewModel != null)
                mainViewModel.ShowAnalogClock = !mainViewModel.ShowAnalogClock;
        };
        _settingsItem.Click += (s, e) =>
        {
            if (_serviceProvider != null)
            {
                var ws = _serviceProvider.GetRequiredService<IWindowService>();
                if (ws is WindowService windowService)
                    windowService.OpenSettingsWindow(false);
                else
                    ws.OpenSettingsWindow();
            }
        };
        _timerAlarmSettingsItem.Click += (s, e) =>
        {
            if (_serviceProvider != null)
            {
                var ws = _serviceProvider.GetRequiredService<IWindowService>();
                if (ws is WindowService windowService)
                    windowService.OpenSettingsWindow(true);
                else
                    ws.OpenSettingsWindow();
            }
        };
        _exitItem.Click += (s, e) => Current.Shutdown();
        _trayMenu.Items.Add(_showDigitalItem);
        _trayMenu.Items.Add(_showAnalogItem);
        _trayMenu.Items.Add(separator2);
        _trayMenu.Items.Add(_settingsItem);
        _trayMenu.Items.Add(_timerAlarmSettingsItem);
        _trayMenu.Items.Add(separator);
        _trayMenu.Items.Add(_exitItem);
        string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "app.ico");
        if (!File.Exists(iconPath))
        {
            _logger?.LogWarning("[App] Tray icon file not found: {Path}", iconPath);
            return;
        }
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = new Icon(iconPath);
        _notifyIcon.Visible = true;
        _notifyIcon.Text = Helpers.LocalizationManager.GetString("Tray_IconText", lang);
        _notifyIcon.ContextMenuStrip = _trayMenu;
        _notifyIcon.MouseUp += new MouseEventHandler(NotifyIcon_MouseUp);
    }

    /// <summary>
    /// Обновляет текст пунктов меню трея в зависимости от состояния приложения.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel.</param>
    private void UpdateTrayMenuItems(MainWindowViewModel mainViewModel)
    {
        string lang = Helpers.LocalizationManager.CurrentLanguage;
        if (_showDigitalItem != null && mainViewModel != null)
        {
            _showDigitalItem.Text = mainViewModel.ShowDigitalClock
                ? Helpers.LocalizationManager.GetString("Tray_HideDigital", lang)
                : Helpers.LocalizationManager.GetString("Tray_ShowDigital", lang);
        }
        if (_showAnalogItem != null && mainViewModel != null)
        {
            _showAnalogItem.Text = mainViewModel.ShowAnalogClock
                ? Helpers.LocalizationManager.GetString("Tray_HideAnalog", lang)
                : Helpers.LocalizationManager.GetString("Tray_ShowAnalog", lang);
        }
        if (_settingsItem != null)
            _settingsItem.Text = Helpers.LocalizationManager.GetString("Tray_Settings", lang);
        if (_timerAlarmSettingsItem != null)
            _timerAlarmSettingsItem.Text = Helpers.LocalizationManager.GetString("Tray_TimerAlarmSettings", lang);
        if (_exitItem != null)
            _exitItem.Text = Helpers.LocalizationManager.GetString("Tray_Exit", lang);
        if (_notifyIcon != null)
            _notifyIcon.Text = Helpers.LocalizationManager.GetString("Tray_IconText", lang);
    }

    /// <summary>
    /// Обработчик клика по иконке в трее.
    /// </summary>
    private void NotifyIcon_MouseUp(object? sender, MouseEventArgs e)
    {
        var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        if (e.Button == MouseButtons.Right && mainViewModel != null)
        {
            UpdateTrayMenuItems(mainViewModel);
            _trayMenu?.Show();
            return;
        }
        if (e.Button == MouseButtons.Left)
        {
            Current.Dispatcher.Invoke(static () =>
            {
                var ws = ((App)Current).Services.GetService(typeof(IWindowService)) as IWindowService;
                ws?.BringAllToFront();
            });
        }
    }

    #endregion
}

