using System.IO;
using System.Text.Json;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для управления настройками виджета.
/// Обеспечивает сохранение и загрузку настроек в JSON-файл.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private readonly string _timersAlarmsPath;
    private WidgetSettings _currentSettings;
    private readonly ILogger<SettingsService> _logger;

    /// <summary>
    /// Получает текущие настройки виджета.
    /// </summary>
    public WidgetSettings CurrentSettings => _currentSettings;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SettingsService"/>.
    /// Загружает настройки из файла или создает настройки по умолчанию.
    /// </summary>
    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockWidget");
            
        // Создаем папку, если она не существует
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
            _logger.LogDebug("[SettingsService] Created settings directory: {Path}", appDataPath);
        }

        _settingsPath = Path.Combine(
            appDataPath,
            Constants.FileSettings.SETTINGS_FILENAME);
            
        _timersAlarmsPath = Path.Combine(appDataPath, "timers_alarms.json");
        _logger.LogDebug("[SettingsService] Settings file path: {Path}", _settingsPath);
        _currentSettings = LoadSettings();
    }

    /// <summary>
    /// Обновляет настройки с помощью указанного действия. Сохраняет только в памяти (буфер), не на диск.
    /// Для сохранения на диск используйте <see cref="SaveBufferedSettings"/>.
    /// </summary>
    /// <param name="updateAction">Действие для обновления настроек.</param>
    public void UpdateSettings(Action<WidgetSettings> updateAction)
    {
        try
        {
            updateAction(_currentSettings);
            _logger.LogDebug("[SettingsService] Settings updated in buffer: {Settings}", 
                JsonSerializer.Serialize(_currentSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Error updating settings (buffered)");
            throw;
        }
    }

    /// <summary>
    /// Сохраняет текущие буферизированные настройки в файл.
    /// </summary>
    public void SaveBufferedSettings()
    {
        SaveSettings(_currentSettings);
    }

    /// <summary>
    /// Загружает настройки из файла.
    /// </summary>
    /// <returns>Загруженные настройки.</returns>
    public WidgetSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return WidgetSettings.ValidateSettings(new WidgetSettings());
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<WidgetSettings>(json) ?? new WidgetSettings();
            return WidgetSettings.ValidateSettings(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Ошибка при загрузке настроек");
            return WidgetSettings.ValidateSettings(new WidgetSettings());
        }
    }

    /// <summary>
    /// Сохраняет настройки в файл.
    /// </summary>
    /// <param name="settings">Настройки для сохранения.</param>
    public void SaveSettings(WidgetSettings settings)
    {
        try
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            // Валидируем настройки перед сохранением
            settings = WidgetSettings.ValidateSettings(settings);

            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsPath, json);
            _logger.LogDebug("[SettingsService] Настройки успешно сохранены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Ошибка при сохранении настроек");
            throw;
        }
    }

    /// <summary>
    /// Сохраняет коллекции таймеров и будильников в отдельный файл.
    /// </summary>
    public void SaveTimersAndAlarms(TimersAndAlarmsPersistModel model)
    {
        try
        {
            var directory = Path.GetDirectoryName(_timersAlarmsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_timersAlarmsPath, json);
            _logger.LogDebug("[SettingsService] Timers and alarms saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Ошибка при сохранении timers/alarms");
            throw;
        }
    }

    /// <summary>
    /// Загружает коллекции таймеров и будильников из файла.
    /// </summary>
    public TimersAndAlarmsPersistModel? LoadTimersAndAlarms()
    {
        try
        {
            if (!File.Exists(_timersAlarmsPath))
                return null;
            var json = File.ReadAllText(_timersAlarmsPath);
            var model = JsonSerializer.Deserialize<TimersAndAlarmsPersistModel>(json);
            _logger.LogDebug("[SettingsService] Timers and alarms loaded");
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Ошибка при загрузке timers/alarms");
            return null;
        }
    }
} 