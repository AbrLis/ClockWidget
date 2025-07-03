using System.IO;
using System.Windows.Media;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Реализация ISoundHandle для управления отдельным MediaPlayer.
/// </summary>
public class SoundHandle : ISoundHandle
{
    private MediaPlayer? _player;
    public SoundHandle(MediaPlayer player)
    {
        _player = player;
    }
    public void Stop()
    {
        if (_player != null)
        {
            _player.Stop();
            _player.Close();
            _player = null;
        }
    }
}

public class SoundService : ISoundService
{
    private readonly ILogger<SoundService> _logger;
    private MediaPlayer? _player;
    private bool _isLooping = false;

    public SoundService(ILogger<SoundService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Воспроизводит аудиофайл по указанному пути. Если loop=true, воспроизводит в цикле.
    /// В случае ошибки логирует путь, loop и ThreadId.
    /// </summary>
    /// <param name="soundPath">Путь к аудиофайлу.</param>
    /// <param name="loop">Воспроизводить в цикле.</param>
    public void PlaySound(string soundPath, bool loop = false)
    {
        try
        {
            StopSound();
            bool fileExists = File.Exists(soundPath);
            _logger.LogDebug($"[SoundService] Sound file path: {soundPath}, Exists: {fileExists}");
            if (!fileExists)
            {
                _logger.LogWarning($"[SoundService] Sound file not found: {soundPath}");
                return;
            }
            _player = new MediaPlayer();
            _isLooping = loop;
            _player.Open(new Uri(soundPath));
            _player.Volume = 1.0;
            _player.MediaEnded += (s, e) =>
            {
                if (_isLooping)
                {
                    _player.Position = TimeSpan.Zero;
                    _player.Play();
                }
                else
                {
                    _player.Close();
                    _player = null;
                    _logger.LogDebug($"[SoundService] MediaPlayer closed after playback: {soundPath}");
                }
            };
            _player.MediaFailed += (s, e) =>
            {
                _logger.LogError($"[SoundService] MediaPlayer failed: {e.ErrorException}");
            };
            _player.Play();
            _logger.LogDebug($"[SoundService] Playing sound: {soundPath}, loop={loop}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SoundService] Error playing sound. Path: {Path}, Loop: {Loop}, ThreadId: {ThreadId}", soundPath, loop, System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
    }

    /// <summary>
    /// Останавливает воспроизведение звука.
    /// </summary>
    public void StopSound()
    {
        if (_player != null)
        {
            _player.Stop();
            _player.Close();
            _player = null;
            _isLooping = false;
            _logger.LogDebug("[SoundService] Sound stopped.");
        }
    }

    /// <summary>
    /// Воспроизводит аудиофайл кукушки для указанного часа.
    /// </summary>
    /// <param name="hour">Час (1-12).</param>
    public void PlayCuckooSound(int hour)
    {
        _logger.LogDebug($"[SoundService] PlayCuckooSound called, hour={hour}");
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
        _logger.LogDebug("[SoundService] PlayHalfHourChime called");
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string soundPath = Path.Combine(baseDir, "Resources", "Sounds", "halfHour.mp3");
        PlaySound(soundPath);
    }

    /// <summary>
    /// Воспроизводит аудиофайл и возвращает handle для управления этим воспроизведением.
    /// В случае ошибки логирует путь, loop и ThreadId.
    /// </summary>
    /// <param name="soundPath">Путь к аудиофайлу.</param>
    /// <param name="loop">Воспроизводить в цикле.</param>
    /// <returns>Handle для управления воспроизведением.</returns>
    public ISoundHandle PlaySoundInstance(string soundPath, bool loop = false)
    {
        try
        {
            bool fileExists = File.Exists(soundPath);
            _logger.LogDebug($"[SoundService] Sound file path: {soundPath}, Exists: {fileExists}");
            if (!fileExists)
            {
                _logger.LogWarning($"[SoundService] Sound file not found: {soundPath}");
                return new SoundHandle(new MediaPlayer()); // пустой handle
            }
            var player = new MediaPlayer();
            player.Open(new Uri(soundPath));
            player.Volume = 1.0;
            player.MediaEnded += (s, e) =>
            {
                if (loop)
                {
                    player.Position = TimeSpan.Zero;
                    player.Play();
                }
                else
                {
                    player.Close();
                }
            };
            player.MediaFailed += (s, e) =>
            {
                _logger.LogError($"[SoundService] MediaPlayer failed: {e.ErrorException}");
            };
            player.Play();
            _logger.LogDebug($"[SoundService] Playing sound instance: {soundPath}, loop={loop}");
            return new SoundHandle(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SoundService] Error playing sound instance. Path: {Path}, Loop: {Loop}, ThreadId: {ThreadId}", soundPath, loop, System.Threading.Thread.CurrentThread.ManagedThreadId);
            return new SoundHandle(new MediaPlayer());
        }
    }
} 