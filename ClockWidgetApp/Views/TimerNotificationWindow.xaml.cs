using System.Windows;
using ClockWidgetApp.ViewModels;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.Views
{
    /// <summary>
    /// Окно оповещения о сработавшем таймере или будильнике.
    /// </summary>
    public partial class TimerNotificationWindow : Window
    {
        /// <summary>
        /// Конструктор окна, принимающий ViewModel.
        /// </summary>
        /// <param name="viewModel">ViewModel для окна уведомления.</param>
        public TimerNotificationWindow(TimerNotificationViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        /// <summary>
        /// Метод для показа окна с корректным callback закрытия.
        /// </summary>
        /// <param name="soundHandle">Handle для управления звуком.</param>
        /// <param name="description">Описание или имя таймера/будильника.</param>
        /// <param name="notificationType">Тип: "timer" или "alarm".</param>
        /// <returns>Экземпляр окна уведомления.</returns>
        public static TimerNotificationWindow CreateWithCloseCallback(ISoundHandle soundHandle, string description, string notificationType = "timer")
        {
            TimerNotificationWindow? window = null;
            var viewModel = new TimerNotificationViewModel(soundHandle, description, notificationType, () => window?.Close());
            window = new TimerNotificationWindow(viewModel);
            return window;
        }
    }
} 