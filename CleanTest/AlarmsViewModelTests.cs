using ClockWidgetApp.ViewModels;
using Xunit;
using System;

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
        // Arrange
        var vm = new AlarmsViewModel();
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
        var vm = new AlarmsViewModel();
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
        var vm = new AlarmsViewModel();
        vm.NewAlarmHours = "6";
        vm.NewAlarmMinutes = "0";
        vm.AddAlarmCommand.Execute(null);
        var alarm = vm.Alarms[0];
        alarm.DeleteCommand.Execute(null);
        Assert.Empty(vm.Alarms);
    }

    /// <summary>
    /// Проверяет редактирование и применение изменений будильника.
    /// </summary>
    [Fact]
    public void EditAndApplyEditAlarm_ShouldUpdateAlarmTime()
    {
        var vm = new AlarmsViewModel();
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
        var vm = new AlarmsViewModel();
        vm.NewAlarmHours = "25";
        vm.NewAlarmMinutes = "-5";
        vm.CorrectAlarmTime();
        Assert.Equal("23", vm.NewAlarmHours);
        Assert.Equal("0", vm.NewAlarmMinutes);
    }
} 