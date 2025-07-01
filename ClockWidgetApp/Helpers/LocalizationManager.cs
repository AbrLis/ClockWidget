using System.Globalization;
using System.Resources;
using System.Reflection;

namespace ClockWidgetApp.Helpers;

/// <summary>
/// Менеджер локализации для динамического получения строк из ресурсов по выбранному языку.
/// </summary>
public static class LocalizationManager
{
    private static ResourceManager? _resourceManagerRu;
    private static ResourceManager? _resourceManagerEn;

    private static string _currentLanguage = "en";
    public static string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public static event EventHandler? LanguageChanged;

    static LocalizationManager()
    {
        // Путь к ресурсам (пространство имён + имя файла без расширения)
        _resourceManagerRu = new ResourceManager("ClockWidgetApp.Resources.Localization.StringsRus", Assembly.GetExecutingAssembly());
        _resourceManagerEn = new ResourceManager("ClockWidgetApp.Resources.Localization.StringsEng", Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Получить строку по ключу и языку ('ru' или 'en').
    /// </summary>
    public static string GetString(string key)
    {
        return GetString(key, _currentLanguage);
    }

    public static string GetString(string key, string lang)
    {
        if (lang == "en")
            return _resourceManagerEn?.GetString(key, CultureInfo.InvariantCulture) ?? key;
        return _resourceManagerRu?.GetString(key, CultureInfo.InvariantCulture) ?? key;
    }

    public static LocalizedStrings GetLocalizedStrings() => new LocalizedStrings(_currentLanguage);
    public static void SetLanguage(string lang) => CurrentLanguage = lang;
}

public class LocalizedStrings
{
    private readonly string _lang;
    public LocalizedStrings(string lang) { _lang = lang; }
    public string SettingsWindow_CommonSettings => LocalizationManager.GetString("SettingsWindow_CommonSettings", _lang);
    public string SettingsWindow_BackgroundOpacity => LocalizationManager.GetString("SettingsWindow_BackgroundOpacity", _lang);
    public string SettingsWindow_TextOpacity => LocalizationManager.GetString("SettingsWindow_TextOpacity", _lang);
    public string SettingsWindow_CuckooEveryHour => LocalizationManager.GetString("SettingsWindow_CuckooEveryHour", _lang);
    public string SettingsWindow_HalfHourChimeEnabled => LocalizationManager.GetString("SettingsWindow_HalfHourChime", _lang);
    public string SettingsWindow_AnalogSettings => LocalizationManager.GetString("SettingsWindow_AnalogSettings", _lang);
    public string SettingsWindow_ShowAnalogClock => LocalizationManager.GetString("SettingsWindow_ShowAnalog", _lang);
    public string SettingsWindow_AnalogClockTopmost => LocalizationManager.GetString("SettingsWindow_AnalogTopmost", _lang);
    public string SettingsWindow_AnalogClockSize => LocalizationManager.GetString("SettingsWindow_AnalogSize", _lang);
    public string SettingsWindow_DigitalSettings => LocalizationManager.GetString("SettingsWindow_DigitalSettings", _lang);
    public string SettingsWindow_ShowDigitalClock => LocalizationManager.GetString("SettingsWindow_ShowDigital", _lang);
    public string SettingsWindow_DigitalClockTopmost => LocalizationManager.GetString("SettingsWindow_DigitalTopmost", _lang);
    public string SettingsWindow_ShowSeconds => LocalizationManager.GetString("SettingsWindow_ShowSeconds", _lang);
    public string SettingsWindow_FontSize => LocalizationManager.GetString("SettingsWindow_FontSize", _lang);
    public string SettingsWindow_CloseWidgetButton => LocalizationManager.GetString("SettingsWindow_CloseWidget", _lang);
    public string SettingsWindow_Logs => LocalizationManager.GetString("SettingsWindow_Logs", _lang);
    public string SettingsWindow_LogsNotFound => LocalizationManager.GetString("SettingsWindow_LogsNotFound", _lang);
    public string LanguageLabel => LocalizationManager.GetString("Language_Label", _lang);
    public string MainWindow_Title => LocalizationManager.GetString("MainWindow_Title", _lang);
    public string AnalogClockWindow_Title => LocalizationManager.GetString("AnalogClockWindow_Title", _lang);
    public string TimersAndAlarms_Tab => LocalizationManager.GetString("TimersAndAlarms_Tab", _lang);
    public string Timers_Title => LocalizationManager.GetString("Timers_Title", _lang);
    public string Timers_Add => LocalizationManager.GetString("Timers_Add", _lang);
    public string Timers_DisableAll => LocalizationManager.GetString("Timers_DisableAll", _lang);
    public string Timers_DeleteAll => LocalizationManager.GetString("Timers_DeleteAll", _lang);
    public string Timers_ActivateAll => LocalizationManager.GetString("Timers_ActivateAll", _lang);
    public string Timers_Delete => LocalizationManager.GetString("Timers_Delete", _lang);
    public string Timers_Status => LocalizationManager.GetString("Timers_Status", _lang);
    public string Timers_Disable => LocalizationManager.GetString("Timers_Disable", _lang);
    public string Timers_Time => LocalizationManager.GetString("Timers_Time", _lang);
    public string Timers_Active => LocalizationManager.GetString("Timers_Active", _lang);
    public string Timers_Inactive => LocalizationManager.GetString("Timers_Inactive", _lang);
    public string Alarms_Title => LocalizationManager.GetString("Alarms_Title", _lang);
    public string Alarms_Add => LocalizationManager.GetString("Alarms_Add", _lang);
    public string Alarms_Delete => LocalizationManager.GetString("Alarms_Delete", _lang);
    public string Alarms_Status => LocalizationManager.GetString("Alarms_Status", _lang);
    public string Alarms_Enable => LocalizationManager.GetString("Alarms_Enable", _lang);
    public string Alarms_Time => LocalizationManager.GetString("Alarms_Time", _lang);
    public string WidgetSettings_Title => LocalizationManager.GetString("WidgetSettings_Title", _lang);
    public string WidgetSettings_Size => LocalizationManager.GetString("WidgetSettings_Size", _lang);
    public string WidgetSettings_SizeTooltip => LocalizationManager.GetString("WidgetSettings_SizeTooltip", _lang);
    public string WidgetSettings_Topmost => LocalizationManager.GetString("WidgetSettings_Topmost", _lang);
    public string WidgetSettings_TopmostTooltip => LocalizationManager.GetString("WidgetSettings_TopmostTooltip", _lang);
    public string Timer_Hours => LocalizationManager.GetString("Timer_Hours", _lang);
    public string Timer_Minutes => LocalizationManager.GetString("Timer_Minutes", _lang);
    public string Timer_Seconds => LocalizationManager.GetString("Timer_Seconds", _lang);
} 