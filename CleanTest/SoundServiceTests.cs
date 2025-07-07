using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;

namespace CleanTest;

/// <summary>
/// Тесты для SoundService: проверка воспроизведения звуковых файлов.
/// </summary>
public class SoundServiceTests
{
    /// <summary>
    /// Проверяет, что при попытке воспроизвести несуществующий файл не выбрасывается исключение.
    /// </summary>
    [Fact]
    public void PlaySound_ShouldNotThrow_WhenFileDoesNotExist()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SoundService>>();
        var service = new SoundService(loggerMock.Object);
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp3");

        // Act & Assert
        var ex = Record.Exception(() => service.PlaySound(nonExistentFile));
        Assert.Null(ex);
    }

    /// <summary>
    /// Проверяет, что при воспроизведении существующего файла не выбрасывается исключение.
    /// </summary>
    [Fact]
    public void PlaySound_ShouldNotThrow_WhenFileExists()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SoundService>>();
        var service = new SoundService(loggerMock.Object);
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp3");
        File.WriteAllText(tempFile, "test"); // создаём фиктивный файл

        // Act & Assert
        var ex = Record.Exception(() => service.PlaySound(tempFile));
        Assert.Null(ex);

        // Clean up
        File.Delete(tempFile);
    }
} 