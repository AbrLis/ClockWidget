using System.Windows;
using ClockWidgetApp.Services;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp.Views
{
    /// <summary>
    /// Окно оповещения о сработавшем таймере.
    /// </summary>
    public partial class TimerNotificationWindow : Window
    {
        /// <summary>
        /// Handle для управления воспроизведением звука этого окна.
        /// </summary>
        private readonly ISoundHandle _soundHandle;
        /// <summary>
        /// Описание или имя таймера.
        /// </summary>
        private readonly string _timerDescription;

        /// <summary>
        /// Локализованные строки для окна.
        /// </summary>
        public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

        /// <summary>
        /// Текст описания таймера для вывода.
        /// </summary>
        public string DescriptionText => $"{Localized.TimerNotification_Description} {_timerDescription}";

        /// <summary>
        /// Конструктор окна оповещения о сработавшем таймере.
        /// </summary>
        /// <param name="soundHandle">Handle для управления звуком.</param>
        /// <param name="timerDescription">Описание или имя таймера.</param>
        public TimerNotificationWindow(ISoundHandle soundHandle, string timerDescription)
        {
            InitializeComponent();
            _soundHandle = soundHandle;
            _timerDescription = timerDescription;
            this.DataContext = this;
            LocalizationManager.LanguageChanged += (s, e) =>
            {
                Localized = LocalizationManager.GetLocalizedStrings();
                OnPropertyChanged(nameof(Localized));
                OnPropertyChanged(nameof(DescriptionText));
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