using ClockWidgetApp.Models;

namespace ClockWidgetApp.Services
{
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
        /// Загружает все данные приложения из файла/файлов.
        /// </summary>
        void Load();

        /// <summary>
        /// Сохраняет все данные приложения в файл/файлы.
        /// </summary>
        void Save();
    }
} 