using ClockWidgetApp.ViewModels;
using Xunit;
using System;
using Moq;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;
using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;

namespace CleanTest;

/// <summary>
/// Тесты для TimersViewModel: добавление, удаление, запуск, остановка таймера, валидация времени.
/// </summary>
public class TimersViewModelTests
{
    /// <summary>
    /// Проверяет, что при валидном вводе таймер добавляется в коллекцию.
    /// </summary>
    [Fact]
    public void AddTimerCommand_ShouldAddTimer_WhenInputIsValid()
    {
        // Arrange
        var vm = new TimersViewModel();
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
        var vm = new TimersViewModel();
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
        var timer = new TimerEntryViewModel(TimeSpan.FromSeconds(5));
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
        var vm = new TimersViewModel();
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
        var timersVM = new TimersViewModel();
        timersVM.NewTimerHours = "0";
        timersVM.NewTimerMinutes = "0";
        timersVM.NewTimerSeconds = "10";
        timersVM.AddTimerCommand.Execute(null);
        var timer = timersVM.Timers[0];
        var timeService = new Mock<ITimeService>().Object;
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var appDataService = appDataServiceMock.Object;
        var soundService = new Mock<ISoundService>().Object;
        var windowService = new Mock<IWindowService>().Object;
        var mainLogger = new Mock<ILogger<MainWindowViewModel>>().Object;
        var mainVM = new MainWindowViewModel(timeService, appDataService, soundService, windowService, mainLogger);
        var trayIconManager = new Mock<TrayIconManager>(MockBehavior.Loose, new object[] { }).Object;
        var timersAndAlarmsVM = new TimersAndAlarmsViewModel(appDataService, soundService, trayIconManager);
        // Копируем таймеры в тестовый экземпляр
        foreach (var t in timersVM.Timers)
            timersAndAlarmsVM.TimersVm.Timers.Add(t);
        var logger = new Mock<ILogger<SettingsWindowViewModel>>().Object;
        var settingsVM = new SettingsWindowViewModel(mainVM, appDataService, timersAndAlarmsVM, logger);
        settingsVM.DeleteTimerCommand.Execute(timersAndAlarmsVM.TimersVm.Timers[0]);
        Assert.Empty(timersAndAlarmsVM.TimersVm.Timers);
    }
} 