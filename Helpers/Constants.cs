namespace ClockWidgetApp.Helpers;

public static class Constants
{
    // Настройки окна
    public const double DEFAULT_WINDOW_OPACITY = 0.6;
    public const double MIN_WINDOW_OPACITY = 0.1;
    public const double MAX_WINDOW_OPACITY = 1.0;
    public const double OPACITY_STEP = 0.1;
    public const double DEFAULT_WINDOW_LEFT = 100.0;
    public const double DEFAULT_WINDOW_TOP = 100.0;

    // Настройки текста
    public const double DEFAULT_TEXT_OPACITY = 1.0;
    public const double MIN_TEXT_OPACITY = 0.1;
    public const double MAX_TEXT_OPACITY = 1.0;
    public const double DEFAULT_FONT_SIZE = 48.0;
    public const double MIN_FONT_SIZE = 24.0;
    public const double MAX_FONT_SIZE = 96.0;
    public const double FONT_SIZE_STEP = 4.0;

    // Настройки отображения
    public const bool DEFAULT_SHOW_SECONDS = true;
    public const string TIME_FORMAT_WITH_SECONDS = "HH:mm:ss";
    public const string TIME_FORMAT_WITHOUT_SECONDS = "HH:mm";

    // Настройки файла
    public const string SETTINGS_FILENAME = "widget_settings.json";
} 