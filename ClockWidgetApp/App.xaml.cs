using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;

namespace ClockWidgetApp;

/// <summary>
/// Класс приложения WPF. Точка входа и управление жизненным циклом.
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private ILogger<App>? _logger;
    private NotifyIcon? _notifyIcon = null;
    private ContextMenuStrip? _trayMenu = null;
    private ToolStripMenuItem? _showDigitalItem;
    private ToolStripMenuItem? _showAnalogItem;
    private ToolStripMenuItem? _settingsItem;
    private ToolStripMenuItem? _exitItem;

    /// <summary>
    /// Конструктор приложения. Инициализирует логирование и обработчики ошибок.
    /// </summary>
    public App()
    {
        var logLevel = ParseLogLevelFromArgs(Environment.GetCommandLineArgs());
        ConfigureLogging(logLevel);
        ConfigureServices();
        _logger = _serviceProvider?.GetRequiredService<ILogger<App>>();
        // Глобальный обработчик необработанных исключений
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
        };
        this.DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
            args.Handled = true;
        };
        // Подписка на завершение сессии (выход из системы, завершение работы и т.д.)
        this.SessionEnding += App_SessionEnding;
    }

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
            var settingsService = _serviceProvider?.GetRequiredService<ISettingsService>();
            settingsService?.SaveBufferedSettings();
            _logger?.LogInformation("[App] Settings saved on shutdown/session ending");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[App] Error saving settings on shutdown/session ending");
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
            {
                return arg.Substring("--log-level=".Length);
            }
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
        // Регистрация сервисов
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ITimeService, TimeService>();
        services.AddSingleton<ISoundService, SoundService>();
        services.AddSingleton<IWindowService, WindowService>();
        // Регистрация ViewModel
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<AnalogClockViewModel>();
        // Логгеры
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger, dispose: true);
        });
        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Обработчик запуска приложения.
    /// </summary>
    /// <param name="e">Аргументы запуска.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // Прогрев сервисов и ViewModel для главного окна
        _serviceProvider!.GetRequiredService<ISettingsService>();
        var mainVm = _serviceProvider!.GetRequiredService<MainWindowViewModel>();
        var mainLogger = _serviceProvider!.GetRequiredService<ILogger<MainWindow>>();
        var settingsVm = _serviceProvider!.GetRequiredService<SettingsWindowViewModel>();
        var logger = _serviceProvider!.GetRequiredService<ILogger<SettingsWindow>>();
        base.OnStartup(e);
        _logger?.LogInformation("[App] Application starting");
        var timeService = _serviceProvider!.GetRequiredService<ITimeService>();
        timeService.Start();
        if (_serviceProvider == null)
            throw new InvalidOperationException("DI ServiceProvider is not initialized.");
        var windowService = _serviceProvider.GetRequiredService<IWindowService>();
        if (mainVm.ShowDigitalClock)
            windowService.OpenMainWindow();
        // Показываем иконку в трее
        InitializeTrayIcon(mainVm);
        // Показываем аналоговые часы, если они включены в настройках через сервис
        if (mainVm.ShowAnalogClock)
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("DI ServiceProvider is not initialized.");
            windowService.OpenAnalogClockWindow();
        }
    }

    /// <summary>
    /// Инициализирует иконку приложения в системном трее и её меню.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel для управления состоянием меню.</param>
    private void InitializeTrayIcon(MainWindowViewModel mainViewModel)
    {
        _trayMenu = new ContextMenuStrip();
        string lang = ClockWidgetApp.Helpers.LocalizationManager.CurrentLanguage;
        _showDigitalItem = new ToolStripMenuItem();
        _showAnalogItem = new ToolStripMenuItem();
        _settingsItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_Settings", lang));
        _exitItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_Exit", lang));
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
            if (mainViewModel != null)
            {
                ShowSettingsWindow(mainViewModel);
            }
        };
        _exitItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();
        _trayMenu.Items.Add(_showDigitalItem);
        _trayMenu.Items.Add(_showAnalogItem);
        _trayMenu.Items.Add(_settingsItem);
        _trayMenu.Items.Add(_exitItem);
        string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "app.ico");
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
        _notifyIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(NotifyIcon_MouseUp);
    }

    /// <summary>
    /// Обновляет текст пунктов меню трея в зависимости от состояния приложения.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel.</param>
    private void UpdateTrayMenuItems(MainWindowViewModel mainViewModel)
    {
        string lang = ClockWidgetApp.Helpers.LocalizationManager.CurrentLanguage;
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
        {
            _settingsItem.Text = Helpers.LocalizationManager.GetString("Tray_Settings", lang);
        }
        if (_exitItem != null)
        {
            _exitItem.Text = Helpers.LocalizationManager.GetString("Tray_Exit", lang);
        }
        if (_notifyIcon != null)
        {
            _notifyIcon.Text = Helpers.LocalizationManager.GetString("Tray_IconText", lang);
        }
    }

    /// <summary>
    /// Обработчик клика по иконке в трее.
    /// При левом клике все активные окна приложения выводятся на передний план.
    /// При правом клике открывается контекстное меню.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события мыши.</param>
    private void NotifyIcon_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        var mainViewModel = _serviceProvider?.GetRequiredService<MainWindowViewModel>();
        if (e.Button == System.Windows.Forms.MouseButtons.Right && mainViewModel != null)
        {
            UpdateTrayMenuItems(mainViewModel);
            _trayMenu?.Show();
            return;
        }
        if (e.Button == System.Windows.Forms.MouseButtons.Left)
        {
            // Выводим все активные окна на передний план через WindowService
            System.Windows.Application.Current.Dispatcher.Invoke(static () =>
            {
                var ws = ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) as IWindowService;
                ws?.BringAllToFront();
            });
        }
    }

    /// <summary>
    /// Открывает окно настроек приложения.
    /// </summary>
    /// <param name="mainViewModel">Главная ViewModel для передачи в окно настроек.</param>
    public void ShowSettingsWindow(MainWindowViewModel mainViewModel)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var ws = ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) as IWindowService;
            ws?.OpenSettingsWindow();
        });
    }

    /// <summary>
    /// Обработчик завершения приложения.
    /// </summary>
    /// <param name="e">Аргументы завершения.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger?.LogInformation("[App] Application shutting down (DI)");
            SaveSettingsOnShutdown();
            var timeService = _serviceProvider?.GetRequiredService<ITimeService>();
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

    /// <summary>
    /// Открывает окно аналоговых часов, если оно ещё не открыто.
    /// </summary>
    public void ShowAnalogClockWindow()
    {
        // Теперь используется сервис для открытия окна аналоговых часов
        if (_serviceProvider == null)
            throw new InvalidOperationException("DI ServiceProvider is not initialized.");
        var windowService = _serviceProvider.GetRequiredService<IWindowService>();
        windowService.OpenAnalogClockWindow();
    }

    /// <summary>
    /// Возвращает DI-контейнер сервисов приложения.
    /// </summary>
    public IServiceProvider Services => _serviceProvider!;
}

