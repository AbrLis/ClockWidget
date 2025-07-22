namespace ClockWidgetApp.Helpers;

/// <summary>
/// Вспомогательный класс для парсинга, коррекции и валидации времени.
/// </summary>
public static class TimeValidationHelper
{
    /// <summary>
    /// Преобразует строку времени в TimeSpan (форматы: HH:mm, HH:mm:ss, mm:ss, ss).
    /// </summary>
    public static bool TryParseTimeSpan(string input, out TimeSpan result)
        => TimeSpan.TryParse(input, out result);

    /// <summary>
    /// Преобразует строку в int, если пусто — возвращает 0.
    /// </summary>
    public static bool TryParseOrZero(string? value, out int result)
    {
        if (string.IsNullOrWhiteSpace(value)) { result = 0; return true; }
        return int.TryParse(value, out result);
    }

    /// <summary>
    /// Корректирует часы, минуты и секунды в допустимые диапазоны для таймера.
    /// </summary>
    public static void CorrectTimerTime(ref string hours, ref string minutes, ref string seconds)
    {
        if (TryParseOrZero(hours, out var h))
        {
            if (h > 23) hours = "23";
            else if (h < 0) hours = "0";
        }
        if (TryParseOrZero(minutes, out var m))
        {
            if (m > 59) minutes = "59";
            else if (m < 0) minutes = "0";
        }
        if (TryParseOrZero(seconds, out var s))
        {
            if (s > 59) seconds = "59";
            else if (s < 0) seconds = "0";
        }
    }

    /// <summary>
    /// Корректирует часы и минуты в допустимые диапазоны для будильника.
    /// </summary>
    public static void CorrectAlarmTime(ref string hours, ref string minutes)
    {
        if (TryParseOrZero(hours, out var h))
        {
            if (h > 23) hours = "23";
            else if (h < 0) hours = "0";
        }
        if (TryParseOrZero(minutes, out var m))
        {
            if (m > 59) minutes = "59";
            else if (m < 0) minutes = "0";
        }
    }
}