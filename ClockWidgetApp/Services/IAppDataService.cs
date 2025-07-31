namespace ClockWidgetApp.Services
{
    using System;
    using Models;

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
        /// Планирует автосохранение timers/alarms с debounce.
        /// </summary>
        void ScheduleTimersAndAlarmsSave();

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

        /// <summary>
        /// Синхронно отменяет все отложенные автосохранения и немедленно сохраняет все данные приложения.
        /// Используется при закрытии приложения для гарантированного сохранения.
        /// </summary>
        void FlushPendingSaves();
    }
}