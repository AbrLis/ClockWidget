namespace ClockWidgetApp.Services
{
    /// <summary>
    /// Интерфейс сервиса для централизованного управления всеми окнами приложения (открытие, скрытие, активация, получение экземпляров).
    /// Вся работа с окнами должна осуществляться только через этот сервис.
    /// </summary>
    public interface IWindowService
    {
        #region Открытие и скрытие окон
        /// <summary>Открывает главное окно приложения (цифровые часы), если оно ещё не открыто, либо активирует его.</summary>
        void OpenMainWindow();
        /// <summary>Скрывает главное окно приложения (MainWindow), не уничтожая его. Повторный показ возможен через OpenMainWindow().</summary>
        void HideMainWindow();
        /// <summary>Открывает окно аналоговых часов, если оно ещё не открыто, либо активирует его.</summary>
        void OpenAnalogClockWindow();
        /// <summary>Скрывает окно аналоговых часов (AnalogClockWindow), не уничтожая его. Повторный показ возможен через OpenAnalogClockWindow().</summary>
        void HideAnalogClockWindow();
        /// <summary>Открывает окно настроек, если оно ещё не открыто, либо активирует его.</summary>
        void OpenSettingsWindow();
        /// <summary>Скрывает окно настроек (SettingsWindow), не уничтожая его. Повторный показ возможен через OpenSettingsWindow().</summary>
        void HideSettingsWindow();
        #endregion

        #region Получение экземпляров окон
        /// <summary>Возвращает текущий экземпляр главного окна (MainWindow), если он существует, иначе null.</summary>
        MainWindow? GetMainWindow();
        /// <summary>Возвращает текущий экземпляр окна аналоговых часов (AnalogClockWindow), если он существует, иначе null.</summary>
        AnalogClockWindow? GetAnalogClockWindow();
        /// <summary>Возвращает текущий экземпляр окна настроек (SettingsWindow), если он существует, иначе null.</summary>
        SettingsWindow? GetSettingsWindow();
        #endregion

        #region Прочее
        /// <summary>Активирует все окна приложения (выводит на передний план все открытые окна).</summary>
        void BringAllToFront();
        #endregion
    }
} 