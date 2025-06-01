using ClockWidgetApp.Models;

namespace ClockWidgetApp.Services;

public class SettingsService
{
    private WidgetSettings _settings;

    public SettingsService()
    {
        _settings = WidgetSettings.Load();
    }

    public WidgetSettings CurrentSettings => _settings;

    public void SaveSettings()
    {
        _settings.Save();
    }

    public void UpdateSettings(Action<WidgetSettings> updateAction)
    {
        updateAction(_settings);
        SaveSettings();
    }
} 