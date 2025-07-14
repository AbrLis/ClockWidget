using System;
using System.IO;
using System.Linq;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var service = new AppDataService(settingsFile, timersFile, fs);
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
    }

    /// <summary>
    /// Проверяет, что таймеры, будильники и длинные таймеры сохраняются и загружаются корректно.
    /// </summary>
    [Fact]
    public void SaveAndLoad_TimersAndAlarms_ShouldPersistValidData()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var service = new AppDataService(settingsFile, timersFile, fs);
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
    }

    /// <summary>
    /// Проверяет, что при повреждённом файле настроек возвращаются значения по умолчанию.
    /// </summary>
    [Fact]
    public async Task Load_WidgetSettings_ShouldReturnDefault_WhenFileIsCorrupted()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        await fs.WriteAllTextAsync(settingsFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);
    }

    /// <summary>
    /// Проверяет, что при пустом файле настроек возвращаются значения по умолчанию.
    /// </summary>
    [Fact]
    public async Task Load_WidgetSettings_ShouldReturnDefault_WhenFileIsEmpty()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        await fs.WriteAllTextAsync(settingsFile, "");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);
    }

    /// <summary>
    /// Проверяет, что при повреждённом файле таймеров коллекции остаются пустыми.
    /// </summary>
    [Fact]
    public async Task Load_TimersAndAlarms_ShouldBeEmpty_WhenFileIsCorrupted()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        await fs.WriteAllTextAsync(timersFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        Assert.Empty(service.Data.Timers);
        Assert.Empty(service.Data.Alarms);
        Assert.Empty(service.Data.LongTimers);
    }

    /// <summary>
    /// Проверяет, что при наличии резервной копии настроек она используется при повреждении основного файла.
    /// </summary>
    [Fact]
    public async Task Load_WidgetSettings_ShouldRestoreFromBackup()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var backupFile = Path.ChangeExtension(settingsFile, ".bak");
        var goodSettings = new WidgetSettings { BackgroundOpacity = 0.55, Language = "ru" };
        await fs.WriteAllTextAsync(backupFile, System.Text.Json.JsonSerializer.Serialize(goodSettings));
        await fs.WriteAllTextAsync(settingsFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(goodSettings.BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(goodSettings.Language, loaded.Language);
    }

    /// <summary>
    /// Проверяет, что если и основной, и резервный файл настроек повреждены — возвращаются значения по умолчанию.
    /// </summary>
    [Fact]
    public async Task Load_WidgetSettings_ShouldReturnDefault_WhenFileAndBackupAreCorrupted()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var backupFile = Path.ChangeExtension(settingsFile, ".bak");
        await fs.WriteAllTextAsync(settingsFile, "{ not valid json }");
        await fs.WriteAllTextAsync(backupFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(new WidgetSettings().BackgroundOpacity, loaded.BackgroundOpacity, 5);
        Assert.Equal(new WidgetSettings().Language, loaded.Language);
    }

    /// <summary>
    /// Проверяет, что если и основной, и резервный файл таймеров повреждены — коллекции остаются пустыми.
    /// </summary>
    [Fact]
    public async Task Load_TimersAndAlarms_ShouldRestoreFromBackup()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var backupFile = Path.ChangeExtension(timersFile, ".bak");
        var goodModel = new TimersAndAlarmsPersistModel
        {
            Timers = { new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) } },
            Alarms = { new AlarmPersistModel { AlarmTime = new TimeSpan(7, 0, 0), IsEnabled = true } },
            LongTimers = { new LongTimerPersistModel { Name = "BackupLT", TargetDateTime = DateTime.Now.AddHours(1) } }
        };
        // Теперь сериализуем одиночный объект, а не массив
        await fs.WriteAllTextAsync(backupFile, System.Text.Json.JsonSerializer.Serialize(goodModel));
        await fs.WriteAllTextAsync(timersFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        Assert.Single(service.Data.Timers);
        Assert.Single(service.Data.Alarms);
        Assert.Single(service.Data.LongTimers);
    }

    /// <summary>
    /// Проверяет, что если и основной, и резервный файл таймеров повреждены — коллекции остаются пустыми.
    /// </summary>
    [Fact]
    public async Task Load_TimersAndAlarms_ShouldBeEmpty_WhenFileAndBackupAreCorrupted()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var backupFile = Path.ChangeExtension(timersFile, ".bak");
        await fs.WriteAllTextAsync(timersFile, "{ not valid json }");
        await fs.WriteAllTextAsync(backupFile, "{ not valid json }");
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Load();
        Assert.Empty(service.Data.Timers);
        Assert.Empty(service.Data.Alarms);
        Assert.Empty(service.Data.LongTimers);
    }

    [Fact]
    public async Task SaveAndLoadAsync_WidgetSettings_ShouldPersistValidSettings()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var service = new AppDataService(settingsFile, timersFile, fs);
        var testSettings = new WidgetSettings
        {
            BackgroundOpacity = 0.7,
            TextOpacity = 0.8,
            FontSize = 20,
            ShowSeconds = true,
            Language = "ru"
        };
        service.Data.WidgetSettings = testSettings;
        await service.SaveAsync();
        service.Data.WidgetSettings = new WidgetSettings(); // сбрасываем
        await service.LoadAsync();
        var loaded = service.Data.WidgetSettings;
        Assert.Equal(testSettings.BackgroundOpacity, loaded.BackgroundOpacity);
        Assert.Equal(testSettings.TextOpacity, loaded.TextOpacity);
        Assert.Equal(testSettings.FontSize, loaded.FontSize);
        Assert.Equal(testSettings.ShowSeconds, loaded.ShowSeconds);
        Assert.Equal(testSettings.Language, loaded.Language);
    }

    [Fact]
    public async Task SaveAndLoadAsync_TimersAndAlarms_ShouldPersistValidData()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var service = new AppDataService(settingsFile, timersFile, fs);
        service.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) });
        service.Data.Alarms.Add(new AlarmPersistModel { AlarmTime = new TimeSpan(6, 0, 0), IsEnabled = true, NextTriggerDateTime = DateTime.Now.AddDays(1) });
        service.Data.LongTimers.Add(new LongTimerPersistModel { TargetDateTime = DateTime.Now.AddHours(2), Name = "TestLongTimer" });
        await service.SaveAsync();
        service.Data.Timers.Clear();
        service.Data.Alarms.Clear();
        service.Data.LongTimers.Clear();
        await service.LoadAsync();
        Assert.Single(service.Data.Timers);
        Assert.Single(service.Data.Alarms);
        Assert.Single(service.Data.LongTimers);
    }
}

// Мок-реализация IFileSystemService для изоляции тестов
public class InMemoryFileSystemService : IFileSystemService
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public bool FileExists(string path) => _files.ContainsKey(path);
    public Stream OpenRead(string path)
    {
        if (!_files.ContainsKey(path)) throw new FileNotFoundException(path);
        var bytes = System.Text.Encoding.UTF8.GetBytes(_files[path]);
        return new MemoryStream(bytes);
    }
    public void CreateDirectory(string path) => _directories.Add(path);
    public Task<string> ReadAllTextAsync(string path)
    {
        if (!_files.ContainsKey(path)) throw new FileNotFoundException(path);
        return Task.FromResult(_files[path]);
    }
    public Task WriteAllTextAsync(string path, string contents)
    {
        _files[path] = contents;
        return Task.CompletedTask;
    }
    public Task CreateBackupAsync(string sourcePath, string backupPath)
    {
        if (!_files.ContainsKey(sourcePath)) throw new FileNotFoundException(sourcePath);
        _files[backupPath] = _files[sourcePath];
        return Task.CompletedTask;
    }
    public void DeleteFileIfExists(string path) => _files.Remove(path);
} 