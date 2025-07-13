namespace ClockWidgetApp.ViewModels
{
    using System.ComponentModel;
    using System.Windows.Input;
    using Helpers;
    using Services;

    /// <summary>
    /// ViewModel для окна уведомления о сработавшем таймере или будильнике.
    /// Вся логика управления звуком, локализацией и командами вынесена из View.
    /// </summary>
    public class TimerNotificationViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Handle для управления воспроизведением звука.
        /// </summary>
        private readonly ISoundHandle _soundHandle;
        /// <summary>
        /// Описание или имя таймера/будильника.
        /// </summary>
        private string? TimerDescription { get; }
        /// <summary>
        /// Тип уведомления: "timer" или "alarm".
        /// </summary>
        public string NotificationType { get; }

        /// <summary>
        /// Первая строка — имя таймера/будильника.
        /// </summary>
        public string NameLine { get; }
        /// <summary>
        /// Вторая строка — время срабатывания.
        /// </summary>
        public string TimeLine { get; }

        /// <summary>
        /// Локализованные строки для окна.
        /// </summary>
        private LocalizedStrings _localized = LocalizationManager.GetLocalizedStrings();
        public LocalizedStrings Localized
        {
            get => _localized;
            private set { _localized = value; OnPropertyChanged(nameof(Localized)); OnPropertyChanged(nameof(TitleText)); }
        }

        /// <summary>
        /// Локализованный заголовок окна.
        /// </summary>
        public string TitleText => NotificationType == "alarm" ? Localized.AlarmNotification_Title : Localized.TimerNotification_Title;

        /// <summary>
        /// Текст времени для отображения в окне.
        /// </summary>
        public string TimeText => TimerDescription ?? string.Empty;

        /// <summary>
        /// Команда для остановки звука и закрытия окна.
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Конструктор ViewModel для окна уведомления.
        /// </summary>
        /// <param name="soundHandle">Handle для управления звуком.</param>
        /// <param name="description">Описание или имя таймера/будильника.</param>
        /// <param name="notificationType">Тип: "timer" или "alarm".</param>
        public TimerNotificationViewModel(ISoundHandle soundHandle, string description, string notificationType = "timer")
        {
            _soundHandle = soundHandle;
            NotificationType = notificationType;
            StopCommand = new RelayCommand(Stop);
            // Парсим description на две строки
            var lines = description.Split('\n');
            NameLine = lines.Length > 0 ? lines[0] : string.Empty;
            TimeLine = lines.Length > 1 ? lines[1] : string.Empty;
            TimerDescription = description;
            LocalizationManager.LanguageChanged += (_, _) => Localized = LocalizationManager.GetLocalizedStrings();
        }

        /// <summary>
        /// Останавливает звук и инициирует закрытие окна (для ICommand).
        /// </summary>
        private void Stop(object? obj)
        {
            Stop();
        }

        /// <summary>
        /// Останавливает звук и выполняет всю бизнес-логику завершения уведомления. Закрытие окна должно происходить только из View.
        /// </summary>
        private void Stop()
        {
            _soundHandle.Stop();
            // _closeWindowCallback?.Invoke(); // Закрытие окна теперь только из View
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 