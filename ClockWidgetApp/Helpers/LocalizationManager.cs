using System.Globalization;
using System.Reflection;
using System.Resources;

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
    public string SettingsWindowCommonSettings => LocalizationManager.GetString("SettingsWindow_CommonSettings", _lang);
    public string SettingsWindowBackgroundOpacity => LocalizationManager.GetString("SettingsWindow_BackgroundOpacity", _lang);
    public string SettingsWindowTextOpacity => LocalizationManager.GetString("SettingsWindow_TextOpacity", _lang);
    public string SettingsWindowCuckooEveryHour => LocalizationManager.GetString("SettingsWindow_CuckooEveryHour", _lang);
    public string SettingsWindowHalfHourChimeEnabled => LocalizationManager.GetString("SettingsWindow_HalfHourChime", _lang);
    public string SettingsWindowAnalogSettings => LocalizationManager.GetString("SettingsWindow_AnalogSettings", _lang);
    public string SettingsWindowShowAnalogClock => LocalizationManager.GetString("SettingsWindow_ShowAnalog", _lang);
    public string SettingsWindowAnalogClockTopmost => LocalizationManager.GetString("SettingsWindow_AnalogTopmost", _lang);
    public string SettingsWindowAnalogClockSize => LocalizationManager.GetString("SettingsWindow_AnalogSize", _lang);
    public string SettingsWindowDigitalSettings => LocalizationManager.GetString("SettingsWindow_DigitalSettings", _lang);
    public string SettingsWindowShowDigitalClock => LocalizationManager.GetString("SettingsWindow_ShowDigital", _lang);
    public string SettingsWindowDigitalClockTopmost => LocalizationManager.GetString("SettingsWindow_DigitalTopmost", _lang);
    public string SettingsWindowShowSeconds => LocalizationManager.GetString("SettingsWindow_ShowSeconds", _lang);
    public string SettingsWindowFontSize => LocalizationManager.GetString("SettingsWindow_FontSize", _lang);
    public string SettingsWindowCloseWidgetButton => LocalizationManager.GetString("SettingsWindow_CloseWidget", _lang);
    public string SettingsWindowLogs => LocalizationManager.GetString("SettingsWindow_Logs", _lang);
    public string SettingsWindowLogsNotFound => LocalizationManager.GetString("SettingsWindow_LogsNotFound", _lang);
    public string LanguageLabel => LocalizationManager.GetString("Language_Label", _lang);
    public string MainWindowTitle => LocalizationManager.GetString("MainWindow_Title", _lang);
    public string AnalogClockWindowTitle => LocalizationManager.GetString("AnalogClockWindow_Title", _lang);
    public string TimersAndAlarmsTab => LocalizationManager.GetString("TimersAndAlarms_Tab", _lang);
    public string TimersTitle => LocalizationManager.GetString("Timers_Title", _lang);
    public string TimersAdd => LocalizationManager.GetString("Timers_Add", _lang);
    public string TimersDisableAll => LocalizationManager.GetString("Timers_DisableAll", _lang);
    public string TimersDeleteAll => LocalizationManager.GetString("Timers_DeleteAll", _lang);
    public string TimersActivateAll => LocalizationManager.GetString("Timers_ActivateAll", _lang);
    public string TimersDelete => LocalizationManager.GetString("Timers_Delete", _lang);
    public string TimersStatus => LocalizationManager.GetString("Timers_Status", _lang);
    public string TimersDisable => LocalizationManager.GetString("Timers_Disable", _lang);
    public string TimersTime => LocalizationManager.GetString("Timers_Time", _lang);
    public string TimersActive => LocalizationManager.GetString("Timers_Active", _lang);
    public string TimersInactive => LocalizationManager.GetString("Timers_Inactive", _lang);
    public string AlarmsTitle => LocalizationManager.GetString("Alarms_Title", _lang);
    public string AlarmsAdd => LocalizationManager.GetString("Alarms_Add", _lang);
    public string AlarmsDelete => LocalizationManager.GetString("Alarms_Delete", _lang);
    public string AlarmsStatus => LocalizationManager.GetString("Alarms_Status", _lang);
    public string AlarmsEnable => LocalizationManager.GetString("Alarms_Enable", _lang);
    public string AlarmsTime => LocalizationManager.GetString("Alarms_Time", _lang);
    public string WidgetSettingsTitle => LocalizationManager.GetString("WidgetSettings_Title", _lang);
    public string WidgetSettingsSize => LocalizationManager.GetString("WidgetSettings_Size", _lang);
    public string WidgetSettingsSizeTooltip => LocalizationManager.GetString("WidgetSettings_SizeTooltip", _lang);
    public string WidgetSettingsTopmost => LocalizationManager.GetString("WidgetSettings_Topmost", _lang);
    public string WidgetSettingsTopmostTooltip => LocalizationManager.GetString("WidgetSettings_TopmostTooltip", _lang);
    public string TimerHours => LocalizationManager.GetString("Timer_Hours", _lang);
    public string TimerMinutes => LocalizationManager.GetString("Timer_Minutes", _lang);
    public string TimerSeconds => LocalizationManager.GetString("Timer_Seconds", _lang);
    public string TimerNotificationTitle => LocalizationManager.GetString("TimerNotification_Title", _lang);
    public string TimerNotificationStopButton => LocalizationManager.GetString("TimerNotification_StopButton", _lang);
    public string AlarmNotificationTitle => LocalizationManager.GetString("AlarmNotification_Title", _lang);
    /// <summary>
    /// Локализованная строка для пункта меню трея 'Настройка таймера/будильника'.
    /// </summary>
    public string TrayTimerAlarmSettings => LocalizationManager.GetString("Tray_TimerAlarmSettings", _lang);
    /// <summary>
    /// Локализованная строка для пункта меню трея 'Стоп'.
    /// </summary>
    public string TrayStop => LocalizationManager.GetString("Tray_Stop", _lang);
    public string AlarmsDuplicateNotification => LocalizationManager.GetString("Alarms_DuplicateNotification", _lang);
    public string LongTimerInputTitle => LocalizationManager.GetString("LongTimerInput_Title", _lang);
    public string LongTimerInputSelectDate => LocalizationManager.GetString("LongTimerInput_SelectDate", _lang);
    public string LongTimerInputTime => LocalizationManager.GetString("LongTimerInput_Time", _lang);
    public string LongTimerInputName => LocalizationManager.GetString("LongTimerInput_Name", _lang);
    public string LongTimerInputOk => LocalizationManager.GetString("LongTimerInput_Ok", _lang);
    public string LongTimerInputCancel => LocalizationManager.GetString("LongTimerInput_Cancel", _lang);
    public string LongTimersTitle => LocalizationManager.GetString("LongTimers_Title", _lang);
    public string LongTimersAdd => LocalizationManager.GetString("LongTimers_Add", _lang);
    public string LongTimersTriggerDateTime => LocalizationManager.GetString("LongTimers_TriggerDateTime", _lang);
    /// <summary>
    /// Ошибка: выбранное время в прошлом (используется в LongTimerInputWindow).
    /// </summary>
    public string LongTimerInputErrorInPast => LocalizationManager.GetString("LongTimerInput_ErrorInPast", _lang);
}