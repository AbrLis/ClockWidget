namespace ClockWidgetApp.Services
{
    using ClockWidgetApp.ViewModels;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис для управления жизненным циклом приложения: graceful shutdown, сохранение настроек, обработка исключений.
    /// </summary>
    public class ApplicationLifecycleService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApplicationLifecycleService> _logger;
        private readonly IAppDataService _appDataService;
        private readonly ITimeService _timeService;
        private readonly TimersAndAlarmsViewModel _timersAndAlarmsViewModel;

        public ApplicationLifecycleService(
            IServiceProvider serviceProvider,
            ILogger<ApplicationLifecycleService> logger,
            IAppDataService appDataService,
            ITimeService timeService,
            TimersAndAlarmsViewModel timersAndAlarmsViewModel)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appDataService = appDataService;
            _timeService = timeService;
            _timersAndAlarmsViewModel = timersAndAlarmsViewModel;
        }

        /// <summary>
        /// Подписывает обработчики событий жизненного цикла приложения.
        /// </summary>
        public void RegisterLifecycleHandlers(System.Windows.Application app)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
            app.DispatcherUnhandledException += (sender, args) =>
            {
                _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
                args.Handled = true;
            };
        }

        /// <summary>
        /// Выполняет graceful shutdown: останавливает сервисы (без сохранения данных).
        /// </summary>
        public void GracefulShutdown()
        {
            if (_timeService != null)
            {
                _timeService.Stop();
                _timeService.Dispose();
                _logger?.LogInformation("[App] Time service disposed");
            }
            _logger?.LogInformation("[App] GracefulShutdownAsync завершён: все сервисы остановлены");
        }
    }
} 