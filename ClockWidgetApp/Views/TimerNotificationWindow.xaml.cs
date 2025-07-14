namespace ClockWidgetApp.Views
{
    using System.Windows;
    using ClockWidgetApp.Services;
    using ClockWidgetApp.ViewModels;

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
            // Подписка на событие закрытия окна
            this.Closing += TimerNotificationWindow_Closing;
            // Подписка на клик по кнопке Stop
        }

        /// <summary>
        /// Обработчик закрытия окна. При закрытии по крестику вызывает StopCommand из ViewModel.
        /// </summary>
        private void TimerNotificationWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DataContext is TimerNotificationViewModel vm)
            {
                if (vm.StopCommand.CanExecute(null))
                    vm.StopCommand.Execute(null);
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку 'Стоп': выполняет StopCommand и закрывает окно.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TimerNotificationViewModel vm)
            {
                if (vm.StopCommand.CanExecute(null))
                    vm.StopCommand.Execute(null);
            }
            this.Close();
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
            var viewModel = new TimerNotificationViewModel(soundHandle, description, notificationType);
            window = new TimerNotificationWindow(viewModel);
            return window;
        }
    }
}