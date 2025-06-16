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
    public static double DEFAULT_WINDOW_LEFT => WindowSettings.DEFAULT_WINDOW_LEFT;
    public static double DEFAULT_WINDOW_TOP => WindowSettings.DEFAULT_WINDOW_TOP;
    public static double DEFAULT_ANALOG_CLOCK_LEFT => WindowSettings.DEFAULT_ANALOG_CLOCK_LEFT;
    public static double DEFAULT_ANALOG_CLOCK_TOP => WindowSettings.DEFAULT_ANALOG_CLOCK_TOP;

    public static double MIN_TEXT_OPACITY => TextSettings.MIN_TEXT_OPACITY;
    public static double MAX_TEXT_OPACITY => TextSettings.MAX_TEXT_OPACITY;
    public static double DEFAULT_TEXT_OPACITY => TextSettings.DEFAULT_TEXT_OPACITY;
    public static double MIN_FONT_SIZE => TextSettings.MIN_FONT_SIZE;
    public static double MAX_FONT_SIZE => TextSettings.MAX_FONT_SIZE;
    public static double FONT_SIZE_STEP => TextSettings.FONT_SIZE_STEP;

    public static bool DEFAULT_SHOW_SECONDS => DisplaySettings.DEFAULT_SHOW_SECONDS;
    public static string TIME_FORMAT_WITH_SECONDS => DisplaySettings.TIME_FORMAT_WITH_SECONDS;
    public static string TIME_FORMAT_WITHOUT_SECONDS => DisplaySettings.TIME_FORMAT_WITHOUT_SECONDS;

    public static string SETTINGS_FILENAME => FileSettings.SETTINGS_FILENAME;

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
    /// Настройки файлов.
    /// </summary>
    public static class FileSettings
    {
        /// <summary>
        /// Имя файла настроек.
        /// </summary>
        public const string SETTINGS_FILENAME = "widget_settings.json";
    }
} 