using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp;

/// <summary>
/// Класс приложения WPF. Точка входа и управление жизненным циклом.
/// </summary>
public partial class App : System.Windows.Application
{
    private static SettingsService? _settingsService;
    private static MainWindowViewModel? _mainViewModel;
    private static TimeService? _timeService;
    private ILogger<App>? _logger;
    private NotifyIcon? _notifyIcon = null;
    private ContextMenuStrip? _trayMenu = null;
    public static SettingsWindow? SettingsWindowInstance { get; set; }

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

    /// <summary>
    /// Конструктор приложения. Инициализирует логирование и обработчики ошибок.
    /// </summary>
    public App()
    {
        // Инициализируем логирование
        LoggingService.Initialize();
        _logger = LoggingService.CreateLogger<App>();

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
    }

    /// <summary>
    /// Гарантирует, что главное окно приложения (MainWindow) существует в единственном экземпляре,
    /// показывает и активирует его. Если окно уже существует, просто активирует его.
    /// Используется для управления жизненным циклом цифрового виджета.
    /// </summary>
    private void EnsureMainWindow()
    {
        var mainWindow = ClockWidgetApp.MainWindow.Instance;
        if (mainWindow == null)
        {
            mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow;
            MainViewModel = mainWindow.ViewModel;
        }
        else
        {
            MainViewModel = mainWindow.ViewModel;
        }
        mainWindow.Show();
        mainWindow.Activate();
        _logger?.LogInformation("[App] Main window shown and activated");
    }

    /// <summary>
    /// Обработчик запуска приложения.
    /// </summary>
    /// <param name="e">Аргументы запуска.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            _logger?.LogInformation("[App] Starting application");

            // Инициализация сервисов и настроек
            SettingsService = new SettingsService();
            TimeService = new TimeService();
            _logger?.LogInformation("[App] Services initialized");

            var settings = SettingsService.CurrentSettings;
            _logger?.LogInformation("[App] Settings loaded: {Settings}", 
                System.Text.Json.JsonSerializer.Serialize(settings));

            if (settings.ShowDigitalClock)
            {
                EnsureMainWindow();
            }
            else
            {
                MainViewModel = new MainWindowViewModel();
                _logger?.LogInformation("[App] Main window not created (ShowDigitalClock == false)");
            }

            TimeService.Start();
            _logger?.LogInformation("[App] Time service started");

            InitializeTrayIcon();
            this.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            _logger?.LogInformation("[App] Application started successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[App] Error during application startup");
            throw;
        }
    }

    private void InitializeTrayIcon()
    {
        _trayMenu = new ContextMenuStrip();
        var settingsItem = new ToolStripMenuItem("Настройки");
        settingsItem.Click += (s, e) => ShowSettingsWindow();
        var exitItem = new ToolStripMenuItem("Закрыть");
        exitItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();
        _trayMenu.Items.Add(settingsItem);
        _trayMenu.Items.Add(exitItem);

        string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "app.ico");
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = new Icon(iconPath);
        _notifyIcon.Visible = true;
        _notifyIcon.Text = "Clock Widget App";
        _notifyIcon.ContextMenuStrip = _trayMenu;
        _notifyIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(NotifyIcon_MouseUp);
    }

    private void NotifyIcon_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            _trayMenu?.Show();
        }
    }

    private void ShowSettingsWindow()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = ClockWidgetApp.MainWindow.Instance;
            if (SettingsWindowInstance == null || !SettingsWindowInstance.IsVisible)
            {
                SettingsWindowInstance = new SettingsWindow(mainWindow?.ViewModel ?? MainViewModel);
                SettingsWindowInstance.Closed += (s, e) => SettingsWindowInstance = null;
                SettingsWindowInstance.Show();
            }
            else
            {
                SettingsWindowInstance.Activate();
            }
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
            _logger?.LogInformation("[App] Application shutting down");
            
            // Останавливаем и освобождаем TimeService
            if (_timeService != null)
            {
                _timeService.Stop();
                _timeService.Dispose();
                _logger?.LogInformation("[App] Time service disposed");
            }
            
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            
            base.OnExit(e);
            _logger?.LogInformation("[App] Application shutdown completed");
            
            // Освобождаем ресурсы логгера
            LoggingService.Dispose();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[App] Error during application shutdown");
            throw;
        }
    }

    public void ShowMainWindowIfNeeded()
    {
        var mainWindow = ClockWidgetApp.MainWindow.Instance;
        if (mainWindow == null)
        {
            mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow;
            MainViewModel = mainWindow.ViewModel;
            mainWindow.Show();
            mainWindow.Activate();
        }
        else if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
            mainWindow.Activate();
        }
    }
}

