using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private int _lastCuckooHour = -1;
    private int _lastHalfHourChimeMinute = -1;

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
        return Math.Round(value / Constants.WindowSettings.OpacityStep) * Constants.WindowSettings.OpacityStep;
    }

    /// <summary>
    /// Проверяет и корректирует значение размера шрифта.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Скорректированное значение размера шрифта.</returns>
    private double ValidateFontSize(double value)
    {
        if (value < Constants.TextSettings.MinFontSize || value > Constants.TextSettings.MaxFontSize)
        {
            return Constants.TextSettings.DefaultFontSize;
        }
        return Math.Round(value / Constants.TextSettings.FontSizeStep) * Constants.TextSettings.FontSizeStep;
    }

    /// <summary>
    /// Обработчик события обновления времени.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="time">Новое время.</param>
    private void OnTimeUpdated(object? sender, DateTime time)
    {
        TimeText = time.ToString(_showSeconds ?
            Constants.DisplaySettings.TimeFormatWithSeconds :
            Constants.DisplaySettings.TimeFormatWithoutSeconds);

        // Логика кукушки
        try
        {
            if (CuckooEveryHour && time.Minute == 0 && _lastCuckooHour != time.Hour)
            {
                _logger.LogDebug($"[MainWindowViewModel] Cuckoo: Playing sound for hour {time.Hour}");
                _soundService.PlayCuckooSound(time.Hour);
                _lastCuckooHour = time.Hour;
                return;
            }
            if (time.Minute != 0)
            {
                _lastCuckooHour = -1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel] Error in cuckoo logic");
        }

        // Логика сигнала каждые полчаса
        try
        {
            if (HalfHourChimeEnabled && time.Minute == 30 && _lastHalfHourChimeMinute != time.Minute)
            {
                _logger.LogDebug($"[MainWindowViewModel] HalfHourChime: Playing half-hour chime at {time:HH:mm:ss}");
                _soundService.PlayHalfHourChime();
                _lastHalfHourChimeMinute = time.Minute;
                return;
            }
            if (time.Minute != 30)
            {
                _lastHalfHourChimeMinute = -1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel] Error in half-hour chime logic");
        }
    }

    /// <summary>
    /// Инициализирует значения ViewModel из настроек.
    /// </summary>
    /// <param name="settings">Объект настроек.</param>
    private void InitializeFromSettings(WidgetSettings settings)
    {
        var validated = WidgetSettings.ValidateSettings(settings);
        BackgroundOpacity = validated.BackgroundOpacity;
        TextOpacity = validated.TextOpacity;
        FontSize = validated.FontSize;
        _showSeconds = validated.ShowSeconds;
        _analogClockSize = validated.AnalogClockSize;
        LocalizationManager.SetLanguage(validated.Language);
        UpdateDigitalClockTopmost();
        UpdateAnalogClockTopmost();
    }
}