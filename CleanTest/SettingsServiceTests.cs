using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CleanTest;

/// <summary>
/// Тесты для SettingsService: проверка сохранения и загрузки настроек.
/// </summary>
public class SettingsServiceTests
{
    /// <summary>
    /// Проверяет, что настройки сохраняются и загружаются корректно.
    /// </summary>
    [Fact]
    public void SaveAndLoadSettings_ShouldPersistValidSettings()
    {
        // Arrange: создаём временную директорию для теста
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var loggerMock = new Mock<ILogger<SettingsService>>();
        var service = new SettingsService(loggerMock.Object, settingsFile, timersFile);
        var testSettings = new WidgetSettings
        {
            BackgroundOpacity = 0.7,
            TextOpacity = 0.8,
            FontSize = 20,
            ShowSeconds = true,
            Language = "ru"
        };

        // Act: сохраняем и загружаем настройки
        service.SaveSettings(testSettings);
        var loaded = service.LoadSettings();

        // Assert: значения совпадают
        Assert.Equal(testSettings.BackgroundOpacity, loaded.BackgroundOpacity);
        Assert.Equal(testSettings.TextOpacity, loaded.TextOpacity);
        Assert.Equal(testSettings.FontSize, loaded.FontSize);
        Assert.Equal(testSettings.ShowSeconds, loaded.ShowSeconds);
        Assert.Equal(testSettings.Language, loaded.Language);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при невалидном файле возвращаются настройки по умолчанию.
    /// </summary>
    [Fact]
    public void LoadSettings_ShouldReturnDefault_WhenFileIsCorrupted()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(settingsFile, "{ not valid json }");
        var loggerMock = new Mock<ILogger<SettingsService>>();
        var service = new SettingsService(loggerMock.Object, settingsFile, timersFile);

        // Act
        var loaded = service.LoadSettings();

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void LoadSettings_ShouldReturnDefault_WhenFileIsEmpty()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(settingsFile, "");
        var loggerMock = new Mock<ILogger<SettingsService>>();
        var service = new SettingsService(loggerMock.Object, settingsFile, timersFile);

        // Act
        var loaded = service.LoadSettings();

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что UpdateSettings и SaveBufferedSettings сохраняют изменения.
    /// </summary>
    [Fact]
    public void UpdateAndSaveBufferedSettings_ShouldApplyChanges()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var loggerMock = new Mock<ILogger<SettingsService>>();
        var service = new SettingsService(loggerMock.Object, settingsFile, timersFile);

        // Act
        service.UpdateSettings(s => s.Language = "ru");
        service.SaveBufferedSettings();
        var loaded = service.LoadSettings();

        // Assert
        Assert.Equal("ru", loaded.Language);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void SaveAndLoadTimersAndAlarms_ShouldPersistData()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var loggerMock = new Mock<ILogger<SettingsService>>();
        var service = new SettingsService(loggerMock.Object, settingsFile, timersFile);
        var model = new TimersAndAlarmsPersistModel
        {
            Timers = new List<TimerPersistModel> { new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) } },
            Alarms = new List<AlarmPersistModel> { new AlarmPersistModel { AlarmTime = new TimeSpan(6, 0, 0), IsEnabled = true, NextTriggerDateTime = DateTime.Now.AddDays(1) } }
        };

        // Act
        service.SaveTimersAndAlarms(model);
        var loaded = service.LoadTimersAndAlarms();

        // Assert
        Assert.NotNull(loaded);
        Assert.Single(loaded.Timers);
        Assert.Single(loaded.Alarms);
        Assert.Equal(model.Timers[0].Duration, loaded.Timers[0].Duration);
        Assert.Equal(model.Alarms[0].AlarmTime, loaded.Alarms[0].AlarmTime);
        Assert.Equal(model.Alarms[0].IsEnabled, loaded.Alarms[0].IsEnabled);
        Assert.Equal(model.Alarms[0].NextTriggerDateTime?.ToString(), loaded.Alarms[0].NextTriggerDateTime?.ToString());

        // Clean up
        Directory.Delete(tempDir, true);
    }
} 