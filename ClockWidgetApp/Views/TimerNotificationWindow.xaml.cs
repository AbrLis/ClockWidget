using System.Windows;
using ClockWidgetApp.Services;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.Views
{
    /// <summary>
    /// Окно оповещения о сработавшем таймере или будильнике.
    /// </summary>
    public partial class TimerNotificationWindow : Window
    {
        /// <summary>
        /// Handle для управления воспроизведением звука этого окна.
        /// </summary>
        private readonly ISoundHandle _soundHandle;
        /// <summary>
        /// Описание или имя таймера или будильника.
        /// </summary>
        private readonly string _timerDescription;
        /// <summary>
        /// Тип уведомления: "timer" или "alarm".
        /// </summary>
        private readonly string _notificationType;

        /// <summary>
        /// Локализованные строки для окна.
        /// </summary>
        public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

        /// <summary>
        /// Локализованный заголовок окна.
        /// </summary>
        public string TitleText => _notificationType == "alarm" ? Localized.AlarmNotification_Title : Localized.TimerNotification_Title;

        /// <summary>
        /// Текст времени для отображения в окне.
        /// </summary>
        public string TimeText => _timerDescription;

        /// <summary>
        /// Конструктор окна оповещения о сработавшем таймере или будильнике.
        /// </summary>
        /// <param name="soundHandle">Handle для управления звуком.</param>
        /// <param name="description">Описание или имя таймера/будильника.</param>
        /// <param name="notificationType">Тип: "timer" или "alarm".</param>
        public TimerNotificationWindow(ISoundHandle soundHandle, string description, string notificationType = "timer")
        {
            InitializeComponent();
            _soundHandle = soundHandle;
            _timerDescription = description;
            _notificationType = notificationType;
            this.DataContext = this;
            LocalizationManager.LanguageChanged += (s, e) =>
            {
                Localized = LocalizationManager.GetLocalizedStrings();
                OnPropertyChanged(nameof(Localized));
                OnPropertyChanged(nameof(TitleText));
            };
        }

        /// <summary>
        /// Обработчик кнопки Stop. Останавливает звук и закрывает окно.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _soundHandle.Stop();
            this.Close();
        }

        /// <summary>
        /// Уведомляет об изменении свойства для биндинга.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
} 