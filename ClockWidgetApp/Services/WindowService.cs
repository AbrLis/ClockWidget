using Microsoft.Extensions.DependencyInjection;

namespace ClockWidgetApp.Services
{
    /// <summary>
    /// Сервис для управления открытием и закрытием окон приложения.
    /// </summary>
    public class WindowService : IWindowService
    {
        private MainWindow? _mainWindow = null;
        private AnalogClockWindow? _analogClockWindow = null;
        private SettingsWindow? _settingsWindow = null;

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
            if (_settingsWindow == null)
            {
                // Получаем зависимости через DI
                var services = ((App)System.Windows.Application.Current).Services;
                var settingsVm = services.GetRequiredService<ClockWidgetApp.ViewModels.SettingsWindowViewModel>();
                var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SettingsWindow>>();
                _settingsWindow = new SettingsWindow(settingsVm, logger);
                // При попытке закрытия окна — скрываем его, не уничтожая
                _settingsWindow.Closing += (s, e) => { e.Cancel = true; _settingsWindow.Hide(); };
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Show(); // Показываем окно, если оно было скрыто
                _settingsWindow.Activate();
            }
            // Устанавливаем флаг в MainWindow
            if (_settingsWindow != null && System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                mainWindow.IsSettingsWindowOpen = true;
        }

        /// <summary>
        /// Скрывает окно настроек (SettingsWindow), не уничтожая его.
        /// </summary>
        public void HideSettingsWindow()
        {
            _settingsWindow?.Hide();
        }

        /// <summary>
        /// Возвращает текущий экземпляр главного окна (MainWindow), если он открыт.
        /// </summary>
        public MainWindow? GetMainWindow() => _mainWindow;

        /// <summary>
        /// Возвращает текущий экземпляр окна аналоговых часов (AnalogClockWindow), если он открыт.
        /// </summary>
        public AnalogClockWindow? GetAnalogClockWindow() => _analogClockWindow;

        /// <summary>
        /// Возвращает текущий экземпляр окна настроек (SettingsWindow), если он открыт.
        /// </summary>
        public SettingsWindow? GetSettingsWindow() => _settingsWindow;

        /// <summary>
        /// Активирует все окна приложения (выводит на передний план).
        /// </summary>
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
    }
} 