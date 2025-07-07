using ClockWidgetApp;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;

namespace CleanTest;

/// <summary>
/// Тесты для MainWindowViewModel: команды, реакция на события, применение настроек.
/// </summary>
public class MainWindowViewModelTests
{
    /// <summary>
    /// Проверяет, что команда открытия окна настроек вызывает соответствующий метод.
    /// </summary>
    [Fact]
    public void OpenSettingsCommand_ShouldSetIsSettingsWindowOpen()
    {
        // Arrange
        var timeServiceMock = new Mock<ITimeService>();
        var settingsServiceMock = new Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings());
        settingsServiceMock.Setup(s => s.UpdateSettings(It.IsAny<Action<ClockWidgetApp.Models.WidgetSettings>>()))
            .Callback<Action<ClockWidgetApp.Models.WidgetSettings>>(a => a(settingsServiceMock.Object.CurrentSettings));
        var soundServiceMock = new Mock<ISoundService>();
        var windowServiceMock = new Mock<IWindowService>();
        var loggerMock = new Mock<ILogger<MainWindowViewModel>>();
        var viewModel = new MainWindowViewModel(timeServiceMock.Object, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, loggerMock.Object);
        var analogVmMock = new Mock<AnalogClockViewModel>(timeServiceMock.Object, settingsServiceMock.Object, viewModel, new Mock<ILogger<AnalogClockViewModel>>().Object, windowServiceMock.Object);
        windowServiceMock.Setup(ws => ws.GetAnalogClockWindow()).Returns((ClockWidgetApp.AnalogClockWindow?)null);
        windowServiceMock.Setup(ws => ws.GetMainWindow()).Returns((ClockWidgetApp.MainWindow?)null);
        windowServiceMock.Setup(ws => ws.SetMainWindowTopmost(It.IsAny<bool>()));
        windowServiceMock.Setup(ws => ws.HideMainWindow());
        windowServiceMock.Setup(ws => ws.GetMainWindowLeft()).Returns(0.0);
        windowServiceMock.Setup(ws => ws.GetMainWindowTop()).Returns(0.0);
        windowServiceMock.Setup(ws => ws.SetMainWindowLeft(It.IsAny<double>()));
        windowServiceMock.Setup(ws => ws.SetMainWindowTop(It.IsAny<double>()));
        soundServiceMock.Setup(x => x.PlayCuckooSound(It.IsAny<int>()));
        soundServiceMock.Setup(x => x.PlayHalfHourChime());

        // Act
        viewModel.OpenSettingsCommand.Execute(null);

        // Assert
        Assert.True(viewModel.IsSettingsWindowOpen);
        viewModel.Dispose();
    }

    /// <summary>
    /// Проверяет, что при обновлении времени через событие TimeUpdated свойство TimeText обновляется.
    /// </summary>
    [Fact]
    public void TimeUpdated_ShouldUpdateTimeText()
    {
        // Arrange
        var timeServiceMock = new Mock<ITimeService>();
        var settingsServiceMock = new Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings { ShowSeconds = true });
        var soundServiceMock = new Mock<ISoundService>();
        var windowServiceMock = new Mock<IWindowService>();
        var loggerMock = new Mock<ILogger<MainWindowViewModel>>();
        var viewModel = new MainWindowViewModel(timeServiceMock.Object, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, loggerMock.Object);
        var analogVmMock = new Mock<AnalogClockViewModel>(timeServiceMock.Object, settingsServiceMock.Object, viewModel, new Mock<ILogger<AnalogClockViewModel>>().Object, windowServiceMock.Object);
        windowServiceMock.Setup(ws => ws.GetAnalogClockWindow()).Returns((ClockWidgetApp.AnalogClockWindow?)null);
        var testTime = new DateTime(2024, 1, 2, 3, 4, 5);
        // Вызов приватного метода через рефлексию
        typeof(MainWindowViewModel).GetMethod("OnTimeUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(viewModel, new object?[] { null, testTime });

        // Assert
        Assert.Contains("03:04:05", viewModel.TimeText);
        viewModel.Dispose();
    }

    /// <summary>
    /// Проверяет, что при изменении свойства BackgroundOpacity оно обновляется и сохраняется в сервисе.
    /// </summary>
    [Fact]
    public void BackgroundOpacity_Set_ShouldUpdateAndSave()
    {
        // Arrange
        var timeServiceMock = new Mock<ITimeService>();
        var settingsServiceMock = new Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings());
        var soundServiceMock = new Mock<ISoundService>();
        var windowServiceMock = new Mock<IWindowService>();
        var loggerMock = new Mock<ILogger<MainWindowViewModel>>();
        var viewModel = new MainWindowViewModel(timeServiceMock.Object, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, loggerMock.Object);
        var analogVmMock = new Mock<AnalogClockViewModel>(timeServiceMock.Object, settingsServiceMock.Object, viewModel, new Mock<ILogger<AnalogClockViewModel>>().Object, windowServiceMock.Object);
        windowServiceMock.Setup(ws => ws.GetAnalogClockWindow()).Returns((ClockWidgetApp.AnalogClockWindow?)null);
        double newOpacity = 0.7;

        // Act
        viewModel.BackgroundOpacity = newOpacity;

        // Assert
        Assert.Equal(newOpacity, viewModel.BackgroundOpacity, 5);
        settingsServiceMock.Verify(s => s.UpdateSettings(It.IsAny<Action<ClockWidgetApp.Models.WidgetSettings>>()), Times.AtLeastOnce());
        viewModel.Dispose();
    }
} 