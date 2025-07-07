using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using Xunit;
using System.IO;
using System.Collections.Generic;

namespace CleanTest;

/// <summary>
/// Тесты для TimersAndAlarmsPersistenceService: проверка сохранения и загрузки таймеров и будильников.
/// </summary>
public class TimersAndAlarmsPersistenceServiceTests
{
    /// <summary>
    /// Проверяет, что таймеры и будильники сохраняются и загружаются корректно.
    /// </summary>
    [Fact]
    public void SaveAndLoad_ShouldPersistValidData()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "timers_alarms.json");
        var service = new TimersAndAlarmsPersistenceService(file);
        var model = new TimersAndAlarmsPersistModel
        {
            Timers = new List<TimerPersistModel> { new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) } },
            Alarms = new List<AlarmPersistModel> { new AlarmPersistModel { AlarmTime = new TimeSpan(7, 30, 0), IsEnabled = true, NextTriggerDateTime = DateTime.Now.AddDays(1) } }
        };

        // Act
        service.Save(model);
        var loaded = service.Load();

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

    /// <summary>
    /// Проверяет, что при отсутствии файла возвращается null.
    /// </summary>
    [Fact]
    public void Load_ShouldReturnNull_WhenFileDoesNotExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "timers_alarms.json");
        var service = new TimersAndAlarmsPersistenceService(file);

        // Act
        var loaded = service.Load();

        // Assert
        Assert.Null(loaded);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при повреждённом файле выбрасывается исключение.
    /// </summary>
    [Fact]
    public void Load_ShouldThrowException_WhenFileIsCorrupted()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(file, "{ this is not valid json }");
        var service = new TimersAndAlarmsPersistenceService(file);

        // Act & Assert
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => service.Load());

        // Clean up
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при пустом файле выбрасывается исключение.
    /// </summary>
    [Fact]
    public void Load_ShouldThrowException_WhenFileIsEmpty()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(file, "");
        var service = new TimersAndAlarmsPersistenceService(file);

        // Act & Assert
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => service.Load());

        // Clean up
        Directory.Delete(tempDir, true);
    }
} 