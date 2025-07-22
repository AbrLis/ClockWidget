namespace ClockWidgetApp.Services;

/// <summary>
/// Интерфейс для сервиса воспроизведения звука.
/// </summary>
public interface ISoundService
{
    /// <summary>
    /// Воспроизводит аудиофайл по указанному пути. Если loop=true, воспроизводит в цикле.
    /// </summary>
    /// <param name="soundPath">Путь к аудиофайлу.</param>
    /// <param name="loop">Воспроизводить в цикле.</param>
    void PlaySound(string soundPath, bool loop = false);

    /// <summary>
    /// Воспроизводит аудиофайл кукушки для указанного часа.
    /// </summary>
    /// <param name="hour">Час (1-12).</param>
    void PlayCuckooSound(int hour);

    /// <summary>
    /// Воспроизводит аудиофайл сигнала для половины часа (например, 12:30, 1:30 и т.д.).
    /// </summary>
    void PlayHalfHourChime();

    /// <summary>
    /// Останавливает воспроизведение звука.
    /// </summary>
    void StopSound();

    /// <summary>
    /// Воспроизводит аудиофайл и возвращает handle для управления этим воспроизведением.
    /// </summary>
    /// <param name="soundPath">Путь к аудиофайлу.</param>
    /// <param name="loop">Воспроизводить в цикле.</param>
    /// <returns>Handle для управления воспроизведением.</returns>
    ISoundHandle PlaySoundInstance(string soundPath, bool loop = false);
}

/// <summary>
/// Интерфейс для управления отдельным воспроизведением звука.
/// </summary>
public interface ISoundHandle
{
    /// <summary>
    /// Останавливает воспроизведение звука.
    /// </summary>
    void Stop();
}

/// <summary>
/// Заглушка для ISoundHandle, не выполняющая никаких действий (Null Object Pattern).
/// </summary>
public class NullSoundHandle : ISoundHandle
{
    /// <summary>
    /// Не выполняет никаких действий.
    /// </summary>
    public void Stop() { /* ничего не делаем */ }
}