using System;
using System.Windows;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.ViewModels;

namespace ClockWidgetApp.Services
{
    /// <summary>
    /// Сервис для управления жизненным циклом приложения: graceful shutdown, сохранение настроек, обработка исключений.
    /// </summary>
    public class ApplicationLifecycleService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApplicationLifecycleService> _logger;
        private readonly ISettingsService _settingsService;
        private readonly ITimeService _timeService;
        private readonly TimersAndAlarmsViewModel _timersAndAlarmsViewModel;

        public ApplicationLifecycleService(
            IServiceProvider serviceProvider,
            ILogger<ApplicationLifecycleService> logger,
            ISettingsService settingsService,
            ITimeService timeService,
            TimersAndAlarmsViewModel timersAndAlarmsViewModel)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settingsService = settingsService;
            _timeService = timeService;
            _timersAndAlarmsViewModel = timersAndAlarmsViewModel;
        }

        /// <summary>
        /// Подписывает обработчики событий жизненного цикла приложения.
        /// </summary>
        public void RegisterLifecycleHandlers(System.Windows.Application app)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                _logger?.LogError(args.ExceptionObject as Exception, "[App] Unhandled exception (AppDomain)");
            };
            app.DispatcherUnhandledException += (sender, args) =>
            {
                _logger?.LogError(args.Exception, "[App] Unhandled exception (Dispatcher)");
                args.Handled = true;
            };
            app.SessionEnding += App_SessionEnding;
        }

        /// <summary>
        /// Сохраняет настройки приложения и таймеры/будильники.
        /// </summary>
        public void SaveOnShutdown()
        {
            try
            {
                _settingsService?.SaveBufferedSettings();
                _timersAndAlarmsViewModel.SaveTimersAndAlarms();
                _logger?.LogInformation("[App] Settings and timers/alarms saved on shutdown/session ending");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[App] Error saving settings/timers/alarms on shutdown/session ending");
            }
        }

        /// <summary>
        /// Выполняет graceful shutdown: останавливает сервисы, сохраняет состояние.
        /// </summary>
        public void GracefulShutdown()
        {
            SaveOnShutdown();
            if (_timeService != null)
            {
                _timeService.Stop();
                _timeService.Dispose();
                _logger?.LogInformation("[App] Time service disposed");
            }
        }

        /// <summary>
        /// Обработчик завершения сессии пользователя.
        /// </summary>
        private void App_SessionEnding(object? sender, SessionEndingCancelEventArgs e)
        {
            _logger?.LogInformation("[App] Session ending: {Reason}", e.ReasonSessionEnding);
            SaveOnShutdown();
        }
    }
} 