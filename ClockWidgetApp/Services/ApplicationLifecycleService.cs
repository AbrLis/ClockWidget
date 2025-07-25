namespace ClockWidgetApp.Services
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис для управления жизненным циклом приложения: graceful shutdown, сохранение настроек, обработка исключений.
    /// </summary>
    public class ApplicationLifecycleService(
        ILogger<ApplicationLifecycleService> logger,
        ITimeService timeService)
    {
        /// <summary>
        /// Подписывает обработчики событий жизненного цикла приложения.
        /// </summary>
        public void RegisterLifecycleHandlers(System.Windows.Application app)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
            app.DispatcherUnhandledException += (sender, args) =>
            {
                logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
                args.Handled = true;
            };
        }

        /// <summary>
        /// Выполняет graceful shutdown: останавливает сервисы (без сохранения данных).
        /// </summary>
        public void GracefulShutdown()
        {
            if (timeService != null)
            {
                timeService.Stop();
                timeService.Dispose();
                logger?.LogInformation("[App] Time service disposed");
            }
            logger?.LogInformation("[App] GracefulShutdownAsync завершён: все сервисы остановлены");
        }
    }
}