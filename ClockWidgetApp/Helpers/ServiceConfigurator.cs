using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Serilog;

namespace ClockWidgetApp.Helpers
{
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
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<TrayIconManager>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<SettingsWindowViewModel>();
            services.AddSingleton<ApplicationLifecycleService>(sp =>
                new ApplicationLifecycleService(
                    sp,
                    sp.GetRequiredService<ILogger<ApplicationLifecycleService>>(),
                    sp.GetRequiredService<ISettingsService>(),
                    sp.GetRequiredService<ITimeService>(),
                    ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance
                )
            );
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
            return services.BuildServiceProvider();
        }
    }
} 