using ClockWidgetApp.ViewModels;
using Xunit;
using System;
using Moq;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;
using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CleanTest;

/// <summary>
/// Тесты для TimersViewModel: добавление, удаление, запуск, остановка таймера, валидация времени.
/// </summary>
public class TimersViewModelTests
{
    private static TimersViewModel CreateTimersViewModelWithMockData()
    {
        var appData = new AppDataModel();
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(appData);
        return new TimersViewModel(appDataServiceMock.Object);
    }

    /// <summary>
    /// Проверяет, что при валидном вводе таймер добавляется в коллекцию.
    /// </summary>
    [Fact]
    public void AddTimerCommand_ShouldAddTimer_WhenInputIsValid()
    {
        // Arrange
        var vm = CreateTimersViewModelWithMockData();
        vm.NewTimerHours = "0";
        vm.NewTimerMinutes = "1";
        vm.NewTimerSeconds = "5";

        // Act
        vm.AddTimerCommand.Execute(null);

        // Assert
        Assert.Single(vm.Timers);
        Assert.Equal(new TimeSpan(0, 1, 5), vm.Timers[0].Duration);
    }

    /// <summary>
    /// Проверяет, что при невалидном вводе таймер не добавляется.
    /// </summary>
    [Fact]
    public void AddTimerCommand_ShouldNotAddTimer_WhenInputIsInvalid()
    {
        var vm = CreateTimersViewModelWithMockData();
        vm.NewTimerHours = "-1";
        vm.NewTimerMinutes = "0";
        vm.NewTimerSeconds = "0";
        vm.AddTimerCommand.Execute(null);
        Assert.Empty(vm.Timers);
    }

    /// <summary>
    /// Проверяет запуск и остановку таймера через команды.
    /// </summary>
    [Fact]
    public void TimerEntryViewModel_StartStop_ShouldChangeIsRunning()
    {
        var timer = new TimerEntryViewModel(new TimerPersistModel { Duration = TimeSpan.FromSeconds(5) });
        Assert.False(timer.IsRunning);
        timer.StartCommand.Execute(null);
        Assert.True(timer.IsRunning);
        timer.StopCommand.Execute(null);
        Assert.False(timer.IsRunning);
        timer.Dispose();
    }

    /// <summary>
    /// Проверяет корректировку времени таймера на граничных значениях.
    /// </summary>
    [Fact]
    public void CorrectTimerTime_ShouldClampValues()
    {
        var vm = CreateTimersViewModelWithMockData();
        vm.NewTimerHours = "25";
        vm.NewTimerMinutes = "70";
        vm.NewTimerSeconds = "-5";
        vm.CorrectTimerTime();
        Assert.Equal("23", vm.NewTimerHours);
        Assert.Equal("59", vm.NewTimerMinutes);
        Assert.Equal("0", vm.NewTimerSeconds);
    }

    /// <summary>
    /// Проверяет удаление таймера через DeleteTimerCommand в SettingsWindowViewModel.
    /// </summary>
    [Fact]
    public void SettingsWindowViewModel_DeleteTimerCommand_ShouldRemoveTimer()
    {
        var timersVM = CreateTimersViewModelWithMockData();
        timersVM.NewTimerHours = "0";
        timersVM.NewTimerMinutes = "0";
        timersVM.NewTimerSeconds = "10";
        timersVM.AddTimerCommand.Execute(null);
        var timer = timersVM.Timers[0];
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var appDataService = appDataServiceMock.Object;
        var soundService = new Mock<ISoundService>().Object;
        var windowService = new Mock<IWindowService>().Object;
        var mainLogger = new Mock<ILogger<MainWindowViewModel>>().Object;
        var mainVM = new MainWindowViewModel(new Mock<ITimeService>().Object, appDataService, soundService, windowService, mainLogger);
        var trayIconManager = new Mock<TrayIconManager>(MockBehavior.Loose, new object[] { }).Object;
        var timersAndAlarmsVM = new TimersAndAlarmsViewModel(appDataService, soundService, trayIconManager);
        // Копируем таймеры в тестовый экземпляр
        foreach (var t in timersVM.Timers)
            timersAndAlarmsVM.TimersVm.Timers.Add(t);
        var logger = new Mock<ILogger<SettingsWindowViewModel>>().Object;
        var settingsVM = new SettingsWindowViewModel(mainVM, appDataService, timersAndAlarmsVM, logger);
        settingsVM.DeleteTimerCommand.Execute(timersAndAlarmsVM.TimersVm.TimerEntries[0]);
        Assert.Empty(timersAndAlarmsVM.TimersVm.Timers);
    }

    /// <summary>
    /// Проверяет, что при удалении таймера через DeleteTimerCommand он удаляется из persist-модели и не появляется после перезапуска.
    /// </summary>
    [Fact]
    public void DeleteTimerCommand_ShouldRemoveFromPersist_AndNotAppearAfterReload()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var appDataService = new AppDataService(settingsFile, timersFile, fs);
        var soundService = new Mock<ISoundService>().Object;
        var windowService = new Mock<IWindowService>().Object;
        var mainLogger = new Mock<ILogger<MainWindowViewModel>>().Object;
        var mainVM = new MainWindowViewModel(new Mock<ITimeService>().Object, appDataService, soundService, windowService, mainLogger);
        var trayIconManager = new Mock<TrayIconManager>(MockBehavior.Loose, new object[] { }).Object;
        var timersAndAlarmsVM = new TimersAndAlarmsViewModel(appDataService, soundService, trayIconManager);
        // Добавляем таймер
        var timerModel = new ClockWidgetApp.Models.TimerPersistModel { Duration = TimeSpan.FromSeconds(42) };
        appDataService.Data.Timers.Add(timerModel);
        appDataService.Save();
        // Удаляем через SettingsWindowViewModel
        var logger = new Mock<ILogger<SettingsWindowViewModel>>().Object;
        var settingsVM = new SettingsWindowViewModel(mainVM, appDataService, timersAndAlarmsVM, logger);
        var timerVM = timersAndAlarmsVM.TimersVm.TimerEntries[0];
        settingsVM.DeleteTimerCommand.Execute(timerVM);
        appDataService.Save();
        // Перезагружаем данные
        appDataService.Data.Timers.Clear();
        appDataService.Load();
        Assert.Empty(appDataService.Data.Timers);
    }

    /// <summary>
    /// Проверяет, что при добавлении таймера он появляется в persist-модели и сохраняется после перезапуска.
    /// </summary>
    [Fact]
    public void AddTimer_ShouldAppearInPersist_AndAfterReload()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var appDataService = new AppDataService(settingsFile, timersFile, fs);
        var timersVM = new TimersViewModel(appDataService);
        timersVM.NewTimerHours = "0";
        timersVM.NewTimerMinutes = "1";
        timersVM.NewTimerSeconds = "5";
        timersVM.AddTimerCommand.Execute(null);
        Assert.Single(appDataService.Data.Timers);
        appDataService.Save();
        appDataService.Data.Timers.Clear();
        appDataService.Load();
        Assert.Single(appDataService.Data.Timers);
        Assert.Equal(new System.TimeSpan(0, 1, 5), appDataService.Data.Timers[0].Duration);
    }

    /// <summary>
    /// Проверяет, что при добавлении таймера он неактивен (IsRunning == false) сразу после создания.
    /// </summary>
    [Fact]
    public void AddTimer_ShouldBeInactiveAfterCreate()
    {
        var appDataService = new AppDataService("settings.json", "timers.json", new InMemoryFileSystemService());
        var timersVM = new TimersViewModel(appDataService);
        timersVM.NewTimerHours = "0";
        timersVM.NewTimerMinutes = "2";
        timersVM.NewTimerSeconds = "0";
        timersVM.AddTimerCommand.Execute(null);
        Assert.Single(timersVM.TimerEntries);
        Assert.False(timersVM.TimerEntries[0].IsRunning);
    }

    /// <summary>
    /// Проверяет, что при запуске таймера он перемещается наверх списка.
    /// </summary>
    [Fact]
    public void StartTimer_ShouldMoveToTop()
    {
        var vm = CreateTimersViewModelWithMockData();
        // Добавляем три таймера
        vm.NewTimerHours = "0"; vm.NewTimerMinutes = "0"; vm.NewTimerSeconds = "10"; vm.AddTimerCommand.Execute(null);
        vm.NewTimerHours = "0"; vm.NewTimerMinutes = "0"; vm.NewTimerSeconds = "20"; vm.AddTimerCommand.Execute(null);
        vm.NewTimerHours = "0"; vm.NewTimerMinutes = "0"; vm.NewTimerSeconds = "30"; vm.AddTimerCommand.Execute(null);
        // Запускаем второй таймер (индекс 1)
        var timerToStart = vm.TimerEntries[1];
        timerToStart.StartCommand.Execute(null);
        // Проверяем, что он теперь первый
        Assert.Equal(timerToStart, vm.TimerEntries[0]);
    }

    /// <summary>
    /// Проверяет, что при добавлении нового таймера он появляется после всех запущенных.
    /// </summary>
    [Fact]
    public void AddTimer_ShouldAppearAfterRunningTimers()
    {
        var vm = CreateTimersViewModelWithMockData();
        // Добавляем и запускаем два таймера
        vm.NewTimerHours = "0"; vm.NewTimerMinutes = "0"; vm.NewTimerSeconds = "10"; vm.AddTimerCommand.Execute(null);
        vm.NewTimerHours = "0"; vm.NewTimerMinutes = "0"; vm.NewTimerSeconds = "20"; vm.AddTimerCommand.Execute(null);
        vm.TimerEntries[0].StartCommand.Execute(null);
        vm.TimerEntries[1].StartCommand.Execute(null);
        // Добавляем новый таймер
        vm.NewTimerHours = "0"; vm.NewTimerMinutes = "0"; vm.NewTimerSeconds = "30"; vm.AddTimerCommand.Execute(null);
        // Новый таймер должен быть после двух запущенных
        Assert.Equal(new TimeSpan(0,0,30), vm.TimerEntries[2].Duration);
    }

    /// <summary>
    /// Проверяет, что порядок таймеров сохраняется после перезапуска (сериализация/десериализация).
    /// </summary>
    [Fact]
    public void TimersOrder_ShouldPersistAfterReload()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var appDataService = new AppDataService(settingsFile, timersFile, fs);
        var timersVM = new TimersViewModel(appDataService);
        // Добавляем три таймера
        timersVM.NewTimerHours = "0"; timersVM.NewTimerMinutes = "0"; timersVM.NewTimerSeconds = "10"; timersVM.AddTimerCommand.Execute(null);
        timersVM.NewTimerHours = "0"; timersVM.NewTimerMinutes = "0"; timersVM.NewTimerSeconds = "20"; timersVM.AddTimerCommand.Execute(null);
        timersVM.NewTimerHours = "0"; timersVM.NewTimerMinutes = "0"; timersVM.NewTimerSeconds = "30"; timersVM.AddTimerCommand.Execute(null);
        // Запускаем последний таймер (он переместится наверх)
        timersVM.TimerEntries[2].StartCommand.Execute(null);
        // Добавляем ещё один таймер (он будет после всех запущенных)
        timersVM.NewTimerHours = "0"; timersVM.NewTimerMinutes = "0"; timersVM.NewTimerSeconds = "40"; timersVM.AddTimerCommand.Execute(null);
        // Сохраняем текущий порядок
        var expectedOrder = timersVM.TimerEntries.Select(t => t.Duration).ToList();
        // Сохраняем и перезагружаем
        appDataService.Save();
        appDataService.Data.Timers.Clear();
        appDataService.Load();
        var timersVM2 = new TimersViewModel(appDataService);
        // Проверяем, что порядок после загрузки совпадает с сохранённым
        var actualOrder = timersVM2.TimerEntries.Select(t => t.Duration).ToList();
        Assert.Equal(expectedOrder, actualOrder);
    }
} 