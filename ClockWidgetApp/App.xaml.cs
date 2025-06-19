using System.Windows;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private static SettingsService? _settingsService;
    private static MainWindowViewModel? _mainViewModel;
    private static TimeService? _timeService;
    private MainWindow? _mainWindow;
    private ILogger<App>? _logger;
    private NotifyIcon? _notifyIcon = null;
    private ContextMenuStrip? _trayMenu = null;

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
            _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
        };
        this.DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
            args.Handled = true;
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            _logger?.LogInformation("[App] Starting application");
            
            // Инициализируем сервисы
            SettingsService = new SettingsService();
            TimeService = new TimeService();
            _logger?.LogInformation("[App] Services initialized");
            
            // Загружаем настройки
            var settings = SettingsService.CurrentSettings;
            _logger?.LogInformation("[App] Settings loaded: {Settings}", 
                System.Text.Json.JsonSerializer.Serialize(settings));

            if (settings.ShowDigitalClock)
            {
                _mainWindow = new MainWindow();
                System.Windows.Application.Current.MainWindow = _mainWindow;
                MainViewModel = _mainWindow.ViewModel;
                TimeService.Start();
                _logger?.LogInformation("[App] Time service started");
                _mainWindow.Show();
                _mainWindow.Activate();
                _logger?.LogInformation("[App] Main window created, shown and activated");
            }
            else
            {
                // Инициализируем ViewModel напрямую для работы с треем и настройками
                MainViewModel = new MainWindowViewModel();
                TimeService.Start();
                _logger?.LogInformation("[App] Time service started (no main window)");
                _logger?.LogInformation("[App] Main window not created (ShowDigitalClock == false)");
            }

            // Добавляем иконку в трее
            InitializeTrayIcon();

            // Устанавливаем режим завершения приложения
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
        if (e.Button == MouseButtons.Left)
        {
            // Показываем контекстное меню под курсором
            _trayMenu?.Show(Cursor.Position);
        }
    }

    private void ShowSettingsWindow()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.OpenSettingsWindow();
            }
        });
    }

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
}

