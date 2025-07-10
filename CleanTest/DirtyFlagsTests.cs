using System;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Xunit;
using System.Reflection;
using ClockWidgetApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CleanTest;

/// <summary>
/// Тесты для проверки корректной работы dirty-флагов и логики их сброса.
/// </summary>
public class DirtyFlagsTests
{
    private static void ResetWidgetSettingsDirtyFlag()
    {
        typeof(ClockWidgetApp.App).GetField("_widgetSettingsDirty", BindingFlags.Static | BindingFlags.NonPublic)
            ?.SetValue(null, 0);
    }
    private static void ResetTimersAlarmsDirtyFlag()
    {
        typeof(ClockWidgetApp.App).GetField("_timersAlarmsDirty", BindingFlags.Static | BindingFlags.NonPublic)
            ?.SetValue(null, 0);
    }
    private static int GetWidgetSettingsDirtyFlag()
    {
        return (int)(typeof(ClockWidgetApp.App).GetField("_widgetSettingsDirty", BindingFlags.Static | BindingFlags.NonPublic)
            ?.GetValue(null) ?? 0);
    }
    private static int GetTimersAlarmsDirtyFlag()
    {
        return (int)(typeof(ClockWidgetApp.App).GetField("_timersAlarmsDirty", BindingFlags.Static | BindingFlags.NonPublic)
            ?.GetValue(null) ?? 0);
    }

    [Fact]
    public void WidgetSettingsDirtyFlag_IsSet_OnUpdate()
    {
        var logger = new TestLogger<SettingsService>();
        var service = new SettingsService(logger);
        ResetWidgetSettingsDirtyFlag();
        service.UpdateSettings(s => s.BackgroundOpacity = 0.5);
        Assert.Equal(1, GetWidgetSettingsDirtyFlag());
    }

    [Fact]
    public void TimersAlarmsDirtyFlag_IsSet_OnTimerChange()
    {
        var timer = new TimerEntryViewModel(TimeSpan.FromMinutes(1));
        ResetTimersAlarmsDirtyFlag();
        timer.Duration = TimeSpan.FromMinutes(2);
        Assert.Equal(1, GetTimersAlarmsDirtyFlag());
    }

    [Fact]
    public void TimersAlarmsDirtyFlag_IsSet_OnAlarmChange()
    {
        var alarm = new AlarmEntryViewModel(TimeSpan.FromMinutes(1));
        ResetTimersAlarmsDirtyFlag();
        alarm.IsEnabled = !alarm.IsEnabled;
        Assert.Equal(1, GetTimersAlarmsDirtyFlag());
    }

    [Fact]
    public void TimersAlarmsDirtyFlag_IsSet_OnLongTimerChange()
    {
        var soundService = new TestSoundService();
        var longTimer = new LongTimerEntryViewModel(DateTime.Now.AddMinutes(10), soundService, "Test");
        ResetTimersAlarmsDirtyFlag();
        longTimer.Name = "Changed";
        Assert.Equal(1, GetTimersAlarmsDirtyFlag());
    }
}

// Простейший заглушечный логгер для тестов
public class TestLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) => NullScope.Instance;
    bool Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    void Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    private class NullScope : IDisposable { public static NullScope Instance { get; } = new NullScope(); public void Dispose() { } }
}

// Простейший заглушечный ISoundService для тестов длинных таймеров
public class TestSoundService : ISoundService
{
    public ISoundHandle PlaySoundInstance(string path, bool loop = false) => new NullSoundHandle();
    public void PlaySound(string path) { }
    public void PlaySound(string path, bool loop = false) { }
    public void PlayCuckooSound(int hour) { }
    public void PlayHalfHourChime() { }
    public void StopAll() { }
    public void StopSound() { }
} 