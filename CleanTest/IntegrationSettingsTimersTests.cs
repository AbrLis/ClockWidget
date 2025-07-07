using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;
using System.Collections.Generic;

namespace CleanTest;

/// <summary>
/// Интеграционные тесты: совместная работа SettingsService и TimersAndAlarmsPersistenceService.
/// </summary>
public class IntegrationSettingsTimersTests
{
    /// <summary>
    /// Проверяет, что настройки и таймеры/будильники сохраняются и восстанавливаются согласованно.
    /// </summary>
    [Fact]
    public void SaveAndRestore_SettingsAndTimers_ShouldBeConsistent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var loggerMock = new Mock<ILogger<SettingsService>>();
        var settingsService = new SettingsService(loggerMock.Object, settingsFile, timersFile);
        var timersService = new TimersAndAlarmsPersistenceService(timersFile);

        var testSettings = new WidgetSettings { BackgroundOpacity = 0.5, Language = "ru" };
        var testTimers = new TimersAndAlarmsPersistModel
        {
            Timers = new List<TimerPersistModel> { new TimerPersistModel { Duration = System.TimeSpan.FromMinutes(15) } },
            Alarms = new List<AlarmPersistModel> { new AlarmPersistModel { AlarmTime = new System.TimeSpan(8, 0, 0), IsEnabled = true, NextTriggerDateTime = System.DateTime.Now.AddDays(1) } }
        };

        // Act
        settingsService.SaveSettings(testSettings);
        timersService.Save(testTimers);

        // Восстанавливаем через оба сервиса
        var loadedSettings = settingsService.LoadSettings();
        var loadedTimers = timersService.Load();

        // Assert
        Assert.NotNull(loadedSettings);
        Assert.Equal(testSettings.BackgroundOpacity, loadedSettings.BackgroundOpacity, 5);
        Assert.Equal(testSettings.Language, loadedSettings.Language);
        Assert.NotNull(loadedTimers);
        Assert.Single(loadedTimers.Timers);
        Assert.Single(loadedTimers.Alarms);
        Assert.Equal(testTimers.Timers[0].Duration, loadedTimers.Timers[0].Duration);
        Assert.Equal(testTimers.Alarms[0].AlarmTime, loadedTimers.Alarms[0].AlarmTime);
        Assert.Equal(testTimers.Alarms[0].IsEnabled, loadedTimers.Alarms[0].IsEnabled);
        Assert.Equal(testTimers.Alarms[0].NextTriggerDateTime?.ToString(), loadedTimers.Alarms[0].NextTriggerDateTime?.ToString());

        // Clean up
        Directory.Delete(tempDir, true);
    }
} 