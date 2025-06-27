namespace ClockWidgetApp.Services
{
    /// <summary>
    /// Интерфейс сервиса для управления открытием и закрытием окон приложения.
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Открывает главное окно приложения.
        /// </summary>
        void OpenMainWindow();

        /// <summary>
        /// Скрывает главное окно приложения (MainWindow), не уничтожая его.
        /// </summary>
        void HideMainWindow();

        /// <summary>
        /// Открывает окно аналоговых часов.
        /// </summary>
        void OpenAnalogClockWindow();

        /// <summary>
        /// Скрывает окно аналоговых часов (AnalogClockWindow), не уничтожая его.
        /// </summary>
        void HideAnalogClockWindow();

        /// <summary>
        /// Открывает окно настроек.
        /// </summary>
        void OpenSettingsWindow();

        /// <summary>
        /// Скрывает окно настроек (SettingsWindow), не уничтожая его.
        /// </summary>
        void HideSettingsWindow();

        /// <summary>
        /// Возвращает текущий экземпляр главного окна (MainWindow), если он открыт.
        /// </summary>
        MainWindow? GetMainWindow();

        /// <summary>
        /// Возвращает текущий экземпляр окна аналоговых часов (AnalogClockWindow), если он открыт.
        /// </summary>
        AnalogClockWindow? GetAnalogClockWindow();

        /// <summary>
        /// Возвращает текущий экземпляр окна настроек (SettingsWindow), если он открыт.
        /// </summary>
        SettingsWindow? GetSettingsWindow();
    }
} 