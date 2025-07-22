namespace ClockWidgetApp.Helpers
{
    using System.IO;
    using Serilog;

    /// <summary>
    /// Статический класс для конфигурации логирования приложения.
    /// </summary>
    public static class LoggingConfigurator
    {
        /// <summary>
        /// Разбирает уровень логирования из аргументов командной строки.
        /// </summary>
        public static string? ParseLogLevelFromArgs(string[] args)
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
        public static void ConfigureLogging(string? logLevelOverride = null)
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
    }
}