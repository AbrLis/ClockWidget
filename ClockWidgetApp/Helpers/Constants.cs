namespace ClockWidgetApp.Helpers;

/// <summary>
/// Класс, содержащий константы и значения по умолчанию для виджета часов.
/// </summary>
public static class Constants
{
    // Свойства для доступа из XAML
    public static double MinWindowOpacity => WindowSettings.MinWindowOpacity;
    public static double MaxWindowOpacity => WindowSettings.MaxWindowOpacity;
    public static double OpacityStep => WindowSettings.OpacityStep;
    public static double MinAnalogClockSize => WindowSettings.MinAnalogClockSize;
    public static double MaxAnalogClockSize => WindowSettings.MaxAnalogClockSize;
    public static double AnalogClockSizeStep => WindowSettings.AnalogClockSizeStep;

    public static double MinTextOpacity => TextSettings.MinTextOpacity;
    public static double MaxTextOpacity => TextSettings.MaxTextOpacity;
    public static double MinFontSize => TextSettings.MinFontSize;
    public static double MaxFontSize => TextSettings.MaxFontSize;
    public static double FontSizeStep => TextSettings.FontSizeStep;

    /// <summary>
    /// Максимальная длина названия длинного таймера.
    /// </summary>
    public const int LongTimerNameMaxLength = 60;

    /// <summary>
    /// Максимальная длина имени длинного таймера для отображения в тултипе (до обрезки и добавления ...).
    /// </summary>
    public const int LongTimerTooltipNameMaxLength = 20;

    /// <summary>
    /// Индекс вкладки 'Общие настройки' в окне настроек.
    /// </summary>
    public const int SettingsTabIndexGeneral = 0;
    /// <summary>
    /// Индекс вкладки 'Будильники и таймеры' в окне настроек.
    /// </summary>
    public const int SettingsTabIndexTimers = 1;

    /// <summary>
    /// Время отображения уведомления о дублирующемся будильнике (мс).
    /// </summary>
    public const int DuplicateAlarmNotificationDurationMs = 3000;

    /// <summary>
    /// Время отображения ошибки выбора времени в LongTimerInputWindow (мс).
    /// </summary>
    public const int LongTimerInputErrorDurationMs = 3000;

    /// <summary>
    /// Настройки окна виджета.
    /// </summary>
    public static class WindowSettings
    {
        /// <summary>
        /// Значение прозрачности окна по умолчанию (90%).
        /// </summary>
        public const double DefaultWindowOpacity = 0.6;

        /// <summary>
        /// Минимальное значение прозрачности окна (10%).
        /// </summary>
        public const double MinWindowOpacity = 0.1;

        /// <summary>
        /// Максимальное значение прозрачности окна (100%).
        /// </summary>
        public const double MaxWindowOpacity = 1.0;

        /// <summary>
        /// Шаг изменения прозрачности (10%).
        /// </summary>
        public const double OpacityStep = 0.1;

        /// <summary>
        /// Позиция окна по горизонтали по умолчанию.
        /// </summary>
        public const double DefaultWindowLeft = 100.0;

        /// <summary>
        /// Позиция окна по вертикали по умолчанию.
        /// </summary>
        public const double DefaultWindowTop = 100.0;

        /// <summary>
        /// Позиция окна аналоговых часов по горизонтали по умолчанию.
        /// </summary>
        public const double DefaultAnalogClockLeft = 100.0;

        /// <summary>
        /// Позиция окна аналоговых часов по вертикали по умолчанию.
        /// </summary>
        public const double DefaultAnalogClockTop = 100.0;

        /// <summary>
        /// Размер окна аналоговых часов по умолчанию.
        /// </summary>
        public const double DefaultAnalogClockSize = 300.0;

        /// <summary>
        /// Минимальный размер окна аналоговых часов.
        /// </summary>
        public const double MinAnalogClockSize = 100.0;

        /// <summary>
        /// Максимальный размер окна аналоговых часов.
        /// </summary>
        public const double MaxAnalogClockSize = 500.0;

        /// <summary>
        /// Шаг изменения размера окна аналоговых часов.
        /// </summary>
        public const double AnalogClockSizeStep = 10.0;
    }

    /// <summary>
    /// Настройки текста виджета.
    /// </summary>
    public static class TextSettings
    {
        /// <summary>
        /// Значение прозрачности текста по умолчанию (100%).
        /// </summary>
        public const double DefaultTextOpacity = 1.0;

        /// <summary>
        /// Минимальное значение прозрачности текста (10%).
        /// </summary>
        public const double MinTextOpacity = 0.1;

        /// <summary>
        /// Максимальное значение прозрачности текста (100%).
        /// </summary>
        public const double MaxTextOpacity = 1.0;

        /// <summary>
        /// Размер шрифта по умолчанию.
        /// </summary>
        public const double DefaultFontSize = 48.0;

        /// <summary>
        /// Минимальный размер шрифта.
        /// </summary>
        public const double MinFontSize = 24.0;

        /// <summary>
        /// Максимальный размер шрифта.
        /// </summary>
        public const double MaxFontSize = 96.0;

        /// <summary>
        /// Шаг изменения размера шрифта.
        /// </summary>
        public const double FontSizeStep = 4.0;
    }

    /// <summary>
    /// Настройки отображения времени.
    /// </summary>
    public static class DisplaySettings
    {
        /// <summary>
        /// Флаг отображения секунд по умолчанию.
        /// </summary>
        public const bool DefaultShowSeconds = true;

        /// <summary>
        /// Формат времени с отображением секунд.
        /// </summary>
        public const string TimeFormatWithSeconds = "HH:mm:ss";

        /// <summary>
        /// Формат времени без отображения секунд.
        /// </summary>
        public const string TimeFormatWithoutSeconds = "HH:mm";
    }
    /// <summary>
    /// Настройки кукушки.
    /// </summary>
    public static class CuckooSettings
    {
        /// <summary>
        /// Воспроизводить звук кукушки каждый час по умолчанию (false).
        /// </summary>
        public const bool DefaultCuckooEveryHour = false;
    }
}