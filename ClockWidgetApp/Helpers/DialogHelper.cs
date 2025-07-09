using System.Windows;
using System.Linq;

namespace ClockWidgetApp.Helpers
{
    /// <summary>
    /// Вспомогательные методы для диалоговых окон подтверждения.
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Показывает окно подтверждения удаления длинного таймера с owner = SettingsWindow, если оно открыто.
        /// </summary>
        /// <returns>true, если пользователь подтвердил удаление; иначе false.</returns>
        public static bool ConfirmLongTimerDelete()
        {
            Window? owner = null;
            // Получаем окно настроек через IWindowService из DI
            if (System.Windows.Application.Current is App app)
            {
                var windowService = app.Services.GetService(typeof(ClockWidgetApp.Services.IWindowService)) as ClockWidgetApp.Services.IWindowService;
                owner = windowService?.GetSettingsWindow();
            }
            return System.Windows.MessageBox.Show(
                owner,
                "Удалить длинный таймер? Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }
    }
} 