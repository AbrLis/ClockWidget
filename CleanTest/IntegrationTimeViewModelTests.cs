using ClockWidgetApp;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;

namespace CleanTest;

/// <summary>
/// Интеграционный тест: TimeService + MainWindowViewModel.
/// </summary>
public class IntegrationTimeViewModelTests
{
    /// <summary>
    /// Проверяет, что при обновлении времени через TimeService свойство TimeText в ViewModel обновляется.
    /// </summary>
    [Fact]
    public void TimeUpdated_ShouldUpdateTimeTextInViewModel()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TimeService>>();
        var timeService = new TimeService(loggerMock.Object);
        var settingsServiceMock = new Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings { ShowSeconds = true });
        settingsServiceMock.Setup(s => s.UpdateSettings(It.IsAny<Action<ClockWidgetApp.Models.WidgetSettings>>()))
            .Callback<Action<ClockWidgetApp.Models.WidgetSettings>>(a => a(settingsServiceMock.Object.CurrentSettings));
        var soundServiceMock = new Mock<ISoundService>();
        soundServiceMock.Setup(x => x.PlayCuckooSound(It.IsAny<int>()));
        soundServiceMock.Setup(x => x.PlayHalfHourChime());
        var windowServiceMock = new Mock<IWindowService>();
        var vmLoggerMock = new Mock<ILogger<MainWindowViewModel>>();
        var viewModel = new MainWindowViewModel(timeService, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, vmLoggerMock.Object);
        var analogVmMock = new Mock<AnalogClockViewModel>(timeService, settingsServiceMock.Object, viewModel, new Mock<ILogger<AnalogClockViewModel>>().Object, windowServiceMock.Object);
        var testTime = new DateTime(2024, 1, 2, 3, 4, 5);

        // Act
        // Имитация события TimeUpdated через вызов приватного метода
        typeof(MainWindowViewModel).GetMethod("OnTimeUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(viewModel, new object?[] { timeService, testTime });

        // Assert
        Assert.Contains("03:04:05", viewModel.TimeText);
        viewModel.Dispose();
    }
} 