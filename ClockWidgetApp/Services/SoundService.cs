using System.IO;
using System.Windows.Media;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Реализация ISoundHandle для управления отдельным MediaPlayer.
/// </summary>
public class SoundHandle : ISoundHandle
{
    /// <summary>MediaPlayer для воспроизведения звука.</summary>
    private MediaPlayer? _player;
    public SoundHandle(MediaPlayer player)
    {
        _player = player;
    }
    /// <summary>
    /// Останавливает воспроизведение и освобождает ресурсы.
    /// </summary>
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

/// <summary>
/// Сервис для воспроизведения звуковых файлов.
/// </summary>
public class SoundService : ISoundService
{
    #region Private fields
    private readonly ILogger<SoundService> _logger;
    private MediaPlayer? _player;

    #endregion

    #region Constructors
    public SoundService(ILogger<SoundService> logger)
    {
        _logger = logger;
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Создаёт и запускает MediaPlayer для воспроизведения звука.
    /// </summary>
    private MediaPlayer CreateAndPlayMediaPlayer(string soundPath, bool loop, Action<MediaPlayer>? onEnded = null)
    {
        var player = new MediaPlayer();
        player.Open(new Uri(soundPath));
        player.Volume = 1.0;
        player.MediaEnded += (_, _) =>
        {
            if (loop)
            {
                player.Position = TimeSpan.Zero;
                player.Play();
            }
            else
            {
                player.Close();
                onEnded?.Invoke(player);
            }
        };
        player.MediaFailed += (_, e) =>
        {
            _logger.LogError($"[SoundService] MediaPlayer failed: {e.ErrorException}");
        };
        player.Play();
        _logger.LogDebug($"[SoundService] Playing sound: {soundPath}, loop={loop}");
        return player;
    }
    #endregion

    #region Public methods
    /// <inheritdoc/>
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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _player = CreateAndPlayMediaPlayer(soundPath, loop, _ => { _player = null; _logger.LogDebug($"[SoundService] MediaPlayer closed after playback: {soundPath}"); });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SoundService] Error playing sound. Path: {Path}, Loop: {Loop}, ThreadId: {ThreadId}", soundPath, loop, Thread.CurrentThread.ManagedThreadId);
        }
    }

    /// <inheritdoc/>
    public void StopSound()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (_player != null)
            {
                _player.Stop();
                _player.Close();
                _player = null;
                _logger.LogDebug("[SoundService] Sound stopped.");
            }
        });
    }

    /// <inheritdoc/>
    public void PlayCuckooSound(int hour)
    {
        _logger.LogDebug($"[SoundService] PlayCuckooSound called, hour={hour}");
        int soundHour = hour % 12;
        if (soundHour == 0) soundHour = 12;
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string soundPath = Path.Combine(baseDir, "Resources", "Sounds", $"{soundHour}.mp3");
        PlaySound(soundPath);
    }

    /// <inheritdoc/>
    public void PlayHalfHourChime()
    {
        _logger.LogDebug("[SoundService] PlayHalfHourChime called");
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string soundPath = Path.Combine(baseDir, "Resources", "Sounds", "halfHour.mp3");
        PlaySound(soundPath);
    }

    /// <inheritdoc/>
    public ISoundHandle PlaySoundInstance(string soundPath, bool loop = false)
    {
        try
        {
            bool fileExists = File.Exists(soundPath);
            _logger.LogDebug($"[SoundService] Sound file path: {soundPath}, Exists: {fileExists}");
            if (!fileExists)
            {
                _logger.LogWarning($"[SoundService] Sound file not found: {soundPath}");
                return new SoundHandle(new MediaPlayer());
            }
            MediaPlayer? player = null;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                player = CreateAndPlayMediaPlayer(soundPath, loop);
            });
            return new SoundHandle(player!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SoundService] Error playing sound instance. Path: {Path}, Loop: {Loop}, ThreadId: {ThreadId}", soundPath, loop, Thread.CurrentThread.ManagedThreadId);
            return new SoundHandle(new MediaPlayer());
        }
    }
    #endregion
} 