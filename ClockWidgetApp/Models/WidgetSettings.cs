using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
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

    public WidgetSettings()
    {
        // Значения по умолчанию уже установлены через инициализаторы свойств
    }

    public static WidgetSettings Load()
    {
        try
        {
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.FileSettings.SETTINGS_FILENAME);
            
            if (!File.Exists(settingsPath))
            {
                return new WidgetSettings();
            }

            string jsonString = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<WidgetSettings>(jsonString);

            // Проверяем корректность загруженных значений
            if (settings == null)
            {
                return new WidgetSettings();
            }

            // Валидация значений
            settings.BackgroundOpacity = ValidateOpacity(settings.BackgroundOpacity, 
                Constants.WindowSettings.MIN_WINDOW_OPACITY, 
                Constants.WindowSettings.MAX_WINDOW_OPACITY, 
                Constants.WindowSettings.DEFAULT_WINDOW_OPACITY);
            
            settings.TextOpacity = ValidateOpacity(settings.TextOpacity, 
                Constants.TextSettings.MIN_TEXT_OPACITY, 
                Constants.TextSettings.MAX_TEXT_OPACITY, 
                Constants.TextSettings.DEFAULT_TEXT_OPACITY);
            
            settings.FontSize = ValidateFontSize(settings.FontSize);
            settings.AnalogClockSize = ValidateAnalogClockSize(settings.AnalogClockSize);
            // ShowSeconds - булево значение, не требует валидации

            // Позиция окна не валидируется, так как может быть null

            return settings;
        }
        catch (Exception)
        {
            // В случае любой ошибки возвращаем настройки по умолчанию
            return new WidgetSettings();
        }
    }

    public void Save()
    {
        try
        {
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.FileSettings.SETTINGS_FILENAME);
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, jsonString);
        }
        catch (Exception)
        {
            // В случае ошибки сохранения просто игнорируем её
            // можно добавить логирование
        }
    }

    /// <summary>
    /// Проверяет и корректирует значения настроек.
    /// </summary>
    /// <param name="settings">Настройки для проверки.</param>
    /// <returns>Скорректированные настройки.</returns>
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
        
        // Убеждаемся, что хотя бы одно окно видимо
        if (!settings.ShowDigitalClock && !settings.ShowAnalogClock)
        {
            settings.ShowDigitalClock = true;
        }

        // Позиции окон не валидируются, так как могут быть null
        
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
} 