using System.Text.Json.Serialization;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.Models;

/// <summary>
/// Класс, представляющий настройки виджета часов.
/// Содержит все настраиваемые параметры виджета и их значения по умолчанию.
/// </summary>
public class WidgetSettings
{
    /// <summary>
    /// Получает или устанавливает прозрачность фона виджета.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DEFAULT_WINDOW_OPACITY"/>.
    /// </summary>
    [JsonPropertyName("backgroundOpacity")]
    public double BackgroundOpacity { get; set; } = Constants.WindowSettings.DEFAULT_WINDOW_OPACITY;

    /// <summary>
    /// Получает или устанавливает прозрачность текста виджета.
    /// Значение по умолчанию: <see cref="Constants.TextSettings.DEFAULT_TEXT_OPACITY"/>.
    /// </summary>
    [JsonPropertyName("textOpacity")]
    public double TextOpacity { get; set; } = Constants.TextSettings.DEFAULT_TEXT_OPACITY;

    /// <summary>
    /// Получает или устанавливает размер шрифта текста.
    /// Значение по умолчанию: <see cref="Constants.TextSettings.DEFAULT_FONT_SIZE"/>.
    /// </summary>
    [JsonPropertyName("fontSize")]
    public double FontSize { get; set; } = Constants.TextSettings.DEFAULT_FONT_SIZE;

    /// <summary>
    /// Получает или устанавливает флаг отображения секунд.
    /// Значение по умолчанию: <see cref="Constants.DisplaySettings.DEFAULT_SHOW_SECONDS"/>.
    /// </summary>
    [JsonPropertyName("showSeconds")]
    public bool ShowSeconds { get; set; } = Constants.DisplaySettings.DEFAULT_SHOW_SECONDS;

    /// <summary>
    /// Получает или устанавливает позицию окна по горизонтали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DEFAULT_WINDOW_LEFT"/>.
    /// </summary>
    [JsonPropertyName("windowLeft")]
    public double? WindowLeft { get; set; } = Constants.WindowSettings.DEFAULT_WINDOW_LEFT;

    /// <summary>
    /// Получает или устанавливает позицию окна по вертикали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DEFAULT_WINDOW_TOP"/>.
    /// </summary>
    [JsonPropertyName("windowTop")]
    public double? WindowTop { get; set; } = Constants.WindowSettings.DEFAULT_WINDOW_TOP;

    /// <summary>
    /// Получает или устанавливает позицию окна аналоговых часов по горизонтали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_LEFT"/>.
    /// </summary>
    [JsonPropertyName("analogClockLeft")]
    public double? AnalogClockLeft { get; set; } = Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_LEFT;

    /// <summary>
    /// Получает или устанавливает позицию окна аналоговых часов по вертикали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_TOP"/>.
    /// </summary>
    [JsonPropertyName("analogClockTop")]
    public double? AnalogClockTop { get; set; } = Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_TOP;

    /// <summary>
    /// Получает или устанавливает размер окна аналоговых часов.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_SIZE"/>.
    /// </summary>
    [JsonPropertyName("analogClockSize")]
    public double AnalogClockSize { get; set; } = Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_SIZE;

    /// <summary>
    /// Получает или устанавливает флаг отображения цифровых часов.
    /// Значение по умолчанию: true.
    /// </summary>
    [JsonPropertyName("showDigitalClock")]
    public bool ShowDigitalClock { get; set; } = true;

    /// <summary>
    /// Получает или устанавливает флаг отображения аналоговых часов.
    /// Значение по умолчанию: true.
    /// </summary>
    [JsonPropertyName("showAnalogClock")]
    public bool ShowAnalogClock { get; set; } = true;

    /// <summary>
    /// Флаг "поверх всех окон" для аналоговых часов. По умолчанию: true.
    /// </summary>
    [JsonPropertyName("analogClockTopmost")]
    public bool AnalogClockTopmost { get; set; } = true;

    /// <summary>
    /// Флаг "поверх всех окон" для цифровых часов. По умолчанию: true.
    /// </summary>
    [JsonPropertyName("digitalClockTopmost")]
    public bool DigitalClockTopmost { get; set; } = true;

    /// <summary>
    /// Воспроизводить звук кукушки каждый час.
    /// Значение по умолчанию: <see cref="Constants.CuckooSettings.DEFAULT_CUCKOO_EVERY_HOUR"/>.
    /// </summary>
    [JsonPropertyName("cuckooEveryHour")]
    public bool CuckooEveryHour { get; set; } = Constants.CuckooSettings.DEFAULT_CUCKOO_EVERY_HOUR;

    /// <summary>
    /// Воспроизводить сигнал каждые полчаса (например, в 12:30, 1:30 и т.д.).
    /// Значение по умолчанию: false.
    /// </summary>
    [JsonPropertyName("halfHourChimeEnabled")]
    public bool HalfHourChimeEnabled { get; set; } = false;

    /// <summary>
    /// Язык интерфейса ("ru" или "en"). По умолчанию: "en".
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// Проверяет и корректирует значения всех настроек.
    /// </summary>
    /// <param name="settings">Объект настроек.</param>
    /// <returns>Скорректированный объект настроек.</returns>
    public static WidgetSettings ValidateSettings(WidgetSettings settings)
    {
        if (settings == null)
        {
            return new WidgetSettings();
        }

        // Валидация прозрачности фона
        settings.BackgroundOpacity = ValidateOpacity(settings.BackgroundOpacity, 
            Constants.WindowSettings.MIN_WINDOW_OPACITY, 
            Constants.WindowSettings.MAX_WINDOW_OPACITY, 
            Constants.WindowSettings.DEFAULT_WINDOW_OPACITY);
        
        // Валидация прозрачности текста
        settings.TextOpacity = ValidateOpacity(settings.TextOpacity, 
            Constants.TextSettings.MIN_TEXT_OPACITY, 
            Constants.TextSettings.MAX_TEXT_OPACITY, 
            Constants.TextSettings.DEFAULT_TEXT_OPACITY);
        
        // Валидация размера шрифта
        settings.FontSize = ValidateFontSize(settings.FontSize);
        
        // Валидация размера аналоговых часов
        settings.AnalogClockSize = ValidateAnalogClockSize(settings.AnalogClockSize);
        
        // Позиции окон не валидируются, так как могут быть null
        
        // Валидация языка
        settings.Language = ValidateLanguage(settings.Language);
        
        return settings;
    }

    /// <summary>
    /// Проверяет и корректирует значение прозрачности.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="minValue">Минимальное допустимое значение.</param>
    /// <param name="maxValue">Максимальное допустимое значение.</param>
    /// <param name="defaultValue">Значение по умолчанию.</param>
    /// <returns>Скорректированное значение прозрачности.</returns>
    public static double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
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
    public static double ValidateFontSize(double value)
    {
        if (value < Constants.TextSettings.MIN_FONT_SIZE || value > Constants.TextSettings.MAX_FONT_SIZE)
        {
            return Constants.TextSettings.DEFAULT_FONT_SIZE;
        }
        return Math.Round(value / Constants.TextSettings.FONT_SIZE_STEP) * Constants.TextSettings.FONT_SIZE_STEP;
    }

    /// <summary>
    /// Проверяет и корректирует значение размера окна аналоговых часов.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Скорректированное значение размера окна.</returns>
    public static double ValidateAnalogClockSize(double value)
    {
        if (value < Constants.WindowSettings.MIN_ANALOG_CLOCK_SIZE || 
            value > Constants.WindowSettings.MAX_ANALOG_CLOCK_SIZE)
        {
            return Constants.WindowSettings.DEFAULT_ANALOG_CLOCK_SIZE;
        }
        return Math.Round(value / Constants.WindowSettings.ANALOG_CLOCK_SIZE_STEP) * 
               Constants.WindowSettings.ANALOG_CLOCK_SIZE_STEP;
    }

    /// <summary>
    /// Проверяет и корректирует значение языка интерфейса.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Корректное значение языка ('ru' или 'en').</returns>
    public static string ValidateLanguage(string value)
    {
        return value == "en" ? "en" : "ru";
    }
} 