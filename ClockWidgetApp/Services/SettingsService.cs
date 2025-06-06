using System;
using System.IO;
using System.Text.Json;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для работы с настройками виджета.
/// Обеспечивает сохранение и загрузку настроек в JSON-файл.
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private WidgetSettings _currentSettings;

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
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Constants.FileSettings.SETTINGS_FILENAME);
        _currentSettings = LoadSettings();
    }

    /// <summary>
    /// Обновляет настройки с помощью указанного действия и сохраняет их в файл.
    /// </summary>
    /// <param name="updateAction">Действие для обновления настроек.</param>
    public void UpdateSettings(Action<WidgetSettings> updateAction)
    {
        updateAction(_currentSettings);
        SaveSettings();
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
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<WidgetSettings>(json) ?? new WidgetSettings();
            }
        }
        catch (Exception)
        {
            // В случае ошибки возвращаем настройки по умолчанию
        }
        return new WidgetSettings();
    }

    /// <summary>
    /// Сохраняет текущие настройки в файл.
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception)
        {
            // Игнорируем ошибки сохранения
        }
    }
} 