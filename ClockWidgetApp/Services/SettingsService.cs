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
    /// Позволяет явно задать путь к файлам настроек и таймеров/будильников (для тестирования).
    /// </summary>
    public SettingsService(ILogger<SettingsService> logger, string? settingsPath = null, string? timersAlarmsPath = null)
    {
        _logger = logger;
        if (string.IsNullOrEmpty(settingsPath) || string.IsNullOrEmpty(timersAlarmsPath))
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
                _logger.LogDebug("[SettingsService] Created settings directory: {Path}", appDataPath);
            }
            _settingsPath = settingsPath ?? Path.Combine(appDataPath, Constants.FileSettings.SETTINGS_FILENAME);
            _timersAlarmsPath = timersAlarmsPath ?? Path.Combine(appDataPath, "timers_alarms.json");
        }
        else
        {
            _settingsPath = settingsPath;
            _timersAlarmsPath = timersAlarmsPath;
            var settingsDir = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(settingsDir) && !Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);
            var timersDir = Path.GetDirectoryName(_timersAlarmsPath);
            if (!string.IsNullOrEmpty(timersDir) && !Directory.Exists(timersDir))
                Directory.CreateDirectory(timersDir);
            _logger.LogDebug("[SettingsService] (Custom/Test) Settings file path: {Path}", _settingsPath);
        }
        _currentSettings = LoadSettings();
        _logger.LogInformation("[SettingsService] Settings loaded");
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
            App.MarkWidgetSettingsDirty();
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
            {
                _logger.LogWarning("[SettingsService] Settings file not found. Using defaults.");
                return WidgetSettings.ValidateSettings(new WidgetSettings());
            }
            var json = File.ReadAllText(_settingsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("[SettingsService] Settings file is empty. Using defaults.");
                return WidgetSettings.ValidateSettings(new WidgetSettings());
            }
            var settings = JsonSerializer.Deserialize<WidgetSettings>(json) ?? new WidgetSettings();
            return WidgetSettings.ValidateSettings(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Error loading settings");
            return WidgetSettings.ValidateSettings(new WidgetSettings());
        }
    }

    /// <summary>
    /// Сохраняет настройки в файл через временный файл для предотвращения потери данных.
    /// Перед сохранением предыдущий файл (если есть) переименовывается в .bak для резервного копирования.
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
            // Создаём резервную копию предыдущего файла
            if (File.Exists(_settingsPath))
            {
                string backupPath = Path.ChangeExtension(_settingsPath, ".bak");
                try
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(_settingsPath, backupPath);
                    _logger.LogInformation("[SettingsService] Settings backup created: {Path}", backupPath);
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "[SettingsService] Error creating settings backup");
                }
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            var tempFile = _settingsPath + ".tmp";
            File.WriteAllText(tempFile, json);
            File.Copy(tempFile, _settingsPath, true);
            File.Delete(tempFile);
            _logger.LogInformation("[SettingsService] Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Error saving settings");
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
            _logger.LogInformation("[SettingsService] Timers and alarms saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Error saving timers/alarms");
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
            {
                _logger.LogWarning("[SettingsService] Timers/alarms file not found");
                return null;
            }
            var json = File.ReadAllText(_timersAlarmsPath);
            var model = JsonSerializer.Deserialize<TimersAndAlarmsPersistModel>(json);
            _logger.LogInformation("[SettingsService] Timers and alarms loaded");
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsService] Error loading timers/alarms");
            return null;
        }
    }
    #endregion
} 