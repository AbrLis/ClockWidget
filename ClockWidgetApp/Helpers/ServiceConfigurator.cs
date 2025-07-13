namespace ClockWidgetApp.Helpers
{
    using ClockWidgetApp.Services;
    using ClockWidgetApp.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;

    /// <summary>
    /// Статический класс для конфигурации DI-контейнера приложения.
    /// </summary>
    public static class ServiceConfigurator
    {
        /// <summary>
        /// Регистрирует сервисы и ViewModel в DI-контейнере и возвращает ServiceProvider.
        /// </summary>
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            // Регистрируем сервисы приложения
            services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<TrayIconManager>();
            services.AddSingleton<IAppDataService>(sp =>
            {
                var appDataPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "ClockWidget");
                var settingsPath = System.IO.Path.Combine(appDataPath, "widget_settings.json");
                var timersPath = System.IO.Path.Combine(appDataPath, "timers_alarms.json");
                return new AppDataService(settingsPath, timersPath);
            });
            services.AddSingleton<MainWindowViewModel>(sp =>
                new MainWindowViewModel(
                    sp.GetRequiredService<ITimeService>(),
                    sp.GetRequiredService<IAppDataService>(),
                    sp.GetRequiredService<ISoundService>(),
                    sp.GetRequiredService<IWindowService>(),
                    sp.GetRequiredService<ILogger<MainWindowViewModel>>()
                )
            );
            services.AddSingleton<TimersAndAlarmsViewModel>(sp =>
                new TimersAndAlarmsViewModel(
                    sp.GetRequiredService<IAppDataService>(),
                    sp.GetRequiredService<ISoundService>(),
                    sp.GetRequiredService<TrayIconManager>()
                )
            );
            services.AddSingleton<ApplicationLifecycleService>(sp =>
                new ApplicationLifecycleService(
                    sp,
                    sp.GetRequiredService<ILogger<ApplicationLifecycleService>>(),
                    sp.GetRequiredService<IAppDataService>(),
                    sp.GetRequiredService<ITimeService>(),
                    sp.GetRequiredService<TimersAndAlarmsViewModel>()
                )
            );
            services.AddTransient<SettingsWindowViewModel>(sp =>
                new SettingsWindowViewModel(
                    sp.GetRequiredService<MainWindowViewModel>(),
                    sp.GetRequiredService<IAppDataService>(),
                    sp.GetRequiredService<TimersAndAlarmsViewModel>(),
                    sp.GetRequiredService<ILogger<SettingsWindowViewModel>>()
                )
            );
            services.AddTransient<AnalogClockViewModel>(sp =>
                new AnalogClockViewModel(
                    sp.GetRequiredService<ITimeService>(),
                    sp.GetRequiredService<IAppDataService>(),
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
            return services.BuildServiceProvider();
        }
    }
} 