using ClockWidgetApp;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;

namespace CleanTest;

/// <summary>
/// Тесты для SettingsWindowViewModel: применение/сброс настроек, реакция на смену языка.
/// </summary>
public class SettingsWindowViewModelTests
{
    /// <summary>
    /// Проверяет, что изменение BackgroundOpacity через SettingsWindowViewModel обновляет MainWindowViewModel.
    /// </summary>
    [Fact]
    public void BackgroundOpacity_Set_ShouldUpdateMainViewModel()
    {
        // Arrange
        var timeServiceMock = new Moq.Mock<ITimeService>();
        var settingsServiceMock = new Moq.Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings());
        settingsServiceMock.Setup(s => s.UpdateSettings(It.IsAny<Action<WidgetSettings>>()))
            .Callback<Action<WidgetSettings>>(a => a(settingsServiceMock.Object.CurrentSettings));
        var soundServiceMock = new Moq.Mock<ISoundService>();
        soundServiceMock.Setup(x => x.PlayCuckooSound(It.IsAny<int>()));
        soundServiceMock.Setup(x => x.PlayHalfHourChime());
        var windowServiceMock = new Moq.Mock<IWindowService>();
        var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<MainWindowViewModel>>();
        var mainVm = new MainWindowViewModel(timeServiceMock.Object, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, loggerMock.Object);
        windowServiceMock.Setup(ws => ws.GetAnalogClockWindow()).Returns((ClockWidgetApp.AnalogClockWindow?)null);
        windowServiceMock.Setup(ws => ws.GetMainWindow()).Returns((ClockWidgetApp.MainWindow?)null);
        windowServiceMock.Setup(ws => ws.SetMainWindowTopmost(It.IsAny<bool>()));
        windowServiceMock.Setup(ws => ws.HideMainWindow());
        windowServiceMock.Setup(ws => ws.GetMainWindowLeft()).Returns(0.0);
        windowServiceMock.Setup(ws => ws.GetMainWindowTop()).Returns(0.0);
        windowServiceMock.Setup(ws => ws.SetMainWindowLeft(It.IsAny<double>()));
        windowServiceMock.Setup(ws => ws.SetMainWindowTop(It.IsAny<double>()));
        var loggerSettingsMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<SettingsWindowViewModel>>();
        var vm = new SettingsWindowViewModel(mainVm, loggerSettingsMock.Object);

        // Act
        vm.BackgroundOpacity = 0.8;

        // Assert
        Assert.Equal(0.8, mainVm.BackgroundOpacity, 5);
    }

    /// <summary>
    /// Проверяет, что смена языка обновляет свойство Language.
    /// </summary>
    [Fact]
    public void Language_Set_ShouldUpdateProperty()
    {
        var timeServiceMock = new Moq.Mock<ITimeService>();
        var settingsServiceMock = new Moq.Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings());
        var soundServiceMock = new Moq.Mock<ISoundService>();
        var windowServiceMock = new Moq.Mock<IWindowService>();
        var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<MainWindowViewModel>>();
        var mainVm = new MainWindowViewModel(timeServiceMock.Object, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, loggerMock.Object);
        windowServiceMock.Setup(ws => ws.GetAnalogClockWindow()).Returns((ClockWidgetApp.AnalogClockWindow?)null);
        var loggerSettingsMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<SettingsWindowViewModel>>();
        var vm = new SettingsWindowViewModel(mainVm, loggerSettingsMock.Object);
        vm.Language = "ru";
        Assert.Equal("ru", vm.Language);
    }
} 