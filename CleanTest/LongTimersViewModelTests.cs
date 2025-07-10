using System;
using Xunit;
using ClockWidgetApp.ViewModels;
using Moq;
using ClockWidgetApp.Services;

namespace CleanTest;

/// <summary>
/// Unit-тесты для проверки основного и граничного функционала длинных таймеров.
/// </summary>
public class LongTimersViewModelTests
{
    /// <summary>
    /// Проверяет, что добавленный длинный таймер появляется в коллекции.
    /// </summary>
    [Fact]
    public void AddLongTimer_ShouldAppearInCollection()
    {
        var soundService = new Mock<ISoundService>().Object;
        var vm = new LongTimersViewModel(soundService);
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(dt, soundService, "Test");
        vm.LongTimers.Add(timer);
        Assert.Single(vm.LongTimers);
        Assert.Equal("Test", vm.LongTimers[0].Name);
    }

    /// <summary>
    /// Проверяет, что удалённый длинный таймер исчезает из коллекции.
    /// </summary>
    [Fact]
    public void RemoveLongTimer_ShouldDisappearFromCollection()
    {
        var soundService = new Mock<ISoundService>().Object;
        var vm = new LongTimersViewModel(soundService);
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(dt, soundService, "Test");
        vm.LongTimers.Add(timer);
        vm.LongTimers.Remove(timer);
        Assert.Empty(vm.LongTimers);
    }

    /// <summary>
    /// Проверяет, что сброс таймера возвращает TargetDateTime к исходному значению.
    /// </summary>
    [Fact]
    public void ResetLongTimer_ShouldRestoreInitialTargetDateTime()
    {
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(dt, soundService, "Test");
        timer.TargetDateTime = dt.AddMinutes(10);
        timer.Reset();
        Assert.Equal(dt, timer.TargetDateTime, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Проверяет, что DisplayTime возвращает "00:00:00" для истёкшего таймера.
    /// </summary>
    [Fact]
    public void DisplayTime_ShouldBeZero_WhenExpired()
    {
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddSeconds(-10); // Уже истёк
        var timer = new LongTimerEntryViewModel(dt, soundService, "Test");
        Assert.Equal("00:00:00", timer.DisplayTime);
    }

    /// <summary>
    /// Проверяет, что имя длинного таймера в тултипе обрезается до 10 символов с многоточием.
    /// </summary>
    [Fact]
    public void TrayTooltip_ShouldTruncateLongName()
    {
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(dt, soundService, "VeryLongTimerName123");
        string tooltip = timer.TrayTooltip;
        Assert.StartsWith("VeryLongTi...", tooltip); // Имя обрезано до 10 символов + ...
    }

    /// <summary>
    /// Проверяет, что коллекция корректно обрабатывает массовое добавление таймеров (edge case).
    /// </summary>
    [Fact]
    public void AddMultipleTimers_ShouldHandleEdgeCases()
    {
        var soundService = new Mock<ISoundService>().Object;
        var vm = new LongTimersViewModel(soundService);
        for (int i = 0; i < 100; i++)
        {
            var timer = new LongTimerEntryViewModel(DateTime.Now.AddMinutes(i), soundService, $"T{i}");
            vm.LongTimers.Add(timer);
        }
        Assert.Equal(100, vm.LongTimers.Count);
    }

    /// <summary>
    /// Проверяет, что при пустом имени в тултипе используется локализованная подпись "(Без имени)" или аналогичная.
    /// </summary>
    [Fact]
    public void AddTimer_WithEmptyName_ShouldUseNoNameLabel()
    {
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(dt, soundService, "");
        // Проверяем, что TrayTooltip не пустой и содержит локализованное значение (зависит от локализации)
        Assert.False(string.IsNullOrWhiteSpace(timer.TrayTooltip));
    }

    /// <summary>
    /// Проверяет, что длинный таймер корректно сохраняется и загружается через сервис.
    /// </summary>
    [Fact]
    public void LongTimer_ShouldBeSavedAndLoadedCorrectly()
    {
        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        System.IO.Directory.CreateDirectory(tempDir);
        var file = System.IO.Path.Combine(tempDir, "timers_alarms.json");
        var service = new ClockWidgetApp.Services.TimersAndAlarmsPersistenceService(file);
        var model = new ClockWidgetApp.Models.TimersAndAlarmsPersistModel();
        var dt = DateTime.Now.AddHours(2);
        var name = "PersistTest";
        model.LongTimers.Add(new ClockWidgetApp.Models.LongTimerPersistModel { TargetDateTime = dt, Name = name });
        service.Save(model);
        var loaded = service.Load();
        Assert.NotNull(loaded);
        Assert.Single(loaded.LongTimers);
        Assert.Equal(name, loaded.LongTimers[0].Name);
        Assert.Equal(dt, loaded.LongTimers[0].TargetDateTime, TimeSpan.FromSeconds(1));
        System.IO.Directory.Delete(tempDir, true);
    }
} 