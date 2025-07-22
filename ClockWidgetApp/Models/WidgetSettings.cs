using System.Text.Json.Serialization;
using ClockWidgetApp.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClockWidgetApp.Models;

/// <summary>
/// Класс, представляющий настройки виджета часов.
/// Содержит все настраиваемые параметры виджета и их значения по умолчанию.
/// </summary>
public class WidgetSettings : INotifyPropertyChanged
{
    private double _backgroundOpacity = Constants.WindowSettings.DefaultWindowOpacity;
    /// <summary>
    /// Получает или устанавливает прозрачность фона виджета.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DefaultWindowOpacity"/>.
    /// </summary>
    [JsonPropertyName("backgroundOpacity")]
    public double BackgroundOpacity
    {
        get => _backgroundOpacity;
        set
        {
            if (DoubleEquals(_backgroundOpacity, value)) return;
            _backgroundOpacity = value; OnPropertyChanged();
        }
    }

    private double _textOpacity = Constants.TextSettings.DefaultTextOpacity;
    /// <summary>
    /// Получает или устанавливает прозрачность текста виджета.
    /// Значение по умолчанию: <see cref="Constants.TextSettings.DefaultTextOpacity"/>.
    /// </summary>
    [JsonPropertyName("textOpacity")]
    public double TextOpacity
    {
        get => _textOpacity;
        set { if (!DoubleEquals(_textOpacity, value)) { _textOpacity = value; OnPropertyChanged(); } }
    }

    private double _fontSize = Constants.TextSettings.DefaultFontSize;
    /// <summary>
    /// Получает или устанавливает размер шрифта текста.
    /// Значение по умолчанию: <see cref="Constants.TextSettings.DefaultFontSize"/>.
    /// </summary>
    [JsonPropertyName("fontSize")]
    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (DoubleEquals(_fontSize, value)) return;
            _fontSize = value; OnPropertyChanged();
        }
    }

    private bool _showSeconds = Constants.DisplaySettings.DefaultShowSeconds;
    /// <summary>
    /// Получает или устанавливает флаг отображения секунд.
    /// Значение по умолчанию: <see cref="Constants.DisplaySettings.DefaultShowSeconds"/>.
    /// </summary>
    [JsonPropertyName("showSeconds")]
    public bool ShowSeconds
    {
        get => _showSeconds;
        set { if (_showSeconds != value) { _showSeconds = value; OnPropertyChanged(); } }
    }

    private double? _windowLeft = Constants.WindowSettings.DefaultWindowLeft;
    /// <summary>
    /// Получает или устанавливает позицию окна по горизонтали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DefaultWindowLeft"/>.
    /// </summary>
    [JsonPropertyName("windowLeft")]
    public double? WindowLeft
    {
        get => _windowLeft;
        set
        {
            // Упрощённое сравнение nullable double с учётом точности
            if (DoubleEquals(_windowLeft ?? double.NaN, value ?? double.NaN)) return;
            _windowLeft = value; OnPropertyChanged();
        }
    }

    private double? _windowTop = Constants.WindowSettings.DefaultWindowTop;
    /// <summary>
    /// Получает или устанавливает позицию окна по вертикали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DefaultWindowTop"/>.
    /// </summary>
    [JsonPropertyName("windowTop")]
    public double? WindowTop
    {
        get => _windowTop;
        set
        {
            if (DoubleEquals(_windowTop ?? double.NaN, value ?? double.NaN)) return;
            _windowTop = value; OnPropertyChanged();
        }
    }

    private double? _analogClockLeft = Constants.WindowSettings.DefaultAnalogClockLeft;
    /// <summary>
    /// Получает или устанавливает позицию окна аналоговых часов по горизонтали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DefaultAnalogClockLeft"/>.
    /// </summary>
    [JsonPropertyName("analogClockLeft")]
    public double? AnalogClockLeft
    {
        get => _analogClockLeft;
        set
        {
            if (DoubleEquals(_analogClockLeft ?? double.NaN, value ?? double.NaN)) return;
            _analogClockLeft = value; OnPropertyChanged();
        }
    }

    private double? _analogClockTop = Constants.WindowSettings.DefaultAnalogClockTop;
    /// <summary>
    /// Получает или устанавливает позицию окна аналоговых часов по вертикали.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DefaultAnalogClockTop"/>.
    /// </summary>
    [JsonPropertyName("analogClockTop")]
    public double? AnalogClockTop
    {
        get => _analogClockTop;
        set
        {
            if (DoubleEquals(_analogClockTop ?? double.NaN, value ?? double.NaN)) return;
            _analogClockTop = value; OnPropertyChanged();
        }
    }

    private double _analogClockSize = Constants.WindowSettings.DefaultAnalogClockSize;
    /// <summary>
    /// Получает или устанавливает размер окна аналоговых часов.
    /// Значение по умолчанию: <see cref="Constants.WindowSettings.DefaultAnalogClockSize"/>.
    /// </summary>
    [JsonPropertyName("analogClockSize")]
    public double AnalogClockSize
    {
        get => _analogClockSize;
        set
        {
            if (DoubleEquals(_analogClockSize, value)) return;
            _analogClockSize = value; OnPropertyChanged();
        }
    }

    private bool _showDigitalClock = true;
    /// <summary>
    /// Получает или устанавливает флаг отображения цифровых часов.
    /// Значение по умолчанию: true.
    /// </summary>
    [JsonPropertyName("showDigitalClock")]
    public bool ShowDigitalClock
    {
        get => _showDigitalClock;
        set { if (_showDigitalClock != value) { _showDigitalClock = value; OnPropertyChanged(); } }
    }

    private bool _showAnalogClock = true;
    /// <summary>
    /// Получает или устанавливает флаг отображения аналоговых часов.
    /// Значение по умолчанию: true.
    /// </summary>
    [JsonPropertyName("showAnalogClock")]
    public bool ShowAnalogClock
    {
        get => _showAnalogClock;
        set { if (_showAnalogClock != value) { _showAnalogClock = value; OnPropertyChanged(); } }
    }

    private bool _analogClockTopmost = true;
    /// <summary>
    /// Флаг "поверх всех окон" для аналоговых часов. По умолчанию: true.
    /// </summary>
    [JsonPropertyName("analogClockTopmost")]
    public bool AnalogClockTopmost
    {
        get => _analogClockTopmost;
        set { if (_analogClockTopmost != value) { _analogClockTopmost = value; OnPropertyChanged(); } }
    }

    private bool _digitalClockTopmost = true;
    /// <summary>
    /// Флаг "поверх всех окон" для цифровых часов. По умолчанию: true.
    /// </summary>
    [JsonPropertyName("digitalClockTopmost")]
    public bool DigitalClockTopmost
    {
        get => _digitalClockTopmost;
        set { if (_digitalClockTopmost != value) { _digitalClockTopmost = value; OnPropertyChanged(); } }
    }

    private bool _cuckooEveryHour = Constants.CuckooSettings.DefaultCuckooEveryHour;
    /// <summary>
    /// Воспроизводить звук кукушки каждый час.
    /// Значение по умолчанию: <see cref="Constants.CuckooSettings.DefaultCuckooEveryHour"/>.
    /// </summary>
    [JsonPropertyName("cuckooEveryHour")]
    public bool CuckooEveryHour
    {
        get => _cuckooEveryHour;
        set { if (_cuckooEveryHour != value) { _cuckooEveryHour = value; OnPropertyChanged(); } }
    }

    private bool _halfHourChimeEnabled;
    /// <summary>
    /// Воспроизводить сигнал каждые полчаса (например, в 12:30, 1:30 и т.д.).
    /// Значение по умолчанию: false.
    /// </summary>
    [JsonPropertyName("halfHourChimeEnabled")]
    public bool HalfHourChimeEnabled
    {
        get => _halfHourChimeEnabled;
        set { if (_halfHourChimeEnabled != value) { _halfHourChimeEnabled = value; OnPropertyChanged(); } }
    }

    private string _language = "en";
    /// <summary>
    /// Язык интерфейса ("ru" или "en"). По умолчанию: "en".
    /// </summary>
    [JsonPropertyName("language")]
    public string Language
    {
        get => _language;
        set { if (_language != value) { _language = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Проверяет и корректирует значения всех настроек.
    /// </summary>
    /// <param name="settings">Объект настроек.</param>
    /// <returns>Скорректированный объект настроек.</returns>
    public static WidgetSettings ValidateSettings(WidgetSettings settings)
    {
        // Валидация прозрачности фона
        settings.BackgroundOpacity = ValidateOpacity(settings.BackgroundOpacity,
            Constants.WindowSettings.MinWindowOpacity,
            Constants.WindowSettings.MaxWindowOpacity,
            Constants.WindowSettings.DefaultWindowOpacity);

        // Валидация прозрачности текста
        settings.TextOpacity = ValidateOpacity(settings.TextOpacity,
            Constants.TextSettings.MinTextOpacity,
            Constants.TextSettings.MaxTextOpacity,
            Constants.TextSettings.DefaultTextOpacity);

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
        return Math.Round(value / Constants.WindowSettings.OpacityStep) * Constants.WindowSettings.OpacityStep;
    }

    /// <summary>
    /// Проверяет и корректирует значение размера шрифта.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Скорректированное значение размера шрифта.</returns>
    public static double ValidateFontSize(double value)
    {
        if (value < Constants.TextSettings.MinFontSize || value > Constants.TextSettings.MaxFontSize)
        {
            return Constants.TextSettings.DefaultFontSize;
        }
        return Math.Round(value / Constants.TextSettings.FontSizeStep) * Constants.TextSettings.FontSizeStep;
    }

    /// <summary>
    /// Проверяет и корректирует значение размера окна аналоговых часов.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns>Скорректированное значение размера окна.</returns>
    public static double ValidateAnalogClockSize(double value)
    {
        if (value < Constants.WindowSettings.MinAnalogClockSize ||
            value > Constants.WindowSettings.MaxAnalogClockSize)
        {
            return Constants.WindowSettings.DefaultAnalogClockSize;
        }
        return Math.Round(value / Constants.WindowSettings.AnalogClockSizeStep) *
               Constants.WindowSettings.AnalogClockSizeStep;
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

    /// <summary>
    /// Сравнивает два значения double с учетом заданной точности.
    /// </summary>
    /// <param name="a">Первое значение.</param>
    /// <param name="b">Второе значение.</param>
    /// <param name="epsilon">Точность сравнения (по умолчанию 1e-6).</param>
    /// <returns>True, если значения равны с учетом точности, иначе false.</returns>
    private static bool DoubleEquals(double a, double b, double epsilon = 1e-6)
        => Math.Abs(a - b) < epsilon;
}