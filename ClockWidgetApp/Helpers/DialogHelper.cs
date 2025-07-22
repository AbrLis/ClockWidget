namespace ClockWidgetApp.Helpers
{
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Вспомогательные методы для диалоговых окон подтверждения.
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Делегат для подтверждения удаления длинного таймера. Можно подменять в тестах.
        /// </summary>
        public static Func<bool> ConfirmLongTimerDelete = () =>
        {
            var message = LocalizationManager.GetString("LongTimers_ConfirmDelete");
            var title = LocalizationManager.GetString("LongTimers_ConfirmDeleteTitle");
            var result = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            return result == System.Windows.MessageBoxResult.Yes;
        };
    }
}