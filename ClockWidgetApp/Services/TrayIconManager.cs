using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClockWidgetApp.Services
{
    /// <summary>
    /// Сервис для управления иконками трея для активных таймеров и будильников.
    /// </summary>
    public class TrayIconManager : IDisposable
    {
        #region Private fields
        /// <summary>
        /// Словарь активных иконок трея по идентификатору таймера/будильника.
        /// </summary>
        private readonly Dictionary<string, TrayIconInfo> _trayIcons = new();
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
            /// Уникальный идентификатор таймера или будильника.
            /// </summary>
            public string? Id { get; set; }
            /// <summary>
            /// Ссылка на пункт меню 'Стоп' для динамического обновления текста.
            /// </summary>
            public ToolStripMenuItem? StopMenuItem { get; set; }
        }
        #endregion

        #region Events
        /// <summary>
        /// Событие при выборе "Стоп" в контекстном меню иконки трея.
        /// </summary>
        public event Action<string>? StopRequested;
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
            if (_trayIcons.ContainsKey(id))
            {
                // Обновляем тултип
                var icon = _trayIcons[id].Icon;
                if (icon != null)
                    icon.Text = tooltip;
                return;
            }

            var contextMenu = new ContextMenuStrip();
            var stopText = Helpers.LocalizationManager.GetString("Tray_Stop");
            var stopItem = new ToolStripMenuItem(stopText);
            stopItem.Click += (s, e) => StopRequested?.Invoke(id);
            contextMenu.Items.Add(stopItem);

            var notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(iconPath),
                Text = tooltip,
                Visible = true,
                ContextMenuStrip = contextMenu
            };

            _trayIcons[id] = new TrayIconInfo { Id = id, Icon = notifyIcon, StopMenuItem = stopItem };
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
                info.Icon.Text = tooltip;
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
            Helpers.LocalizationManager.LanguageChanged += (s, e) => UpdateMenuLanguage();
        }
        #endregion
    }
} 