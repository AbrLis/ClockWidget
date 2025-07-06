using System;
using System.IO;
using System.Text.Json;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для управления настройками виджета. Сохраняет и загружает настройки в JSON-файл.
/// </summary>
public class SettingsService : ISettingsService
{
    #region Private fields
    /// <summary>Путь к файлу настроек.</summary>
    private readonly string _settingsPath;
    /// <summary>Путь к файлу таймеров и будильников.</summary>
    private readonly string _timersAlarmsPath;
    /// <summary>Текущие настройки виджета.</summary>
    private WidgetSettings _currentSettings;
    /// <summary>Логгер для событий сервиса.</summary>
    private readonly ILogger<SettingsService> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SettingsService"/> и загружает настройки из файла или создает по умолчанию.
    /// </summary>
    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockWidget");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
            _logger.LogDebug("[SettingsService] Created settings directory: {Path}", appDataPath);
        }
        _settingsPath = Path.Combine(appDataPath, Constants.FileSettings.SETTINGS_FILENAME);
        _timersAlarmsPath = Path.Combine(appDataPath, "timers_alarms.json");
        _logger.LogDebug("[SettingsService] Settings file path: {Path}", _settingsPath);
        _currentSettings = LoadSettings();
    }
    #endregion

    #region Public properties
    /// <summary>
    /// Получает текущие настройки виджета.
    /// </summary>
    public WidgetSettings CurrentSettings => _currentSettings;
    #endregion

    #region Public methods
    /// <summary>
    /// Обновляет настройки с помощью указанного действия. Сохраняет только в памяти (буфер), не на диск.
    /// </summary>
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
    /// Загружает настройки из файла. Если файл отсутствует или пустой, возвращает настройки по умолчанию.
    /// </summary>
    public WidgetSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return WidgetSettings.ValidateSettings(new WidgetSettings());
            var json = File.ReadAllText(_settingsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("[SettingsService] Файл настроек пустой. Используются настройки по умолчанию.");
                return WidgetSettings.ValidateSettings(new WidgetSettings());
            }
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
    /// Сохраняет настройки в файл через временный файл для предотвращения потери данных.
    /// </summary>
    public void SaveSettings(WidgetSettings settings)
    {
        try
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            settings = WidgetSettings.ValidateSettings(settings);
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            var tempFile = _settingsPath + ".tmp";
            File.WriteAllText(tempFile, json);
            File.Copy(tempFile, _settingsPath, true);
            File.Delete(tempFile);
            _logger.LogDebug("[SettingsService] Настройки успешно сохранены через временный файл");
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
    #endregion
} 