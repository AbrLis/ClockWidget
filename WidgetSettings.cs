using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace ClockWidgetApp;

public class WidgetSettings
{
    [JsonPropertyName("backgroundOpacity")]
    public double BackgroundOpacity { get; set; }

    [JsonPropertyName("textOpacity")]
    public double TextOpacity { get; set; }

    [JsonPropertyName("fontSize")]
    public double FontSize { get; set; }

    [JsonPropertyName("showSeconds")]
    public bool ShowSeconds { get; set; }

    [JsonPropertyName("windowLeft")]
    public double? WindowLeft { get; set; }

    [JsonPropertyName("windowTop")]
    public double? WindowTop { get; set; }

    public WidgetSettings()
    {
        // Значения по умолчанию
        BackgroundOpacity = Constants.DEFAULT_WINDOW_OPACITY;
        TextOpacity = Constants.DEFAULT_TEXT_OPACITY;
        FontSize = Constants.DEFAULT_FONT_SIZE;
        ShowSeconds = Constants.DEFAULT_SHOW_SECONDS;
        // Позиция окна по умолчанию не устанавливается
    }

    public static WidgetSettings Load()
    {
        try
        {
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SETTINGS_FILENAME);
            
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
                Constants.MIN_WINDOW_OPACITY, 
                Constants.MAX_WINDOW_OPACITY, 
                Constants.DEFAULT_WINDOW_OPACITY);
            
            settings.TextOpacity = ValidateOpacity(settings.TextOpacity, 
                Constants.MIN_TEXT_OPACITY, 
                Constants.MAX_TEXT_OPACITY, 
                Constants.DEFAULT_TEXT_OPACITY);
            
            settings.FontSize = ValidateFontSize(settings.FontSize);
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
            string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SETTINGS_FILENAME);
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, jsonString);
        }
        catch (Exception)
        {
            // В случае ошибки сохранения просто игнорируем её
            // В реальном приложении здесь можно добавить логирование
        }
    }

    private static double ValidateOpacity(double value, double minValue, double maxValue, double defaultValue)
    {
        if (value < minValue || value > maxValue)
        {
            return defaultValue;
        }
        // Округляем значение до ближайшего шага
        return Math.Round(value / Constants.OPACITY_STEP) * Constants.OPACITY_STEP;
    }

    private static double ValidateFontSize(double value)
    {
        if (value < Constants.MIN_FONT_SIZE || value > Constants.MAX_FONT_SIZE)
        {
            return Constants.DEFAULT_FONT_SIZE;
        }
        // Округляем значение до ближайшего шага
        return Math.Round(value / Constants.FONT_SIZE_STEP) * Constants.FONT_SIZE_STEP;
    }
} 