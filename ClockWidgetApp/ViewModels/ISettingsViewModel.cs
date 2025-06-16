namespace ClockWidgetApp.ViewModels;

public interface ISettingsViewModel
{
    double BackgroundOpacity { get; set; }
    double TextOpacity { get; set; }
    double FontSize { get; set; }
    bool ShowSeconds { get; set; }
    bool ShowDigitalClock { get; set; }
    bool ShowAnalogClock { get; set; }
} 