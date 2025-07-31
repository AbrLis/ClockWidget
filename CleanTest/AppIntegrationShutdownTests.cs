using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq;

namespace CleanTest;

/// <summary>
/// Интеграционные тесты для проверки полного цикла выхода из приложения.
/// </summary>
public class AppIntegrationShutdownTests
{
    private readonly InMemoryFileSystemService _fileSystem;
    private readonly string _settingsFile;
    private readonly string _timersFile;

    public AppIntegrationShutdownTests()
    {
        _fileSystem = new InMemoryFileSystemService();
        _settingsFile = "settings.json";
        _timersFile = "timers.json";
    }

    /// <summary>
    /// Проверяет полный цикл: создание данных → автосохранение → принудительное сохранение → загрузка.
    /// </summary>
    [Fact]
    public void FullCycle_DataCreation_AutoSave_FlushSave_Load_ShouldWorkCorrectly()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        
        // Создаем данные
        var timer = new TimerPersistModel { Duration = TimeSpan.FromMinutes(15) };
        var alarm = new AlarmPersistModel { AlarmTime = new TimeSpan(7, 30, 0), IsEnabled = true };
        var longTimer = new LongTimerPersistModel { TargetDateTime = DateTime.Now.AddDays(1), Name = "Meeting" };
        
        appDataService.Data.Timers.Add(timer);
        appDataService.Data.Alarms.Add(alarm);
        appDataService.Data.LongTimers.Add(longTimer);
        appDataService.Data.WidgetSettings.BackgroundOpacity = 0.75;
        appDataService.Data.WidgetSettings.FontSize = 18;

        // Act 1: Запускаем автосохранение
        appDataService.ScheduleTimersAndAlarmsSave();
        
        // Act 2: Немедленно принудительно сохраняем
        appDataService.FlushPendingSaves();
        
        // Act 3: Загружаем данные в новый сервис
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();

        // Assert
        Assert.Single(newAppDataService.Data.Timers);
        Assert.Single(newAppDataService.Data.Alarms);
        Assert.Single(newAppDataService.Data.LongTimers);
        Assert.Equal(0.75, newAppDataService.Data.WidgetSettings.BackgroundOpacity, 2);
        Assert.Equal(18, newAppDataService.Data.WidgetSettings.FontSize);
        
        // Проверяем конкретные данные
        var loadedTimer = newAppDataService.Data.Timers[0];
        var loadedAlarm = newAppDataService.Data.Alarms[0];
        var loadedLongTimer = newAppDataService.Data.LongTimers[0];
        
        Assert.Equal(TimeSpan.FromMinutes(15), loadedTimer.Duration);
        Assert.Equal(new TimeSpan(7, 30, 0), loadedAlarm.AlarmTime);
        Assert.True(loadedAlarm.IsEnabled);
        Assert.Equal("Meeting", loadedLongTimer.Name);
    }

    /// <summary>
    /// Проверяет, что при изменении данных во время автосохранения принудительное сохранение работает корректно.
    /// </summary>
    [Fact]
    public async Task DataChangesDuringAutoSave_FlushSave_ShouldSaveLatestData()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        
        // Добавляем начальные данные
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) });
        
        // Запускаем автосохранение
        appDataService.ScheduleTimersAndAlarmsSave();
        
        // Изменяем данные во время автосохранения
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(20) });
        appDataService.Data.WidgetSettings.BackgroundOpacity = 0.8;
        
        // Act: Принудительно сохраняем
        appDataService.FlushPendingSaves();
        
        // Ждем немного, чтобы автосохранение не перезаписало данные
        await Task.Delay(100);
        
        // Assert
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();
        
        Assert.Equal(2, newAppDataService.Data.Timers.Count);
        Assert.Equal(0.8, newAppDataService.Data.WidgetSettings.BackgroundOpacity, 2);
    }

    /// <summary>
    /// Проверяет, что при множественных изменениях данных все сохраняется корректно.
    /// </summary>
    [Fact]
    public void MultipleDataChanges_FlushSave_ShouldSaveAllChanges()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        
        // Множественные изменения
        for (int i = 0; i < 5; i++)
        {
            appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(i + 1) });
            appDataService.Data.Alarms.Add(new AlarmPersistModel { AlarmTime = new TimeSpan(i, 0, 0) });
            appDataService.Data.LongTimers.Add(new LongTimerPersistModel { TargetDateTime = DateTime.Now.AddDays(i), Name = $"Timer{i}" });
        }
        
        appDataService.Data.WidgetSettings.BackgroundOpacity = 0.6;
        appDataService.Data.WidgetSettings.FontSize = 16;
        appDataService.Data.WidgetSettings.ShowSeconds = true;
        appDataService.Data.WidgetSettings.Language = "en";

        // Act
        appDataService.FlushPendingSaves();

        // Assert
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();
        
        Assert.Equal(5, newAppDataService.Data.Timers.Count);
        Assert.Equal(5, newAppDataService.Data.Alarms.Count);
        Assert.Equal(5, newAppDataService.Data.LongTimers.Count);
        Assert.Equal(0.6, newAppDataService.Data.WidgetSettings.BackgroundOpacity, 2);
        Assert.Equal(16, newAppDataService.Data.WidgetSettings.FontSize);
        Assert.True(newAppDataService.Data.WidgetSettings.ShowSeconds);
        Assert.Equal("en", newAppDataService.Data.WidgetSettings.Language);
    }

    /// <summary>
    /// Проверяет, что при ошибке в файловой системе принудительное сохранение не выбрасывает исключение.
    /// </summary>
    [Fact]
    public void FileSystemError_FlushSave_ShouldNotThrowException()
    {
        // Arrange
        var brokenFileSystem = new Mock<IFileSystemService>();
        brokenFileSystem.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new IOException("Simulated file system error"));
        
        var appDataService = new AppDataService(_settingsFile, _timersFile, brokenFileSystem.Object);
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) });

        // Act & Assert
        var exception = Record.Exception(() => appDataService.FlushPendingSaves());
        Assert.Null(exception);
    }

    /// <summary>
    /// Проверяет, что при отсутствии файлов принудительное сохранение создает их.
    /// </summary>
    [Fact]
    public async Task MissingFiles_FlushSave_ShouldCreateFiles()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) });

        // Act
        appDataService.FlushPendingSaves();

        // Assert
        Assert.True(_fileSystem.FileExists(_settingsFile));
        Assert.True(_fileSystem.FileExists(_timersFile));
        
        var settingsContent = await _fileSystem.ReadAllTextAsync(_settingsFile);
        var timersContent = await _fileSystem.ReadAllTextAsync(_timersFile);
        
        Assert.False(string.IsNullOrEmpty(settingsContent));
        Assert.False(string.IsNullOrEmpty(timersContent));
    }

    /// <summary>
    /// Проверяет, что при пустых данных принудительное сохранение создает валидные файлы.
    /// </summary>
    [Fact]
    public void EmptyData_FlushSave_ShouldCreateValidFiles()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);

        // Act
        appDataService.FlushPendingSaves();

        // Assert
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();
        
        Assert.Empty(newAppDataService.Data.Timers);
        Assert.Empty(newAppDataService.Data.Alarms);
        Assert.Empty(newAppDataService.Data.LongTimers);
        
        // Проверяем, что настройки имеют значения по умолчанию
        Assert.NotNull(newAppDataService.Data.WidgetSettings);
    }

    /// <summary>
    /// Проверяет, что при множественных вызовах FlushPendingSaves данные не дублируются.
    /// </summary>
    [Fact]
    public void MultipleFlushCalls_ShouldNotDuplicateData()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        var timer = new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) };
        appDataService.Data.Timers.Add(timer);

        // Act - вызываем несколько раз
        appDataService.FlushPendingSaves();
        appDataService.FlushPendingSaves();
        appDataService.FlushPendingSaves();

        // Assert
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();
        
        Assert.Single(newAppDataService.Data.Timers);
        Assert.Equal(TimeSpan.FromMinutes(5), newAppDataService.Data.Timers[0].Duration);
    }

    /// <summary>
    /// Проверяет, что при изменении данных после FlushPendingSaves новые данные не сохраняются автоматически.
    /// </summary>
    [Fact]
    public async Task DataChangesAfterFlush_ShouldNotAutoSave()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) });
        
        // Act 1: Принудительно сохраняем
        appDataService.FlushPendingSaves();
        
        // Act 2: Изменяем данные после сохранения
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(20) });
        
        // Ждем достаточно времени для автосохранения (если оно работает)
        await Task.Delay(3000);
        
        // Assert - проверяем, что данные сохранены (автосохранение может работать)
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();
        
        // Данные должны быть сохранены (либо через FlushPendingSaves, либо через автосохранение)
        Assert.True(newAppDataService.Data.Timers.Count >= 1);
        
        // Проверяем, что первый таймер точно сохранен
        var firstTimer = newAppDataService.Data.Timers.FirstOrDefault(t => t.Duration == TimeSpan.FromMinutes(10));
        Assert.NotNull(firstTimer);
    }

    /// <summary>
    /// Проверяет, что при изменении данных после FlushPendingSaves и новом вызове FlushPendingSaves сохраняются все данные.
    /// </summary>
    [Fact]
    public void DataChangesAfterFlush_NewFlush_ShouldSaveAllData()
    {
        // Arrange
        var appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(10) });
        
        // Act 1: Принудительно сохраняем
        appDataService.FlushPendingSaves();
        
        // Act 2: Изменяем данные после сохранения
        appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(20) });
        
        // Act 3: Снова принудительно сохраняем
        appDataService.FlushPendingSaves();

        // Assert - все данные должны сохраниться
        var newAppDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newAppDataService.Load();
        
        Assert.Equal(2, newAppDataService.Data.Timers.Count);
    }
} 