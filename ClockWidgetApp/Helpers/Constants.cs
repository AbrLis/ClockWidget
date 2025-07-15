namespace ClockWidgetApp.Helpers;

/// <summary>
/// Класс, содержащий константы и значения по умолчанию для виджета часов.
/// </summary>
public static class Constants
{
    // Свойства для доступа из XAML
    public static double MIN_WINDOW_OPACITY => WindowSettings.MIN_WINDOW_OPACITY;
    public static double MAX_WINDOW_OPACITY => WindowSettings.MAX_WINDOW_OPACITY;
    public static double OPACITY_STEP => WindowSettings.OPACITY_STEP;
    public static double MIN_ANALOG_CLOCK_SIZE => WindowSettings.MIN_ANALOG_CLOCK_SIZE;
    public static double MAX_ANALOG_CLOCK_SIZE => WindowSettings.MAX_ANALOG_CLOCK_SIZE;
    public static double ANALOG_CLOCK_SIZE_STEP => WindowSettings.ANALOG_CLOCK_SIZE_STEP;

    public static double MIN_TEXT_OPACITY => TextSettings.MIN_TEXT_OPACITY;
    public static double MAX_TEXT_OPACITY => TextSettings.MAX_TEXT_OPACITY;
    public static double MIN_FONT_SIZE => TextSettings.MIN_FONT_SIZE;
    public static double MAX_FONT_SIZE => TextSettings.MAX_FONT_SIZE;
    public static double FONT_SIZE_STEP => TextSettings.FONT_SIZE_STEP;

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
    public const int SETTINGS_TAB_INDEX_GENERAL = 0;
    /// <summary>
    /// Индекс вкладки 'Будильники и таймеры' в окне настроек.
    /// </summary>
    public const int SETTINGS_TAB_INDEX_TIMERS = 1;

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
        public const double DEFAULT_WINDOW_OPACITY = 0.6;

        /// <summary>
        /// Минимальное значение прозрачности окна (10%).
        /// </summary>
        public const double MIN_WINDOW_OPACITY = 0.1;

        /// <summary>
        /// Максимальное значение прозрачности окна (100%).
        /// </summary>
        public const double MAX_WINDOW_OPACITY = 1.0;

        /// <summary>
        /// Шаг изменения прозрачности (10%).
        /// </summary>
        public const double OPACITY_STEP = 0.1;

        /// <summary>
        /// Позиция окна по горизонтали по умолчанию.
        /// </summary>
        public const double DEFAULT_WINDOW_LEFT = 100.0;

        /// <summary>
        /// Позиция окна по вертикали по умолчанию.
        /// </summary>
        public const double DEFAULT_WINDOW_TOP = 100.0;

        /// <summary>
        /// Позиция окна аналоговых часов по горизонтали по умолчанию.
        /// </summary>
        public const double DEFAULT_ANALOG_CLOCK_LEFT = 100.0;

        /// <summary>
        /// Позиция окна аналоговых часов по вертикали по умолчанию.
        /// </summary>
        public const double DEFAULT_ANALOG_CLOCK_TOP = 100.0;

        /// <summary>
        /// Размер окна аналоговых часов по умолчанию.
        /// </summary>
        public const double DEFAULT_ANALOG_CLOCK_SIZE = 300.0;

        /// <summary>
        /// Минимальный размер окна аналоговых часов.
        /// </summary>
        public const double MIN_ANALOG_CLOCK_SIZE = 100.0;

        /// <summary>
        /// Максимальный размер окна аналоговых часов.
        /// </summary>
        public const double MAX_ANALOG_CLOCK_SIZE = 500.0;

        /// <summary>
        /// Шаг изменения размера окна аналоговых часов.
        /// </summary>
        public const double ANALOG_CLOCK_SIZE_STEP = 10.0;
    }

    /// <summary>
    /// Настройки текста виджета.
    /// </summary>
    public static class TextSettings
    {
        /// <summary>
        /// Значение прозрачности текста по умолчанию (100%).
        /// </summary>
        public const double DEFAULT_TEXT_OPACITY = 1.0;

        /// <summary>
        /// Минимальное значение прозрачности текста (10%).
        /// </summary>
        public const double MIN_TEXT_OPACITY = 0.1;

        /// <summary>
        /// Максимальное значение прозрачности текста (100%).
        /// </summary>
        public const double MAX_TEXT_OPACITY = 1.0;

        /// <summary>
        /// Размер шрифта по умолчанию.
        /// </summary>
        public const double DEFAULT_FONT_SIZE = 48.0;

        /// <summary>
        /// Минимальный размер шрифта.
        /// </summary>
        public const double MIN_FONT_SIZE = 24.0;

        /// <summary>
        /// Максимальный размер шрифта.
        /// </summary>
        public const double MAX_FONT_SIZE = 96.0;

        /// <summary>
        /// Шаг изменения размера шрифта.
        /// </summary>
        public const double FONT_SIZE_STEP = 4.0;
    }

    /// <summary>
    /// Настройки отображения времени.
    /// </summary>
    public static class DisplaySettings
    {
        /// <summary>
        /// Флаг отображения секунд по умолчанию.
        /// </summary>
        public const bool DEFAULT_SHOW_SECONDS = true;

        /// <summary>
        /// Формат времени с отображением секунд.
        /// </summary>
        public const string TIME_FORMAT_WITH_SECONDS = "HH:mm:ss";

        /// <summary>
        /// Формат времени без отображения секунд.
        /// </summary>
        public const string TIME_FORMAT_WITHOUT_SECONDS = "HH:mm";
    }
    /// <summary>
    /// Настройки кукушки.
    /// </summary>
    public static class CuckooSettings
    {
        /// <summary>
        /// Воспроизводить звук кукушки каждый час по умолчанию (false).
        /// </summary>
        public const bool DEFAULT_CUCKOO_EVERY_HOUR = false;
    }
} 