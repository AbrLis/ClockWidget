using System;
using System.IO;
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
    /// Инициализирует сервис логирования.
    /// </summary>
    /// <param name="logToFile">Флаг, указывающий, нужно ли записывать логи в файл.</param>
    public static void Initialize(bool logToFile = false)
    {
        if (_isInitialized) return;

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        if (logToFile)
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget",
                "logs",
                "clock-widget-.log");

            loggerConfiguration.WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");
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