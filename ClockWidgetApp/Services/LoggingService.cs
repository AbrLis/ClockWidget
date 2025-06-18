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
    /// Преобразует строку с уровнями логирования в массив LogEventLevel.
    /// </summary>
    private static LogEventLevel[] ParseLogLevels(string? levels)
    {
        if (string.IsNullOrEmpty(levels)) return Array.Empty<LogEventLevel>();

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
    public static void Initialize(bool logToFile = true)
    {
        if (_isInitialized) return;

        // Проверяем переменную окружения для уровней логирования в файл
        var fileLogLevels = Environment.GetEnvironmentVariable("FILE_LOG_LEVELS");
        
        // По умолчанию для файла используем WARN, ERROR, FATAL если не указано иное
        var fileLevels = ParseLogLevels(fileLogLevels);
        if (fileLevels.Length == 0)
        {
            fileLevels = new[] { LogEventLevel.Warning, LogEventLevel.Error, LogEventLevel.Fatal };
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose); // Минимальный уровень для всех логов

        if (logToFile)
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClockWidget",
                    "logs",
                    "clock-widget-.log");

                // Создаем директорию для логов, если она не существует
                var logDir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDir))
                {
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }
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
            catch (Exception ex)
            {
                // Если не удалось настроить логирование в файл, выводим ошибку в консоль
                Console.WriteLine($"[ERROR] Failed to setup file logging: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
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