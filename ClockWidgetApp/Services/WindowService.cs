using Microsoft.Extensions.DependencyInjection;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.Services
{
    /// <summary>
    /// Сервис для централизованного управления всеми окнами приложения (открытие, скрытие, активация, получение экземпляров).
    /// Гарантирует единственность экземпляров окон, скрытие вместо закрытия, и централизованный доступ.
    /// Вся работа с окнами должна осуществляться только через этот сервис.
    /// </summary>
    public class WindowService : IWindowService
    {
        #region Private Fields
        /// <summary>Экземпляр главного окна.</summary>
        private MainWindow? _mainWindow = null;
        /// <summary>Экземпляр окна аналоговых часов.</summary>
        private AnalogClockWindow? _analogClockWindow = null;
        /// <summary>Экземпляр окна настроек.</summary>
        private SettingsWindow? _settingsWindow = null;
        #endregion

        #region Открытие и скрытие окон
        /// <inheritdoc/>
        public void OpenMainWindow()
        {
            if (_mainWindow == null)
            {
                // Получаем зависимости через DI
                var services = ((App)System.Windows.Application.Current).Services;
                var viewModel = services.GetRequiredService<ClockWidgetApp.ViewModels.MainWindowViewModel>();
                var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MainWindow>>();
                _mainWindow = new MainWindow(viewModel, logger);
                // При попытке закрытия окна — скрываем его, не уничтожая
                _mainWindow.Closing += (s, e) => { e.Cancel = true; _mainWindow.Hide(); };
                _mainWindow.Closed += (s, e) => _mainWindow = null;
                _mainWindow.Show();
            }
            else
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            }
        }

        /// <summary>
        /// Скрывает главное окно (MainWindow), не уничтожая его.
        /// </summary>
        public void HideMainWindow()
        {
            _mainWindow?.Hide();
        }

        /// <inheritdoc/>
        public void OpenAnalogClockWindow()
        {
            if (_analogClockWindow == null)
            {
                // Получаем зависимости через DI
                var services = ((App)System.Windows.Application.Current).Services;
                var analogVm = services.GetRequiredService<ClockWidgetApp.ViewModels.AnalogClockViewModel>();
                var mainVm = services.GetRequiredService<ClockWidgetApp.ViewModels.MainWindowViewModel>();
                var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AnalogClockWindow>>();
                _analogClockWindow = new AnalogClockWindow(analogVm, mainVm, logger);
                _analogClockWindow.Width = mainVm.AnalogClockSize;
                _analogClockWindow.Height = mainVm.AnalogClockSize;
                _analogClockWindow.Topmost = mainVm.AnalogClockTopmost;
                // При попытке закрытия окна — скрываем его, не уничтожая
                _analogClockWindow.Closing += (s, e) => { e.Cancel = true; _analogClockWindow.Hide(); };
                _analogClockWindow.Closed += (s, e) => _analogClockWindow = null;
                _analogClockWindow.Show();
            }
            else
            {
                _analogClockWindow.Show();
                _analogClockWindow.Activate();
            }
        }

        /// <summary>
        /// Скрывает окно аналоговых часов (AnalogClockWindow), не уничтожая его.
        /// </summary>
        public void HideAnalogClockWindow()
        {
            _analogClockWindow?.Hide();
        }

        /// <inheritdoc/>
        public void OpenSettingsWindow()
        {
            // Если окно ещё не создано (или не внедрено) — создаём его
            if (_settingsWindow == null)
            {
                var services = ((App)System.Windows.Application.Current).Services;
                var settingsVm = services.GetRequiredService<ClockWidgetApp.ViewModels.SettingsWindowViewModel>();
                var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SettingsWindow>>();
                _settingsWindow = new SettingsWindow(settingsVm, logger);
                // При попытке закрытия окна — скрываем его, не уничтожая
                _settingsWindow.Closing += (s, e) => { e.Cancel = true; _settingsWindow.Hide(); };
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
            // Устанавливаем флаг в MainWindow
            if (_settingsWindow != null && System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                /// <summary>
                /// Устанавливает флаг открытия окна настроек во ViewModel главного окна.
                /// </summary>
                if (mainWindow.ViewModel != null)
                    mainWindow.ViewModel.IsSettingsWindowOpen = true;
            }
        }

        /// <summary>
        /// Скрывает окно настроек (SettingsWindow), не уничтожая его.
        /// </summary>
        public void HideSettingsWindow()
        {
            _settingsWindow?.Hide();
        }
        #endregion

        #region Получение экземпляров окон
        /// <summary>Возвращает текущий экземпляр главного окна (MainWindow), если он открыт.</summary>
        public MainWindow? GetMainWindow() => _mainWindow;

        /// <summary>Возвращает текущий экземпляр окна аналоговых часов (AnalogClockWindow), если он открыт.</summary>
        public AnalogClockWindow? GetAnalogClockWindow() => _analogClockWindow;

        /// <summary>Возвращает текущий экземпляр окна настроек (SettingsWindow), если он открыт.</summary>
        public SettingsWindow? GetSettingsWindow() => _settingsWindow;
        #endregion

        #region Прочее
        /// <summary>Активирует все окна приложения (выводит на передний план).</summary>
        public void BringAllToFront()
        {
            void BringToFront(System.Windows.Window? window)
            {
                if (window == null || !window.IsVisible)
                    return;
                window.Show();
                bool wasTopmost = window.Topmost;
                window.Topmost = true;
                window.Activate();
                window.Topmost = wasTopmost;
            }
            BringToFront(_mainWindow);
            BringToFront(_analogClockWindow);
            BringToFront(_settingsWindow);
        }

        /// <summary>Открывает окно настроек с выбором вкладки.</summary>
        public void OpenSettingsWindow(bool selectTimersTab)
        {
            OpenSettingsWindow();
            if (_settingsWindow != null)
            {
                var vm = _settingsWindow.ViewModel;
                if (vm != null)
                    vm.SelectedTabIndex = selectTimersTab ? Constants.SETTINGS_TAB_INDEX_TIMERS : Constants.SETTINGS_TAB_INDEX_GENERAL;
            }
        }

        /// <summary>Внедряет заранее созданный экземпляр окна настроек.</summary>
        public void SetSettingsWindow(SettingsWindow window)
        {
            _settingsWindow = window;
        }
        #endregion
    }
} 