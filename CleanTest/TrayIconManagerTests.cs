using ClockWidgetApp;
using ClockWidgetApp.Services;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;

namespace CleanTest;

/// <summary>
/// Тесты для TrayIconManager: взаимодействие с треем.
/// </summary>
public class TrayIconManagerTests
{
    /// <summary>
    /// Проверяет, что TrayIconManager корректно инициализирует и освобождает главную иконку трея.
    /// </summary>
    [Fact]
    public void InitializeAndDisposeMainTrayIcon_ShouldWorkWithoutExceptions()
    {
        // Arrange
        var timeServiceMock = new Mock<ITimeService>();
        var settingsServiceMock = new Mock<ISettingsService>();
        settingsServiceMock.SetupGet(s => s.CurrentSettings).Returns(new ClockWidgetApp.Models.WidgetSettings());
        var soundServiceMock = new Mock<ISoundService>();
        var windowServiceMock = new Mock<IWindowService>();
        var loggerVmMock = new Mock<ILogger<MainWindowViewModel>>();
        var mainViewModel = new MainWindowViewModel(timeServiceMock.Object, settingsServiceMock.Object, soundServiceMock.Object, windowServiceMock.Object, loggerVmMock.Object);
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger>();
        var trayManager = new TrayIconManager();

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

        // Act & Assert
        trayManager.InitializeMainTrayIcon(mainViewModel, serviceProviderMock.Object, loggerMock.Object);
        trayManager.UpdateMainTrayMenuItems();
        trayManager.DisposeMainTrayIcon();
        trayManager.Dispose();
    }

    /// <summary>
    /// Проверяет, что повторный вызов DisposeMainTrayIcon не вызывает исключений.
    /// </summary>
    [Fact]
    public void DisposeMainTrayIcon_ShouldBeSafe_MultipleTimes()
    {
        var trayManager = new TrayIconManager();
        trayManager.DisposeMainTrayIcon();
        trayManager.DisposeMainTrayIcon();
    }
} 