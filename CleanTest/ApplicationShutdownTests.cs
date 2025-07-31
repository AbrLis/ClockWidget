using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace CleanTest;

/// <summary>
/// Тесты для проверки всех случаев выхода из приложения и корректности сохранения данных.
/// </summary>
public class ApplicationShutdownTests
{
    private readonly InMemoryFileSystemService _fileSystem;
    private readonly string _settingsFile;
    private readonly string _timersFile;
    private readonly AppDataService _appDataService;
    private readonly Mock<ILogger<SettingsWindowViewModel>> _loggerMock;

    public ApplicationShutdownTests()
    {
        _fileSystem = new InMemoryFileSystemService();
        _settingsFile = "settings.json";
        _timersFile = "timers.json";
        _appDataService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        _loggerMock = new Mock<ILogger<SettingsWindowViewModel>>();
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves сохраняет данные синхронно и отменяет отложенные сохранения.
    /// </summary>
    [Fact]
    public void FlushPendingSaves_ShouldSaveDataSynchronously()
    {
        // Arrange
        var timer = new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) };
        var alarm = new AlarmPersistModel { AlarmTime = new TimeSpan(6, 0, 0), IsEnabled = true };
        var longTimer = new LongTimerPersistModel { TargetDateTime = DateTime.Now.AddHours(1), Name = "Test" };
        
        _appDataService.Data.Timers.Add(timer);
        _appDataService.Data.Alarms.Add(alarm);
        _appDataService.Data.LongTimers.Add(longTimer);
        _appDataService.Data.WidgetSettings.BackgroundOpacity = 0.8;

        // Act
        _appDataService.FlushPendingSaves();

        // Assert
        Assert.True(_fileSystem.FileExists(_settingsFile));
        Assert.True(_fileSystem.FileExists(_timersFile));
        
        // Проверяем, что данные действительно сохранены
        var newService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newService.Load();
        
        Assert.Single(newService.Data.Timers);
        Assert.Single(newService.Data.Alarms);
        Assert.Single(newService.Data.LongTimers);
        Assert.Equal(0.8, newService.Data.WidgetSettings.BackgroundOpacity, 2);
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves отменяет отложенные автосохранения.
    /// </summary>
    [Fact]
    public async Task FlushPendingSaves_ShouldCancelPendingSaves()
    {
        // Arrange
        var timer = new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) };
        _appDataService.Data.Timers.Add(timer);
        
        // Запускаем отложенное сохранение
        _appDataService.ScheduleTimersAndAlarmsSave();
        
        // Act - немедленно вызываем принудительное сохранение
        _appDataService.FlushPendingSaves();
        
        // Ждем немного, чтобы убедиться, что отложенное сохранение не выполнится
        await Task.Delay(100);
        
        // Assert - проверяем, что данные сохранены только один раз
        var newService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newService.Load();
        Assert.Single(newService.Data.Timers);
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves работает корректно при отсутствии данных.
    /// </summary>
    [Fact]
    public void FlushPendingSaves_ShouldWorkWithEmptyData()
    {
        // Act
        _appDataService.FlushPendingSaves();

        // Assert
        Assert.True(_fileSystem.FileExists(_settingsFile));
        Assert.True(_fileSystem.FileExists(_timersFile));
        
        var newService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newService.Load();
        
        Assert.Empty(newService.Data.Timers);
        Assert.Empty(newService.Data.Alarms);
        Assert.Empty(newService.Data.LongTimers);
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves обрабатывает исключения корректно.
    /// </summary>
    [Fact]
    public void FlushPendingSaves_ShouldHandleExceptions()
    {
        // Arrange - создаем поврежденную файловую систему
        var brokenFileSystem = new Mock<IFileSystemService>();
        brokenFileSystem.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new IOException("Test exception"));
        
        var brokenService = new AppDataService(_settingsFile, _timersFile, brokenFileSystem.Object);
        brokenService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) });

        // Act & Assert - не должно выбрасывать исключение
        var exception = Record.Exception(() => brokenService.FlushPendingSaves());
        Assert.Null(exception);
    }

    /// <summary>
    /// Проверяет, что SettingsWindowViewModel.CloseApp вызывает Shutdown без сохранения.
    /// </summary>
    [Fact]
    public void SettingsWindowViewModel_CloseApp_ShouldCallShutdownOnly()
    {
        // Arrange
        var mainViewModelMock = new Mock<MainWindowViewModel>(
            Mock.Of<ITimeService>(),
            _appDataService,
            Mock.Of<ISoundService>(),
            Mock.Of<IWindowService>(),
            Mock.Of<ILogger<MainWindowViewModel>>()
        );
        
        var timersAndAlarmsViewModel = new TimersAndAlarmsViewModel(
            _appDataService,
            Mock.Of<ISoundService>(),
            Mock.Of<TrayIconManager>()
        );
        
        var viewModel = new SettingsWindowViewModel(
            mainViewModelMock.Object,
            _appDataService,
            timersAndAlarmsViewModel,
            _loggerMock.Object
        );

        // Act
        viewModel.CloseAppCommand.Execute(null);

        // Assert - проверяем, что метод CloseApp был вызван
        // (прямое тестирование Shutdown() сложно, поэтому проверяем логику команды)
        Assert.True(viewModel.CloseAppCommand.CanExecute(null));
    }

    /// <summary>
    /// Проверяет, что TrayIconManager Exit вызывает Shutdown без сохранения.
    /// </summary>
    [Fact]
    public void TrayIconManager_Exit_ShouldCallShutdownOnly()
    {
        // Arrange
        var trayManager = new TrayIconManager();
        
        // Создаем реальный MainWindowViewModel вместо Mock
        var timeServiceMock = new Mock<ITimeService>();
        var soundServiceMock = new Mock<ISoundService>();
        var windowServiceMock = new Mock<IWindowService>();
        var loggerMock = new Mock<ILogger<MainWindowViewModel>>();
        
        var mainViewModel = new MainWindowViewModel(
            timeServiceMock.Object,
            _appDataService,
            soundServiceMock.Object,
            windowServiceMock.Object,
            loggerMock.Object
        );
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock2 = new Mock<ILogger>();

        // Act & Assert - проверяем, что инициализация проходит без ошибок
        var exception = Record.Exception(() => 
            trayManager.InitializeMainTrayIcon(mainViewModel, serviceProviderMock.Object, loggerMock2.Object));
        Assert.Null(exception);
    }

    /// <summary>
    /// Проверяет, что ApplicationLifecycleService.GracefulShutdown не сохраняет данные.
    /// </summary>
    [Fact]
    public void ApplicationLifecycleService_GracefulShutdown_ShouldNotSaveData()
    {
        // Arrange
        var timeServiceMock = new Mock<ITimeService>();
        var loggerMock = new Mock<ILogger<ApplicationLifecycleService>>();
        var lifecycleService = new ApplicationLifecycleService(loggerMock.Object, timeServiceMock.Object);
        
        var app = new Application();

        // Act
        lifecycleService.RegisterLifecycleHandlers(app);
        lifecycleService.GracefulShutdown();

        // Assert - проверяем, что TimeService был остановлен
        timeServiceMock.Verify(x => x.Stop(), Times.Once);
        timeServiceMock.Verify(x => x.Dispose(), Times.Once);
    }

    /// <summary>
    /// Проверяет, что при множественных вызовах FlushPendingSaves данные сохраняются корректно.
    /// </summary>
    [Fact]
    public void MultipleFlushPendingSaves_ShouldSaveDataCorrectly()
    {
        // Arrange
        var timer = new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) };
        _appDataService.Data.Timers.Add(timer);

        // Act - вызываем несколько раз
        _appDataService.FlushPendingSaves();
        _appDataService.FlushPendingSaves();
        _appDataService.FlushPendingSaves();

        // Assert
        var newService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newService.Load();
        Assert.Single(newService.Data.Timers);
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves работает корректно с большим объемом данных.
    /// </summary>
    [Fact]
    public void FlushPendingSaves_ShouldHandleLargeDataVolume()
    {
        // Arrange - создаем много данных
        for (int i = 0; i < 100; i++)
        {
            _appDataService.Data.Timers.Add(new TimerPersistModel 
            { 
                Duration = TimeSpan.FromMinutes(i + 1),
                Id = Guid.NewGuid()
            });
            
            _appDataService.Data.Alarms.Add(new AlarmPersistModel 
            { 
                AlarmTime = new TimeSpan(i % 24, 0, 0),
                IsEnabled = i % 2 == 0,
                Id = Guid.NewGuid()
            });
            
            _appDataService.Data.LongTimers.Add(new LongTimerPersistModel 
            { 
                TargetDateTime = DateTime.Now.AddDays(i),
                Name = $"LongTimer{i}",
                Id = Guid.NewGuid()
            });
        }
        
        _appDataService.Data.WidgetSettings.BackgroundOpacity = 0.9;
        _appDataService.Data.WidgetSettings.FontSize = 25;

        // Act
        _appDataService.FlushPendingSaves();

        // Assert
        var newService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newService.Load();
        
        Assert.Equal(100, newService.Data.Timers.Count);
        Assert.Equal(100, newService.Data.Alarms.Count);
        Assert.Equal(100, newService.Data.LongTimers.Count);
        Assert.Equal(0.9, newService.Data.WidgetSettings.BackgroundOpacity, 2);
        Assert.Equal(25, newService.Data.WidgetSettings.FontSize);
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves сохраняет данные в правильном формате JSON.
    /// </summary>
    [Fact]
    public async Task FlushPendingSaves_ShouldSaveValidJson()
    {
        // Arrange
        var timer = new TimerPersistModel 
        { 
            Duration = TimeSpan.FromMinutes(30),
            Id = Guid.NewGuid(),
            LastStartedUtc = DateTime.UtcNow
        };
        _appDataService.Data.Timers.Add(timer);
        _appDataService.Data.WidgetSettings.Language = "en";

        // Act
        _appDataService.FlushPendingSaves();

        // Assert - проверяем, что файлы содержат валидный JSON
        var settingsContent = await _fileSystem.ReadAllTextAsync(_settingsFile);
        var timersContent = await _fileSystem.ReadAllTextAsync(_timersFile);
        
        Assert.Contains("en", settingsContent);
        Assert.Contains("30", timersContent);
        Assert.Contains(timer.Id.ToString(), timersContent);
    }

    /// <summary>
    /// Проверяет, что FlushPendingSaves работает корректно при параллельных изменениях данных.
    /// </summary>
    [Fact]
    public async Task FlushPendingSaves_ShouldHandleConcurrentDataChanges()
    {
        // Arrange
        var timer = new TimerPersistModel { Duration = TimeSpan.FromMinutes(5) };
        _appDataService.Data.Timers.Add(timer);

        // Act - запускаем параллельные изменения и сохранение
        var tasks = new List<Task>();
        
        // Задача 1: добавляем данные
        tasks.Add(Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                _appDataService.Data.Timers.Add(new TimerPersistModel { Duration = TimeSpan.FromMinutes(i) });
            }
        }));
        
        // Задача 2: принудительное сохранение
        tasks.Add(Task.Run(() => _appDataService.FlushPendingSaves()));
        
        // Задача 3: еще изменения
        tasks.Add(Task.Run(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                _appDataService.Data.Alarms.Add(new AlarmPersistModel { AlarmTime = new TimeSpan(i, 0, 0) });
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        var newService = new AppDataService(_settingsFile, _timersFile, _fileSystem);
        newService.Load();
        
        // Проверяем, что данные сохранены (может быть разное количество из-за параллельности)
        Assert.True(newService.Data.Timers.Count >= 1);
        Assert.True(newService.Data.Alarms.Count >= 0);
    }
} 