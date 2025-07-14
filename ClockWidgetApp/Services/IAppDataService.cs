namespace ClockWidgetApp.Services
{
    using System;
    using ClockWidgetApp.Models;

    /// <summary>
    /// Интерфейс единого сервиса управления всеми данными приложения (настройки, таймеры, будильники).
    /// </summary>
    public interface IAppDataService
    {
        /// <summary>
        /// Все данные приложения (настройки, таймеры, будильники, длинные таймеры).
        /// </summary>
        AppDataModel Data { get; }

        /// <summary>
        /// Событие изменения настроек виджета.
        /// </summary>
        event EventHandler SettingsChanged;

        /// <summary>
        /// Событие изменения коллекции таймеров.
        /// </summary>
        event EventHandler TimersChanged;

        /// <summary>
        /// Событие изменения коллекции будильников.
        /// </summary>
        event EventHandler AlarmsChanged;

        /// <summary>
        /// Событие изменения коллекции длинных таймеров.
        /// </summary>
        event EventHandler LongTimersChanged;

        /// <summary>
        /// Уведомляет об изменении настроек.
        /// </summary>
        void NotifySettingsChanged();

        /// <summary>
        /// Загружает все данные приложения из файла/файлов.
        /// </summary>
        void Load();

        /// <summary>
        /// Асинхронно загружает все данные приложения из файла/файлов.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Сохраняет все данные приложения в файл/файлы.
        /// </summary>
        void Save();

        /// <summary>
        /// Асинхронно сохраняет все данные приложения в файл/файлы.
        /// </summary>
        Task SaveAsync();
    }
}