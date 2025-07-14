using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    /// <summary>
    /// Обновляет видимость окон (цифровых и аналоговых часов) в зависимости от настроек.
    /// </summary>
    private void UpdateWindowsVisibility()
    {
        try
        {
            this._logger.LogDebug("[MainWindowViewModel.Windows] Updating windows visibility: Digital={0}, Analog={1}", ShowDigitalClock, ShowAnalogClock);

            // --- Цифровое окно ---
            if (this.ShowDigitalClock)
            {
                this._windowService.OpenMainWindow();
                this._logger.LogDebug("[MainWindowViewModel.Windows] Main window ensured visible and activated");
            }
            else
            {
                this._windowService.HideMainWindow();
                this._logger.LogDebug("[MainWindowViewModel.Windows] Main window hidden");
            }

            // --- Аналоговое окно ---
            if (this.ShowAnalogClock)
            {
                this._windowService.OpenAnalogClockWindow();
                this._logger.LogDebug("[MainWindowViewModel.Windows] Analog clock window ensured visible and activated");
            }
            else
            {
                this._windowService.HideAnalogClockWindow();
                this._logger.LogDebug("[MainWindowViewModel.Windows] Analog clock window hidden");
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "[MainWindowViewModel.Windows] Error updating windows visibility");
        }
    }

    /// <summary>
    /// Получает сохранённую позицию главного окна.
    /// </summary>
    /// <returns>Кортеж с координатами Left и Top.</returns>
    public (double Left, double Top) GetWindowPosition()
    {
        return WindowPositionHelper.GetWindowPosition(_appDataService, false);
    }

    private void UpdateAnalogClockSize()
    {
        var analogWindow = _windowService.GetAnalogClockWindow();
        if (analogWindow != null)
        {
            analogWindow.Width = _analogClockSize;
            analogWindow.Height = _analogClockSize;
        }
    }

    /// <summary>
    /// Публичный метод для показа аналоговых часов из внешних источников (например, из трея).
    /// Гарантирует отсутствие дублирующихся окон.
    /// </summary>
    public void ShowAnalogClockWindow()
    {
        ShowAnalogClock = true;
        UpdateWindowsVisibility();
    }
}