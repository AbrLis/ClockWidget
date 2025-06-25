namespace ClockWidgetApp.Services;

/// <summary>
/// Интерфейс для сервиса воспроизведения звука.
/// </summary>
public interface ISoundService
{
    /// <summary>
    /// Воспроизводит аудиофайл по указанному пути.
    /// </summary>
    /// <param name="soundPath">Путь к аудиофайлу.</param>
    void PlaySound(string soundPath);

    /// <summary>
    /// Воспроизводит аудиофайл кукушки для указанного часа.
    /// </summary>
    /// <param name="hour">Час (1-12).</param>
    void PlayCuckooSound(int hour);
} 