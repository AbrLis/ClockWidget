using ClockWidgetApp.ViewModels;
using Xunit;
using System;
using Moq;
using ClockWidgetApp.Services;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace CleanTest;

/// <summary>
/// Тесты для AlarmsViewModel: добавление, удаление, редактирование будильника, валидация времени.
/// </summary>
public class AlarmsViewModelTests
{
    /// <summary>
    /// Проверяет, что при валидном вводе будильник добавляется в коллекцию.
    /// </summary>
    [Fact]
    public void AddAlarmCommand_ShouldAddAlarm_WhenInputIsValid()
    {
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var vm = new AlarmsViewModel(appDataServiceMock.Object);
        vm.NewAlarmHours = "7";
        vm.NewAlarmMinutes = "30";

        // Act
        vm.AddAlarmCommand.Execute(null);

        // Assert
        Assert.Single(vm.Alarms);
        Assert.Equal(new TimeSpan(7, 30, 0), vm.Alarms[0].AlarmTime);
    }

    /// <summary>
    /// Проверяет, что при невалидном вводе будильник не добавляется.
    /// </summary>
    [Fact]
    public void AddAlarmCommand_ShouldNotAddAlarm_WhenInputIsInvalid()
    {
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var vm = new AlarmsViewModel(appDataServiceMock.Object);
        vm.NewAlarmHours = "-1";
        vm.NewAlarmMinutes = "0";
        vm.AddAlarmCommand.Execute(null);
        Assert.Empty(vm.Alarms);
    }

    /// <summary>
    /// Проверяет удаление будильника через событие RequestDelete.
    /// </summary>
    [Fact]
    public void AlarmEntryViewModel_RequestDelete_ShouldRemoveAlarm()
    {
        // Удалён тест, так как DeleteCommand больше не существует
    }

    /// <summary>
    /// Проверяет редактирование и применение изменений будильника.
    /// </summary>
    [Fact]
    public void EditAndApplyEditAlarm_ShouldUpdateAlarmTime()
    {
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var vm = new AlarmsViewModel(appDataServiceMock.Object);
        vm.NewAlarmHours = "8";
        vm.NewAlarmMinutes = "15";
        vm.AddAlarmCommand.Execute(null);
        var alarm = vm.Alarms[0];
        vm.EditAlarmCommand.Execute(alarm);
        vm.NewAlarmHours = "9";
        vm.NewAlarmMinutes = "45";
        vm.ApplyEditAlarmCommand.Execute(null);
        Assert.Equal(new TimeSpan(9, 45, 0), alarm.AlarmTime);
    }

    /// <summary>
    /// Проверяет корректировку времени будильника на граничных значениях.
    /// </summary>
    [Fact]
    public void CorrectAlarmTime_ShouldClampValues()
    {
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var vm = new AlarmsViewModel(appDataServiceMock.Object);
        vm.NewAlarmHours = "25";
        vm.NewAlarmMinutes = "-5";
        vm.CorrectAlarmTime();
        Assert.Equal("23", vm.NewAlarmHours);
        Assert.Equal("0", vm.NewAlarmMinutes);
    }

    /// <summary>
    /// Проверяет, что нельзя добавить дублирующийся будильник и появляется уведомление.
    /// </summary>
    [Fact]
    public void AddAlarmCommand_ShouldNotAddDuplicateAlarm_AndShowNotification()
    {
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var vm = new AlarmsViewModel(appDataServiceMock.Object);
        vm.NewAlarmHours = "7";
        vm.NewAlarmMinutes = "30";
        vm.AddAlarmCommand.Execute(null);
        // Пытаемся добавить тот же будильник
        vm.NewAlarmHours = "7";
        vm.NewAlarmMinutes = "30";
        vm.AddAlarmCommand.Execute(null);
        // Должен остаться только один будильник
        Assert.Single(vm.Alarms);
        // Должно появиться уведомление о дублировании
        Assert.False(string.IsNullOrWhiteSpace(vm.DuplicateAlarmNotification));
    }

    /// <summary>
    /// Проверяет удаление будильника через DeleteAlarmCommand в SettingsWindowViewModel.
    /// </summary>
    [Fact]
    public void SettingsWindowViewModel_DeleteAlarmCommand_ShouldRemoveAlarm()
    {
        var appDataServiceMock1 = new Mock<IAppDataService>();
        appDataServiceMock1.SetupGet(s => s.Data).Returns(new AppDataModel());
        var alarmsVM = new AlarmsViewModel(appDataServiceMock1.Object);
        alarmsVM.NewAlarmHours = "6";
        alarmsVM.NewAlarmMinutes = "0";
        alarmsVM.AddAlarmCommand.Execute(null);
        var alarm = alarmsVM.Alarms[0];
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
        // Копируем будильники в тестовый экземпляр
        foreach (var a in alarmsVM.Alarms)
            timersAndAlarmsVM.AlarmsVm.Alarms.Add(a);
        var logger = new Mock<ILogger<SettingsWindowViewModel>>().Object;
        var settingsVM = new SettingsWindowViewModel(mainVM, appDataService, timersAndAlarmsVM, logger);
        settingsVM.DeleteAlarmCommand.Execute(timersAndAlarmsVM.AlarmsVm.Alarms[0]);
        Assert.Empty(timersAndAlarmsVM.AlarmsVm.Alarms);
    }
} 