using System;
using System.Linq;
using ClockWidgetApp.ViewModels;
using ClockWidgetApp.Services;
using Moq;
using Xunit;

namespace CleanTest;

public class LongTimersViewModelTests
{
    [Fact]
    public void AddTimer_ShouldAddValidLongTimer()
    {
        var soundServiceMock = new Mock<ISoundService>();
        var vm = new LongTimersViewModel(soundServiceMock.Object);
        var now = DateTime.Now.AddMinutes(1);
        vm.NewYear = now.Year.ToString();
        vm.NewMonth = now.Month.ToString();
        vm.NewDay = now.Day.ToString();
        vm.NewHour = now.Hour.ToString();
        vm.NewMinute = now.Minute.ToString();
        vm.NewSecond = now.Second.ToString();
        Assert.True(vm.IsNewTimerValid);
        vm.AddTimerCommand.Execute(null);
        Assert.Single(vm.LongTimers);
        Assert.Equal(now.Year, vm.LongTimers[0].TargetDateTime.Year);
    }

    [Fact]
    public void DeleteTimer_ShouldRemoveLongTimer()
    {
        var soundServiceMock = new Mock<ISoundService>();
        var vm = new LongTimersViewModel(soundServiceMock.Object);
        var now = DateTime.Now.AddMinutes(2);
        vm.NewYear = now.Year.ToString();
        vm.NewMonth = now.Month.ToString();
        vm.NewDay = now.Day.ToString();
        vm.NewHour = now.Hour.ToString();
        vm.NewMinute = now.Minute.ToString();
        vm.NewSecond = now.Second.ToString();
        vm.AddTimerCommand.Execute(null);
        var timer = vm.LongTimers.First();
        timer.RequestDelete?.Invoke(timer);
        Assert.Empty(vm.LongTimers);
    }

    [Fact]
    public void StartStopTimer_ShouldChangeIsRunning()
    {
        var soundServiceMock = new Mock<ISoundService>();
        var now = DateTime.Now.AddMinutes(3);
        var timer = new LongTimerEntryViewModel(now, soundServiceMock.Object);
        Assert.False(timer.IsRunning);
        timer.Start();
        Assert.True(timer.IsRunning);
        timer.Stop();
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Remaining_ShouldBeCorrect()
    {
        var soundServiceMock = new Mock<ISoundService>();
        var now = DateTime.Now.AddMinutes(5);
        var timer = new LongTimerEntryViewModel(now, soundServiceMock.Object);
        var diff = timer.Remaining.TotalSeconds - (now - DateTime.Now).TotalSeconds;
        Assert.InRange(diff, -2, 2); // допускаем небольшую погрешность
    }

    [Fact]
    public void Signal_ShouldBePlayedOnFinish()
    {
        var soundServiceMock = new Mock<ISoundService>();
        var now = DateTime.Now.AddSeconds(1);
        var timer = new LongTimerEntryViewModel(now, soundServiceMock.Object);
        timer.Start();
        System.Threading.Thread.Sleep(1500);
        soundServiceMock.Verify(s => s.PlaySoundInstance(It.IsAny<string>(), true), Times.AtLeastOnce);
    }
} 