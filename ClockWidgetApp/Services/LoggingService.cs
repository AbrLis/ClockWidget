using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для логирования приложения.
/// </summary>
public static class LoggingService
{
    private static ILoggerFactory? _loggerFactory;
    private static bool _isInitialized;

    /// <summary>
    /// Преобразует строковый уровень логирования в LogEventLevel.
    /// </summary>
    private static LogEventLevel ParseLogLevel(string? level)
    {
        if (string.IsNullOrEmpty(level)) return LogEventLevel.Debug;

        return level.ToUpperInvariant() switch
        {
            "TRACE" => LogEventLevel.Verbose,
            "DEBUG" => LogEventLevel.Debug,
            "INFO" => LogEventLevel.Information,
            "WARN" => LogEventLevel.Warning,
            "ERROR" => LogEventLevel.Error,
            "FATAL" => LogEventLevel.Fatal,
            _ => LogEventLevel.Debug
        };
    }

    /// <summary>
    /// Преобразует строку с уровнями логирования в массив LogEventLevel.
    /// </summary>
    private static LogEventLevel[] ParseLogLevels(string? levels)
    {
        if (string.IsNullOrEmpty(levels)) return new[] { LogEventLevel.Debug };

        return levels.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(level => level.Trim().ToUpperInvariant())
            .Select(level => level switch
            {
                "TRACE" => LogEventLevel.Verbose,
                "DEBUG" => LogEventLevel.Debug,
                "INFO" => LogEventLevel.Information,
                "WARN" => LogEventLevel.Warning,
                "ERROR" => LogEventLevel.Error,
                "FATAL" => LogEventLevel.Fatal,
                _ => LogEventLevel.Debug
            })
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Инициализирует сервис логирования.
    /// </summary>
    /// <param name="logToFile">Флаг, указывающий, нужно ли записывать логи в файл.</param>
    public static void Initialize(bool logToFile = false)
    {
        if (_isInitialized) return;

        // Проверяем переменные окружения
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var consoleLogLevel = Environment.GetEnvironmentVariable("CONSOLE_LOG_LEVEL");
        var fileLogLevels = Environment.GetEnvironmentVariable("FILE_LOG_LEVELS");
        
        // Определяем уровни логирования
        var consoleMinimumLevel = ParseLogLevel(consoleLogLevel);
        var fileLevels = ParseLogLevels(fileLogLevels);

        // Если не указаны уровни для файла, используем уровень консоли
        if (fileLevels.Length == 0)
        {
            fileLevels = new[] { consoleMinimumLevel };
        }

        // Если переменные окружения установлены, используем их значения для logToFile
        if (!string.IsNullOrEmpty(environment))
        {
            logToFile = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(consoleMinimumLevel)
            .WriteTo.Console(
                restrictedToMinimumLevel: consoleMinimumLevel,
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        if (logToFile)
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget",
                "logs",
                "clock-widget-.log");

            // Создаем директорию для логов, если она не существует
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // Добавляем фильтр для каждого уровня логирования
            foreach (var level in fileLevels)
            {
                loggerConfiguration.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == level)
                    .WriteTo.File(
                        logPath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"));
            }
        }

        var logger = loggerConfiguration.CreateLogger();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(logger);
        });

        _isInitialized = true;
    }

    /// <summary>
    /// Создает логгер для указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип, для которого создается логгер.</typeparam>
    /// <returns>Логгер для указанного типа.</returns>
    public static ILogger<T> CreateLogger<T>()
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        return _loggerFactory!.CreateLogger<T>();
    }

    /// <summary>
    /// Создает логгер для указанного имени категории.
    /// </summary>
    /// <param name="categoryName">Имя категории логгера.</param>
    /// <returns>Логгер для указанной категории.</returns>
    public static Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        return _loggerFactory!.CreateLogger(categoryName);
    }
} 