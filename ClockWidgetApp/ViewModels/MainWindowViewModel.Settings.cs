using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private int _lastCuckooHour = -1;
    private readonly ClockWidgetApp.Services.SoundService _soundService = new();

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
                _soundService.PlayCuckooSound(time.Hour);
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