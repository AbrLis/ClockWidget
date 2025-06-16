using System;
using System.Diagnostics;

namespace ClockWidgetApp.Helpers;

/// <summary>
/// Вспомогательный класс для отладочного вывода.
/// </summary>
public static class DebugHelper
{
    private static readonly bool _isDebugEnabled;

    static DebugHelper()
    {
        // Проверяем наличие переменной окружения DEBUG_OUTPUT
        _isDebugEnabled = Environment.GetEnvironmentVariable("DEBUG_OUTPUT") == "1";
    }

    /// <summary>
    /// Выводит отладочное сообщение, если включен режим отладки.
    /// </summary>
    /// <param name="message">Сообщение для вывода.</param>
    public static void WriteLine(string message)
    {
        if (_isDebugEnabled)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }

    /// <summary>
    /// Выводит отладочное сообщение об ошибке, если включен режим отладки.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="ex">Исключение.</param>
    public static void WriteError(string message, Exception ex)
    {
        if (_isDebugEnabled)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {message}");
            Debug.WriteLine($"Exception: {ex}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
} 