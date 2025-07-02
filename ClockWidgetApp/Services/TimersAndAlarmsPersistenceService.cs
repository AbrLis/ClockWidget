using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.Services;

/// <summary>
/// Сервис для сохранения и загрузки таймеров и будильников в/из файла.
/// </summary>
public class TimersAndAlarmsPersistenceService
{
    private readonly string _filePath;

    /// <summary>
    /// Создаёт сервис с указанным путём к файлу.
    /// </summary>
    /// <param name="filePath">Путь к файлу для хранения данных.</param>
    public TimersAndAlarmsPersistenceService(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Сохраняет данные таймеров и будильников в файл.
    /// </summary>
    /// <param name="persist">Модель для сохранения.</param>
    public void Save(TimersAndAlarmsPersistModel persist)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(persist, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Загружает данные таймеров и будильников из файла.
    /// </summary>
    /// <returns>Модель с таймерами и будильниками или null, если файл не найден.</returns>
    public TimersAndAlarmsPersistModel? Load()
    {
        if (!File.Exists(_filePath))
            return null;
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<TimersAndAlarmsPersistModel>(json);
    }
} 