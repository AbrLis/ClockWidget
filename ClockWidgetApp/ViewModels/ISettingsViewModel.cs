namespace ClockWidgetApp.ViewModels;

/// <summary>
/// Интерфейс для ViewModel, поддерживающих работу с настройками.
/// </summary>
public interface ISettingsViewModel
{
    double BackgroundOpacity { get; set; }
    double TextOpacity { get; set; }
    double FontSize { get; set; }
    bool ShowSeconds { get; set; }
    bool ShowDigitalClock { get; set; }
    bool ShowAnalogClock { get; set; }
    bool CuckooEveryHour { get; set; }
} 