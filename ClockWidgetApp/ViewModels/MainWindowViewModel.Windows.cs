using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

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
            _logger.LogDebug("[MainWindowViewModel.Windows] Updating windows visibility: Digital={0}, Analog={1}", ShowDigitalClock, ShowAnalogClock);
            var mainWindow = _windowService.GetMainWindow();
            // --- Цифровое окно ---
            if (ShowDigitalClock)
            {
                _windowService?.OpenMainWindow();
                _logger.LogDebug("[MainWindowViewModel.Windows] Main window ensured visible and activated");
            }
            else
            {
                _windowService?.HideMainWindow();
                _logger.LogDebug("[MainWindowViewModel.Windows] Main window hidden");
            }

            // --- Аналоговое окно ---
            if (ShowAnalogClock)
            {
                _windowService?.OpenAnalogClockWindow();
                _logger.LogDebug("[MainWindowViewModel.Windows] Analog clock window ensured visible and activated");
            }
            else
            {
                _windowService?.HideAnalogClockWindow();
                _logger.LogDebug("[MainWindowViewModel.Windows] Analog clock window hidden");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MainWindowViewModel.Windows] Error updating windows visibility");
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