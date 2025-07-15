namespace ClockWidgetApp.Services
{
    using System.IO;
    using ViewModels;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Сервис для управления иконками трея для активных таймеров, будильников и главного приложения.
    /// </summary>
    public class TrayIconManager : IDisposable
    {
        #region Private fields
        /// <summary>
        /// Словарь активных иконок трея по идентификатору таймера/будильника.
        /// </summary>
        private readonly Dictionary<string, TrayIconInfo> _trayIcons = new();

        // Главный NotifyIcon приложения и его меню
        private NotifyIcon? _mainNotifyIcon;
        private ContextMenuStrip? _mainTrayMenu;
        private ToolStripMenuItem? _showDigitalItem;
        private ToolStripMenuItem? _showAnalogItem;
        private ToolStripMenuItem? _settingsItem;
        private ToolStripMenuItem? _timerAlarmSettingsItem;
        private ToolStripMenuItem? _exitItem;
        private ILogger? _logger;
        private MainWindowViewModel? _mainViewModel;
        private IServiceProvider? _serviceProvider;
        #endregion

        #region Nested types
        /// <summary>
        /// Информация об иконке трея и её идентификаторе.
        /// </summary>
        private class TrayIconInfo
        {
            /// <summary>
            /// NotifyIcon, отображаемый в трее.
            /// </summary>
            public NotifyIcon? Icon { get; set; }

            /// <summary>
            /// Ссылка на пункт меню 'Стоп' для динамического обновления текста.
            /// </summary>
            public ToolStripMenuItem? StopMenuItem { get; init; }
        }
        #endregion

        #region Events
        /// <summary>
        /// Событие при выборе "Стоп" в контекстном меню иконки трея.
        /// </summary>
        public event Action<string>? StopRequested;
        #endregion

        #region Main tray icon logic
        /// <summary>
        /// Инициализирует главный NotifyIcon приложения и его меню.
        /// </summary>
        public void InitializeMainTrayIcon(MainWindowViewModel mainViewModel, IServiceProvider serviceProvider, ILogger logger)
        {
            _mainViewModel = mainViewModel;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _mainTrayMenu = new ContextMenuStrip();
            string lang = Helpers.LocalizationManager.CurrentLanguage;
            _showDigitalItem = new ToolStripMenuItem();
            _showAnalogItem = new ToolStripMenuItem();
            _settingsItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_Settings", lang));
            _timerAlarmSettingsItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_TimerAlarmSettings", lang));
            _exitItem = new ToolStripMenuItem(Helpers.LocalizationManager.GetString("Tray_Exit", lang));
            var separator = new ToolStripSeparator();
            var separator2 = new ToolStripSeparator();
            _showDigitalItem.Click += (_, _) =>
            {
                if (_mainViewModel != null)
                    _mainViewModel.ShowDigitalClock = !_mainViewModel.ShowDigitalClock;
            };
            _showAnalogItem.Click += (_, _) =>
            {
                if (_mainViewModel != null)
                    _mainViewModel.ShowAnalogClock = !_mainViewModel.ShowAnalogClock;
            };
            _settingsItem.Click += (_, _) =>
            {
                if (_serviceProvider != null)
                {
                    var ws = _serviceProvider.GetService(typeof(IWindowService)) as IWindowService;
                    if (ws is WindowService windowService)
                        windowService.OpenSettingsWindow(false);
                    else
                        ws?.OpenSettingsWindow();
                }
            };
            _timerAlarmSettingsItem.Click += (_, _) =>
            {
                if (_serviceProvider == null) return;
                var ws = _serviceProvider.GetService(typeof(IWindowService)) as IWindowService;
                if (ws is WindowService windowService)
                    windowService.OpenSettingsWindow(true);
                else
                    ws?.OpenSettingsWindow();
            };
            _exitItem.Click += (_, _) => System.Windows.Application.Current.Shutdown();
            _mainTrayMenu.Items.Add(_showDigitalItem);
            _mainTrayMenu.Items.Add(_showAnalogItem);
            _mainTrayMenu.Items.Add(separator2);
            _mainTrayMenu.Items.Add(_settingsItem);
            _mainTrayMenu.Items.Add(_timerAlarmSettingsItem);
            _mainTrayMenu.Items.Add(separator);
            _mainTrayMenu.Items.Add(_exitItem);
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "app.ico");
            if (!File.Exists(iconPath))
            {
                _logger?.LogWarning("[TrayIconManager] Tray icon file not found: {Path}", iconPath);
                return;
            }
            _mainNotifyIcon = new NotifyIcon();
            _mainNotifyIcon.Icon = new Icon(iconPath);
            _mainNotifyIcon.Visible = true;
            _mainNotifyIcon.Text = TrimTooltip(Helpers.LocalizationManager.GetString("Tray_IconText", lang));
            _mainNotifyIcon.ContextMenuStrip = _mainTrayMenu;
            _mainNotifyIcon.MouseUp += NotifyIcon_MouseUp;
            Helpers.LocalizationManager.LanguageChanged += (_, _) => UpdateMainTrayMenuItems();
        }

        /// <summary>
        /// Обновляет текст пунктов меню главного трея в зависимости от состояния приложения.
        /// </summary>
        public void UpdateMainTrayMenuItems()
        {
            if (_mainViewModel == null) return;
            string lang = Helpers.LocalizationManager.CurrentLanguage;
            if (_showDigitalItem != null)
            {
                _showDigitalItem.Text = _mainViewModel.ShowDigitalClock
                    ? Helpers.LocalizationManager.GetString("Tray_HideDigital", lang)
                    : Helpers.LocalizationManager.GetString("Tray_ShowDigital", lang);
            }
            if (_showAnalogItem != null)
            {
                _showAnalogItem.Text = _mainViewModel.ShowAnalogClock
                    ? Helpers.LocalizationManager.GetString("Tray_HideAnalog", lang)
                    : Helpers.LocalizationManager.GetString("Tray_ShowAnalog", lang);
            }
            if (_settingsItem != null)
                _settingsItem.Text = Helpers.LocalizationManager.GetString("Tray_Settings", lang);
            if (_timerAlarmSettingsItem != null)
                _timerAlarmSettingsItem.Text = Helpers.LocalizationManager.GetString("Tray_TimerAlarmSettings", lang);
            if (_exitItem != null)
                _exitItem.Text = Helpers.LocalizationManager.GetString("Tray_Exit", lang);
            if (_mainNotifyIcon != null)
                _mainNotifyIcon.Text = TrimTooltip(Helpers.LocalizationManager.GetString("Tray_IconText", lang));
        }

        /// <summary>
        /// Освобождает ресурсы главного NotifyIcon.
        /// </summary>
        public void DisposeMainTrayIcon()
        {
            if (_mainNotifyIcon != null)
            {
                _mainNotifyIcon.Visible = false;
                _mainNotifyIcon.Dispose();
                _mainNotifyIcon = null;
            }
            _mainTrayMenu = null;
            _showDigitalItem = null;
            _showAnalogItem = null;
            _settingsItem = null;
            _timerAlarmSettingsItem = null;
            _exitItem = null;
        }

        /// <summary>
        /// Обработчик клика по главной иконке в трее.
        /// </summary>
        private void NotifyIcon_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_mainViewModel == null) return;
            if (e.Button == MouseButtons.Right)
            {
                UpdateMainTrayMenuItems();
                _mainTrayMenu?.Show();
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(static () =>
                {
                    var ws = (System.Windows.Application.Current as App)?.Services.GetService(typeof(IWindowService)) as IWindowService;
                    ws?.BringAllToFront();
                });
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Добавляет или обновляет иконку трея для таймера или будильника.
        /// </summary>
        /// <param name="id">Уникальный идентификатор таймера или будильника.</param>
        /// <param name="iconPath">Путь к иконке (ico-файл).</param>
        /// <param name="tooltip">Начальный текст тултипа (оставшееся время).</param>
        public void AddOrUpdateTrayIcon(string id, string iconPath, string tooltip)
        {
            if (_trayIcons.TryGetValue(id, out var trayIcon))
            {
                // Обновляем тултип
                var icon = trayIcon.Icon;
                if (icon != null)
                    icon.Text = TrimTooltip(tooltip);
                return;
            }

            // Для иконки длинных таймеров не создаём меню
            ContextMenuStrip? contextMenu = null;
            if (id != "longtimers")
            {
                contextMenu = new ContextMenuStrip();
                string stopText = id.StartsWith("longtimer_")
                    ? Helpers.LocalizationManager.GetString("Tray_Stop")
                    : Helpers.LocalizationManager.GetString("Tray_StopTimer");
                var stopItem = new ToolStripMenuItem(stopText);
                stopItem.Click += (_, _) => {
                    if (id.StartsWith("longtimer_")) {
                        if (Helpers.DialogHelper.ConfirmLongTimerDelete())
                            StopRequested?.Invoke(id);
                    } else {
                        StopRequested?.Invoke(id);
                    }
                };
                contextMenu.Items.Add(stopItem);
            }

            var notifyIcon = new NotifyIcon
            {
                Icon = new Icon(iconPath),
                Text = TrimTooltip(tooltip),
                Visible = true,
                ContextMenuStrip = contextMenu
            };

            // Для иконки длинных таймеров подписываемся на MouseUp
            if (id == "longtimers")
            {
                notifyIcon.MouseUp += (_, e) =>
                {
                    if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                    {
                        // Открываем окно настроек таймеров (вкладка длинных таймеров)
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var ws = (System.Windows.Application.Current as App)?.Services.GetService(typeof(IWindowService)) as IWindowService;
                            if (ws is WindowService windowService)
                                windowService.OpenSettingsWindow(true); // true — вкладка таймеров (индекс 1)
                            else
                                ws?.OpenSettingsWindow();
                        });
                    }
                };
            }

            _trayIcons[id] = new TrayIconInfo { Icon = notifyIcon, StopMenuItem = null };
        }

        /// <summary>
        /// Обновляет тултип для иконки трея.
        /// </summary>
        /// <param name="id">Уникальный идентификатор таймера или будильника.</param>
        /// <param name="tooltip">Новый текст тултипа.</param>
        public void UpdateTooltip(string id, string tooltip)
        {
            if (_trayIcons.TryGetValue(id, out var info) && info.Icon != null)
            {
                info.Icon.Text = TrimTooltip(tooltip);
            }
        }

        /// <summary>
        /// Удаляет иконку трея по идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор таймера или будильника.</param>
        public void RemoveTrayIcon(string id)
        {
            if (_trayIcons.TryGetValue(id, out var info) && info.Icon != null)
            {
                info.Icon.Visible = false;
                info.Icon.Dispose();
                _trayIcons.Remove(id);
            }
        }

        /// <summary>
        /// Освобождает все ресурсы, связанные с NotifyIcon.
        /// </summary>
        public void Dispose()
        {
            foreach (var info in _trayIcons.Values)
            {
                if (info.Icon != null)
                    info.Icon.Dispose();
            }
            _trayIcons.Clear();
        }

        /// <summary>
        /// Обновляет текст пунктов меню 'Стоп' у всех иконок трея при смене языка.
        /// </summary>
        private void UpdateMenuLanguage()
        {
            var stopText = Helpers.LocalizationManager.GetString("Tray_Stop");
            foreach (var info in _trayIcons.Values)
            {
                if (info.StopMenuItem != null)
                    info.StopMenuItem.Text = stopText;
            }
        }
        #endregion

        #region Constructor
        public TrayIconManager()
        {
            Helpers.LocalizationManager.LanguageChanged += (_, _) => UpdateMenuLanguage();
        }
        #endregion

        /// <summary>
        /// Обрезает строку тултипа до максимально допустимой длины для NotifyIcon (127 символов).
        /// </summary>
        /// <param name="tooltip">Исходный текст тултипа.</param>
        /// <returns>Обрезанный текст тултипа.</returns>
        private static string TrimTooltip(string tooltip)
        {
            const int maxTooltipLength = 127;
            if (!string.IsNullOrEmpty(tooltip) && tooltip.Length > maxTooltipLength)
                return tooltip.Substring(0, maxTooltipLength - 3) + "...";
            return tooltip;
        }
    }
} 