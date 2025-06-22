using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
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
    private static readonly object _lock = new object();
    private static Serilog.ILogger? _serilogLogger;

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
        lock (_lock)
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
                .MinimumLevel.Is(LogEventLevel.Warning); // Минимальный уровень для всех логов

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
                    if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    // Настройка файлового логирования с фильтром только указанных уровней
                    loggerConfiguration.WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(e => fileLevels.Contains(e.Level))
                        .WriteTo.File(
                            logPath,
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                            retainedFileCountLimit: 30)); // Храним логи за последние 30 дней
                }
                catch (Exception ex)
                {
                    // Если не удалось настроить логирование в файл, выводим ошибку в консоль
                    Debug.WriteLine($"[ERROR] Failed to setup file logging: {ex.Message}");
                    Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                }
            }

            _serilogLogger = loggerConfiguration.CreateLogger();

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddSerilog(_serilogLogger, dispose: true)
                    .AddDebug();
            });

            _isInitialized = true;
        }
    }

    /// <summary>
    /// Освобождает ресурсы логгера.
    /// </summary>
    public static void Dispose()
    {
        lock (_lock)
        {
            // Serilog.ILogger не реализует IDisposable, поэтому не вызываем Dispose
            _serilogLogger = null;
            
            if (_loggerFactory != null)
            {
                _loggerFactory.Dispose();
                _loggerFactory = null;
            }
            
            _isInitialized = false;
        }
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
} 