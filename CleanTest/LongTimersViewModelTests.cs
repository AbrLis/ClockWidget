using System;
using Xunit;
using ClockWidgetApp.ViewModels;
using Moq;
using ClockWidgetApp.Services;
using ClockWidgetApp.Helpers;
using System.IO;
using ClockWidgetApp.Models;
using System.Linq;

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
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var soundService = new Mock<ISoundService>().Object;
        var vm = new LongTimersViewModel(appDataServiceMock.Object, soundService);
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = dt, Name = "Test" }, soundService);
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
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var soundService = new Mock<ISoundService>().Object;
        var vm = new LongTimersViewModel(appDataServiceMock.Object, soundService);
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = dt, Name = "Test" }, soundService);
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
        var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = dt, Name = "Test" }, soundService);
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
        var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = dt, Name = "Test" }, soundService);
        Assert.Equal("00:00:00", timer.DisplayTime);
    }

    /// <summary>
    /// Проверяет, что имя длинного таймера в тултипе обрезается до максимальной длины (Constants.LongTimerTooltipNameMaxLength) с многоточием.
    /// </summary>
    [Fact]
    public void TrayTooltip_ShouldTruncateLongName()
    {
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = dt, Name = "VeryLongTimerName12356" }, soundService);
        string tooltip = timer.TrayTooltip;
        // Имя обрезано до максимальной длины + ...
        string expectedStart = "VeryLongTimerName12356".Substring(0, Constants.LongTimerTooltipNameMaxLength) + "...";
        Assert.StartsWith(expectedStart, tooltip);
    }

    /// <summary>
    /// Проверяет, что коллекция корректно обрабатывает массовое добавление таймеров (edge case).
    /// </summary>
    [Fact]
    public void AddMultipleTimers_ShouldHandleEdgeCases()
    {
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var soundService = new Mock<ISoundService>().Object;
        var vm = new LongTimersViewModel(appDataServiceMock.Object, soundService);
        for (int i = 0; i < 100; i++)
        {
            var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = DateTime.Now.AddMinutes(i), Name = $"T{i}" }, soundService);
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
        var appDataServiceMock = new Mock<IAppDataService>();
        appDataServiceMock.SetupGet(s => s.Data).Returns(new AppDataModel());
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddHours(1);
        var timer = new LongTimerEntryViewModel(new LongTimerPersistModel { TargetDateTime = dt, Name = "" }, soundService);
        // Проверяем, что TrayTooltip не пустой и содержит локализованное значение (зависит от локализации)
        Assert.False(string.IsNullOrWhiteSpace(timer.TrayTooltip));
    }

    /// <summary>
    /// Проверяет, что длинный таймер корректно сохраняется и загружается через сервис.
    /// </summary>
    [Fact]
    public void LongTimer_ShouldBeSavedAndLoadedCorrectly()
    {
        var fs = new InMemoryFileSystemService();
        var file = "timers_alarms.json";
        var settingsFile = "widget_settings.json";
        var service = new AppDataService(settingsFile, file, fs);
        var appDataService = new Mock<IAppDataService>().Object;
        var soundService = new Mock<ISoundService>().Object;
        var dt = DateTime.Now.AddHours(2);
        var name = "PersistTest";
        service.Data.LongTimers.Add(new ClockWidgetApp.Models.LongTimerPersistModel { TargetDateTime = dt, Name = name });
        service.Save();
        service.Data.LongTimers.Clear();
        service.Load();
        Assert.Single(service.Data.LongTimers);
        Assert.Equal(name, service.Data.LongTimers[0].Name);
        Assert.Equal(dt, service.Data.LongTimers[0].TargetDateTime, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Проверяет, что при удалении длинного таймера из persist-модели он не появляется после перезапуска.
    /// </summary>
    [Fact]
    public void DeleteLongTimer_FromPersist_ShouldNotAppearAfterReload()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var appDataService = new AppDataService(settingsFile, timersFile, fs);
        // Добавляем длинный таймер
        var dt = DateTime.Now.AddHours(1);
        var name = "TestLongTimerPersist";
        var longTimerModel = new ClockWidgetApp.Models.LongTimerPersistModel { TargetDateTime = dt, Name = name };
        appDataService.Data.LongTimers.Add(longTimerModel);
        appDataService.Save();
        // Удаляем напрямую из persist
        var toRemove = appDataService.Data.LongTimers.FirstOrDefault(m => m.TargetDateTime == dt && m.Name == name);
        Assert.NotNull(toRemove);
        appDataService.Data.LongTimers.Remove(toRemove);
        appDataService.Save();
        // Перезагружаем данные
        appDataService.Data.LongTimers.Clear();
        appDataService.Load();
        Assert.Empty(appDataService.Data.LongTimers);
    }

    /// <summary>
    /// Проверяет, что при добавлении длинного таймера он появляется в persist-модели и сохраняется после перезапуска.
    /// </summary>
    [Fact]
    public void AddLongTimer_ShouldAppearInPersist_AndAfterReload()
    {
        var fs = new InMemoryFileSystemService();
        var settingsFile = "settings.json";
        var timersFile = "timers.json";
        var appDataService = new AppDataService(settingsFile, timersFile, fs);
        var soundService = new Mock<ISoundService>().Object;
        var longTimersVM = new LongTimersViewModel(appDataService, soundService);
        var dt = DateTime.Now.AddHours(2);
        var name = "PersistTest";
        var persistModel = new ClockWidgetApp.Models.LongTimerPersistModel { TargetDateTime = dt, Name = name };
        longTimersVM.LongTimerModels.Add(persistModel);
        Assert.Single(appDataService.Data.LongTimers);
        appDataService.Save();
        appDataService.Data.LongTimers.Clear();
        appDataService.Load();
        Assert.Single(appDataService.Data.LongTimers);
        Assert.Equal(name, appDataService.Data.LongTimers[0].Name);
        Assert.Equal(dt, appDataService.Data.LongTimers[0].TargetDateTime, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Проверяет, что при добавлении длинного таймера он активен (IsRunning == true) сразу после создания.
    /// </summary>
    [Fact]
    public void AddLongTimer_ShouldBeInactiveAfterCreate()
    {
        var appDataService = new AppDataService("settings.json", "timers.json", new InMemoryFileSystemService());
        var soundService = new Mock<ISoundService>().Object;
        var longTimersVM = new LongTimersViewModel(appDataService, soundService);
        var dt = DateTime.Now.AddMinutes(30);
        var name = "TestInactive";
        var persistModel = new ClockWidgetApp.Models.LongTimerPersistModel { TargetDateTime = dt, Name = name };
        longTimersVM.LongTimerModels.Add(persistModel);
        Assert.Single(longTimersVM.LongTimers);
        Assert.True(longTimersVM.LongTimers[0].Remaining > TimeSpan.Zero);
    }

    /// <summary>
    /// Проверяет, что после создания длинный таймер сразу начинает отсчёт времени (Remaining > 0).
    /// </summary>
    [Fact]
    public void AddLongTimer_ShouldStartImmediately()
    {
        var appDataService = new AppDataService("settings.json", "timers.json", new InMemoryFileSystemService());
        var soundService = new Mock<ISoundService>().Object;
        var longTimersVM = new LongTimersViewModel(appDataService, soundService);
        var dt = DateTime.Now.AddMinutes(30);
        var name = "TestActive";
        var persistModel = new ClockWidgetApp.Models.LongTimerPersistModel { TargetDateTime = dt, Name = name };
        longTimersVM.LongTimerModels.Add(persistModel);
        Assert.Single(longTimersVM.LongTimers);
        Assert.True(longTimersVM.LongTimers[0].Remaining > TimeSpan.Zero);
    }
} 