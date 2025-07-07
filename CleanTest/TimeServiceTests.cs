using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Threading;

namespace CleanTest;

/// <summary>
/// Тесты для TimeService: генерация тиков и переход через полночь.
/// </summary>
public class TimeServiceTests
{
    /// <summary>
    /// Проверяет, что событие TimeUpdated вызывается при изменении секунды.
    /// </summary>
    [Fact]
    public void TimeUpdated_ShouldBeRaised_OnSecondChange()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TimeService>>();
        var service = new TimeService(loggerMock.Object);
        DateTime? updatedTime = null;
        service.TimeUpdated += (s, t) => updatedTime = t;

        // Act
        service.Start();
        Thread.Sleep(1200); // ждём чуть больше секунды
        service.Stop();

        // Assert
        Assert.NotNull(updatedTime);
        Assert.True((DateTime.Now - updatedTime.Value).TotalSeconds < 2);
        service.Dispose();
    }

    /// <summary>
    /// Проверяет, что сервис корректно работает при многократном запуске и остановке.
    /// </summary>
    [Fact]
    public void StartStop_ShouldNotThrow_MultipleTimes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TimeService>>();
        var service = new TimeService(loggerMock.Object);

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            service.Start();
            Thread.Sleep(100);
            service.Stop();
        }
        service.Dispose();
    }

    /// <summary>
    /// Проверяет, что Dispose можно вызывать многократно без исключений.
    /// </summary>
    [Fact]
    public void Dispose_ShouldBeSafe_MultipleTimes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TimeService>>();
        var service = new TimeService(loggerMock.Object);

        // Act & Assert
        service.Dispose();
        service.Dispose();
    }
} 