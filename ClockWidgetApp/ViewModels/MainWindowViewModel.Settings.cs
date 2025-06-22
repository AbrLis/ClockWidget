using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private int _lastCuckooHour = -1;

    /// <summary>
    /// Проверяет и корректирует значение прозрачности.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="minValue">Минимальное допустимое значение.</param>
    /// <param name="maxValue">Максимальное допустимое значение.</param>
    /// <param name="defaultValue">Значение по умолчанию.</param>
    /// <returns>Скорректированное значение прозрачности.</returns>
    private double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        return Math.Round(value / Constants.WindowSettings.OPACITY_STEP) * Constants.WindowSettings.OPACITY_STEP;
    }

    /// <summary>
    /// Проверяет и корректирует значение размера шрифта.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Скорректированное значение размера шрифта.</returns>
    private double ValidateFontSize(double value)
    {
        if (value < Constants.TextSettings.MIN_FONT_SIZE || value > Constants.TextSettings.MAX_FONT_SIZE)
        {
            return Constants.TextSettings.DEFAULT_FONT_SIZE;
        }
        return Math.Round(value / Constants.TextSettings.FONT_SIZE_STEP) * Constants.TextSettings.FONT_SIZE_STEP;
    }

    /// <summary>
    /// Обновляет настройки из объекта WidgetSettings и сохраняет их.
    /// </summary>
    /// <param name="settings">Объект настроек.</param>
    public void UpdateSettings(WidgetSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }
        settings = WidgetSettings.ValidateSettings(settings);
        if (settings.ShowDigitalClock)
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Show();
                mainWindow.Activate();
            }
        }
        else
        {
            System.Windows.Application.Current.MainWindow?.Hide();
        }
        UpdateAnalogClockSettings(settings);
        _settingsService.SaveSettings(settings);
    }

    /// <summary>
    /// Обработчик события обновления времени.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="time">Новое время.</param>
    private void OnTimeUpdated(object? sender, DateTime time)
    {
        TimeText = time.ToString(_showSeconds ? 
            Constants.DisplaySettings.TIME_FORMAT_WITH_SECONDS : 
            Constants.DisplaySettings.TIME_FORMAT_WITHOUT_SECONDS);

        // Логика кукушки
        try
        {
            if (CuckooEveryHour && time.Minute == 0 && time.Second == 0 && _lastCuckooHour != time.Hour)
            {
                _logger.LogInformation($"[MainWindowViewModel] Cuckoo: Playing sound for hour {time.Hour}");
                PlayCuckooSound(time.Hour);
                _lastCuckooHour = time.Hour;
            }
            else if (time.Minute != 0 || time.Second != 0)
            {
                _lastCuckooHour = -1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel] Error in cuckoo logic");
        }
    }

    /// <summary>
    /// Воспроизводит аудиофайл кукушки для указанного часа.
    /// </summary>
    /// <param name="hour">Час (1-12).</param>
    public void PlayCuckooSound(int hour)
    {
        _logger.LogInformation($"[MainWindowViewModel] PlayCuckooSound called, hour={hour}");
        try
        {
            int soundHour = hour % 12;
            if (soundHour == 0) soundHour = 12;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", $"{soundHour}.mp3");
            bool fileExists = System.IO.File.Exists(soundPath);
            _logger.LogInformation($"[MainWindowViewModel] Cuckoo sound file path: {soundPath}, Exists: {fileExists}");
            if (!fileExists)
            {
                _logger.LogWarning($"[MainWindowViewModel] Cuckoo sound file not found: {soundPath}");
                return;
            }
            var player = new System.Windows.Media.MediaPlayer();
            player.Open(new System.Uri(soundPath));
            player.Volume = 1.0;
            player.MediaEnded += (s, e) =>
            {
                player.Close();
                player.Dispatcher?.InvokeAsync(() => player = null);
                _logger.LogInformation($"[MainWindowViewModel] Cuckoo MediaPlayer closed after playback: {soundPath}");
            };
            player.MediaFailed += (s, e) =>
            {
                _logger.LogError($"[MainWindowViewModel] MediaPlayer failed: {e.ErrorException}");
            };
            player.Play();
            _logger.LogInformation($"[MainWindowViewModel] Playing cuckoo sound: {soundPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel] Error playing cuckoo sound");
        }
    }

    /// <summary>
    /// Инициализирует значения ViewModel из настроек.
    /// </summary>
    /// <param name="settings">Объект настроек.</param>
    private void InitializeFromSettings(WidgetSettings settings)
    {
        _backgroundOpacity = settings.BackgroundOpacity;
        _textOpacity = settings.TextOpacity;
        _fontSize = settings.FontSize;
        _showSeconds = settings.ShowSeconds;
        _showDigitalClock = settings.ShowDigitalClock;
        _showAnalogClock = settings.ShowAnalogClock;
        _analogClockSize = settings.AnalogClockSize;
        _analogClockTopmost = settings.AnalogClockTopmost;
        _digitalClockTopmost = settings.DigitalClockTopmost;
        UpdateDigitalClockTopmost();
    }
} 