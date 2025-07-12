using System;
using System.IO;
using System.Linq;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using Xunit;

namespace CleanTest;

/// <summary>
/// Тесты для AppDataService: проверка сохранения и загрузки всех данных приложения.
/// </summary>
public class AppDataServiceTests
{
    /// <summary>
    /// Проверяет, что настройки виджета сохраняются и загружаются корректно.
    /// </summary>
    [Fact]
    public void SaveAndLoad_WidgetSettings_ShouldPersistValidSettings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "widget_settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var service = new AppDataService(settingsFile, timersFile);
        var testSettings = new WidgetSettings
        {
            BackgroundOpacity = 0.7,
            TextOpacity = 0.8,
            FontSize = 20,
            ShowSeconds = true,
            Language = "ru"
        };
        service.Data.WidgetSettings = testSettings;
        service.Save();
        service.Data.WidgetSettings = new WidgetSettings(); // сбрасываем
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(testSettings.BackgroundOpacity, loaded.BackgroundOpacity);
        Assert.Equal(testSettings.TextOpacity, loaded.TextOpacity);
        Assert.Equal(testSettings.FontSize, loaded.FontSize);
        Assert.Equal(testSettings.ShowSeconds, loaded.ShowSeconds);
        Assert.Equal(testSettings.Language, loaded.Language);
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что таймеры, будильники и длинные таймеры сохраняются и загружаются корректно.
    /// </summary>
    [Fact]
    public void SaveAndLoad_TimersAndAlarms_ShouldPersistValidData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "widget_settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var service = new AppDataService(settingsFile, timersFile);
        service.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) });
        service.Data.Alarms.Add(new AlarmPersistModel { AlarmTime = new TimeSpan(6, 0, 0), IsEnabled = true, NextTriggerDateTime = DateTime.Now.AddDays(1) });
        service.Data.LongTimers.Add(new LongTimerPersistModel { TargetDateTime = DateTime.Now.AddHours(2), Name = "TestLongTimer" });
        service.Save();
        service.Data.Timers.Clear();
        service.Data.Alarms.Clear();
        service.Data.LongTimers.Clear();
        service.Load();
        Assert.Single(service.Data.Timers);
        Assert.Single(service.Data.Alarms);
        Assert.Single(service.Data.LongTimers);
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при повреждённом файле настроек возвращаются значения по умолчанию.
    /// </summary>
    [Fact]
    public void Load_WidgetSettings_ShouldReturnDefault_WhenFileIsCorrupted()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "widget_settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(settingsFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при пустом файле настроек возвращаются значения по умолчанию.
    /// </summary>
    [Fact]
    public void Load_WidgetSettings_ShouldReturnDefault_WhenFileIsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "widget_settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(settingsFile, "");
        var service = new AppDataService(settingsFile, timersFile);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при повреждённом файле таймеров коллекции остаются пустыми.
    /// </summary>
    [Fact]
    public void Load_TimersAndAlarms_ShouldBeEmpty_WhenFileIsCorrupted()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "widget_settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        File.WriteAllText(timersFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile);
        service.Load();
        Assert.Empty(service.Data.Timers);
        Assert.Empty(service.Data.Alarms);
        Assert.Empty(service.Data.LongTimers);
        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Проверяет, что при наличии резервной копии настроек она используется при повреждении основного файла.
    /// </summary>
    [Fact]
    public void Load_WidgetSettings_ShouldRestoreFromBackup()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var settingsFile = Path.Combine(tempDir, "widget_settings.json");
        var timersFile = Path.Combine(tempDir, "timers_alarms.json");
        var backupFile = Path.ChangeExtension(settingsFile, ".bak");
        var goodSettings = new WidgetSettings { BackgroundOpacity = 0.55, Language = "ru" };
        File.WriteAllText(backupFile, System.Text.Json.JsonSerializer.Serialize(goodSettings));
        File.WriteAllText(settingsFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(goodSettings.BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(goodSettings.Language, loaded.Language);
        Directory.Delete(tempDir, true);
    }
} 