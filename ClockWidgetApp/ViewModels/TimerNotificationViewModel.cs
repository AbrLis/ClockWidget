using System.ComponentModel;
using System.Windows.Input;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels
{
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
        public string TimerDescription { get; }
        /// <summary>
        /// Тип уведомления: "timer" или "alarm".
        /// </summary>
        public string NotificationType { get; }

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
        public string TimeText => TimerDescription;

        /// <summary>
        /// Команда для остановки звука и закрытия окна.
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Callback для запроса закрытия окна из ViewModel.
        /// </summary>
        private readonly Action? _closeWindowCallback;

        /// <summary>
        /// Конструктор ViewModel для окна уведомления.
        /// </summary>
        /// <param name="soundHandle">Handle для управления звуком.</param>
        /// <param name="description">Описание или имя таймера/будильника.</param>
        /// <param name="notificationType">Тип: "timer" или "alarm".</param>
        /// <param name="closeWindowCallback">Callback для закрытия окна.</param>
        public TimerNotificationViewModel(ISoundHandle soundHandle, string description, string notificationType = "timer", Action? closeWindowCallback = null)
        {
            _soundHandle = soundHandle;
            TimerDescription = description;
            NotificationType = notificationType;
            _closeWindowCallback = closeWindowCallback;
            StopCommand = new RelayCommand(Stop);
            LocalizationManager.LanguageChanged += (s, e) =>
            {
                Localized = LocalizationManager.GetLocalizedStrings();
            };
        }

        /// <summary>
        /// Останавливает звук и инициирует закрытие окна (для ICommand).
        /// </summary>
        private void Stop(object? obj)
        {
            Stop();
        }

        /// <summary>
        /// Останавливает звук и инициирует закрытие окна (без параметров).
        /// </summary>
        private void Stop()
        {
            _soundHandle.Stop();
            _closeWindowCallback?.Invoke();
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 