using System;
using System.IO;
using System.Windows.Media;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

public class SoundService : ISoundService
{
    private readonly ILogger<SoundService> _logger;

    public SoundService(ILogger<SoundService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Воспроизводит аудиофайл по указанному пути.
    /// </summary>
    /// <param name="soundPath">Путь к аудиофайлу.</param>
    public void PlaySound(string soundPath)
    {
        try
        {
            bool fileExists = File.Exists(soundPath);
            _logger.LogInformation($"[SoundService] Sound file path: {soundPath}, Exists: {fileExists}");
            if (!fileExists)
            {
                _logger.LogWarning($"[SoundService] Sound file not found: {soundPath}");
                return;
            }
            var player = new MediaPlayer();
            player.Open(new Uri(soundPath));
            player.Volume = 1.0;
            player.MediaEnded += (s, e) =>
            {
                player.Close();
                player.Dispatcher?.InvokeAsync(() => player = null);
                _logger.LogInformation($"[SoundService] MediaPlayer closed after playback: {soundPath}");
            };
            player.MediaFailed += (s, e) =>
            {
                _logger.LogError($"[SoundService] MediaPlayer failed: {e.ErrorException}");
            };
            player.Play();
            _logger.LogInformation($"[SoundService] Playing sound: {soundPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SoundService] Error playing sound");
        }
    }

    /// <summary>
    /// Воспроизводит аудиофайл кукушки для указанного часа.
    /// </summary>
    /// <param name="hour">Час (1-12).</param>
    public void PlayCuckooSound(int hour)
    {
        _logger.LogInformation($"[SoundService] PlayCuckooSound called, hour={hour}");
        int soundHour = hour % 12;
        if (soundHour == 0) soundHour = 12;
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string soundPath = Path.Combine(baseDir, "Resources", "Sounds", $"{soundHour}.mp3");
        PlaySound(soundPath);
    }

    /// <summary>
    /// Воспроизводит аудиофайл сигнала для половины часа (например, 12:30, 1:30 и т.д.).
    /// </summary>
    public void PlayHalfHourChime()
    {
        _logger.LogInformation("[SoundService] PlayHalfHourChime called");
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string soundPath = Path.Combine(baseDir, "Resources", "Sounds", "halfHour.mp3");
        PlaySound(soundPath);
    }
} 