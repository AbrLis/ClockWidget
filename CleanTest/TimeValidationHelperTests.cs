using ClockWidgetApp.Helpers;
using Xunit;

namespace CleanTest;

/// <summary>
/// Тесты для TimeValidationHelper: валидация и коррекция времени.
/// </summary>
public class TimeValidationHelperTests
{
    /// <summary>
    /// Проверяет корректный парсинг строковых значений времени.
    /// </summary>
    [Theory]
    [InlineData("12:34", 0, 12, 34, 0)]
    [InlineData("01:02:03", 0, 1, 2, 3)]
    [InlineData("5", 5, 0, 0, 0)]
    public void TryParseTimeSpan_ShouldParseSupportedFormats(string input, int days, int h, int m, int s)
    {
        var result = TimeValidationHelper.TryParseTimeSpan(input, out var ts);
        Assert.True(result);
        Assert.Equal(days, ts.Days);
        Assert.Equal(h, ts.Hours);
        Assert.Equal(m, ts.Minutes);
        Assert.Equal(s, ts.Seconds);
    }

    /// <summary>
    /// Проверяет TryParseTimeSpan для неподдерживаемых или невалидных форматов.
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("99:99:99:99")]
    [InlineData("")]
    [InlineData("59:59")]
    [InlineData("123:45:56")]
    public void TryParseTimeSpan_ShouldReturnFalse_OnInvalidOrUnsupported(string input)
    {
        var result = TimeValidationHelper.TryParseTimeSpan(input, out var ts);
        Assert.False(result);
    }

    /// <summary>
    /// Проверяет TryParseOrZero для пустых и невалидных значений.
    /// </summary>
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("abc", 0)]
    [InlineData("42", 42)]
    public void TryParseOrZero_ShouldReturnZeroOrParsed(string? input, int expected)
    {
        var result = TimeValidationHelper.TryParseOrZero(input, out var value);
        Assert.True(result || input == "abc");
        Assert.Equal(expected, value);
    }

    /// <summary>
    /// Проверяет коррекцию времени таймера на граничных и невалидных значениях.
    /// </summary>
    [Theory]
    [InlineData("-1", "-1", "-1", "0", "0", "0")]
    [InlineData("24", "60", "61", "23", "59", "59")]
    [InlineData("5", "30", "15", "5", "30", "15")]
    public void CorrectTimerTime_ShouldClampValues(string h, string m, string s, string eh, string em, string es)
    {
        var hours = h; var minutes = m; var seconds = s;
        TimeValidationHelper.CorrectTimerTime(ref hours, ref minutes, ref seconds);
        Assert.Equal(eh, hours);
        Assert.Equal(em, minutes);
        Assert.Equal(es, seconds);
    }

    /// <summary>
    /// Проверяет коррекцию времени будильника на граничных и невалидных значениях.
    /// </summary>
    [Theory]
    [InlineData("-1", "-1", "0", "0")]
    [InlineData("24", "60", "23", "59")]
    [InlineData("7", "30", "7", "30")]
    public void CorrectAlarmTime_ShouldClampValues(string h, string m, string eh, string em)
    {
        var hours = h; var minutes = m;
        TimeValidationHelper.CorrectAlarmTime(ref hours, ref minutes);
        Assert.Equal(eh, hours);
        Assert.Equal(em, minutes);
    }
} 