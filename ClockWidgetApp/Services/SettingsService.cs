using System;
using System.IO;
using System.Text.Json;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для работы с настройками виджета.
/// Обеспечивает сохранение и загрузку настроек в JSON-файл.
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private WidgetSettings _currentSettings;
    private readonly ILogger<SettingsService> _logger = LoggingService.CreateLogger<SettingsService>();

    /// <summary>
    /// Получает текущие настройки виджета.
    /// </summary>
    public WidgetSettings CurrentSettings => _currentSettings;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SettingsService"/>.
    /// Загружает настройки из файла или создает настройки по умолчанию.
    /// </summary>
    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockWidget");
            
        // Создаем папку, если она не существует
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
            _logger.LogInformation("Created settings directory: {Path}", appDataPath);
        }

        _settingsPath = Path.Combine(
            appDataPath,
            Constants.FileSettings.SETTINGS_FILENAME);
            
        _logger.LogInformation("Settings file path: {Path}", _settingsPath);
        _currentSettings = LoadSettings();
    }

    /// <summary>
    /// Обновляет настройки с помощью указанного действия и сохраняет их в файл.
    /// </summary>
    /// <param name="updateAction">Действие для обновления настроек.</param>
    public void UpdateSettings(Action<WidgetSettings> updateAction)
    {
        try
        {
            _logger.LogInformation("Updating settings");
            updateAction(_currentSettings);
            SaveSettings();
            _logger.LogInformation("Settings updated successfully: {Settings}", 
                JsonSerializer.Serialize(_currentSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            throw;
        }
    }

    /// <summary>
    /// Загружает настройки из файла.
    /// Если файл не существует или поврежден, возвращает настройки по умолчанию.
    /// </summary>
    /// <returns>Загруженные настройки или настройки по умолчанию.</returns>
    private WidgetSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                _logger.LogInformation("Loading settings from file: {Path}", _settingsPath);
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<WidgetSettings>(json);
                
                if (settings != null)
                {
                    _logger.LogInformation("Settings loaded successfully: {Settings}", json);
                    return settings;
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize settings, using defaults");
                }
            }
            else
            {
                _logger.LogInformation("Settings file not found, using defaults");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings from file: {Path}", _settingsPath);
        }
        
        var defaultSettings = new WidgetSettings();
        _logger.LogInformation("Using default settings: {Settings}", 
            JsonSerializer.Serialize(defaultSettings));
        return defaultSettings;
    }

    /// <summary>
    /// Сохраняет текущие настройки в файл.
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            _logger.LogInformation("Saving settings to file: {Path}", _settingsPath);
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            _logger.LogInformation("Settings saved successfully: {Settings}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings to file: {Path}", _settingsPath);
            throw;
        }
    }
} 