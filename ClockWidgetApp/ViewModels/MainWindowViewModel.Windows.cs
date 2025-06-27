using ClockWidgetApp.Helpers;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

public partial class MainWindowViewModel
{
    private IWindowService? _windowService => ((App)System.Windows.Application.Current).Services.GetService(typeof(IWindowService)) as IWindowService;

    /// <summary>
    /// Обновляет видимость окон (цифровых и аналоговых часов) в зависимости от настроек.
    /// </summary>
    private void UpdateWindowsVisibility()
    {
        try
        {
            _logger.LogInformation("[MainWindowViewModel.Windows] Updating windows visibility: Digital={0}, Analog={1}", _showDigitalClock, _showAnalogClock);
            var mainWindow = MainWindow;
            // --- Цифровое окно ---
            if (_showDigitalClock)
            {
                _windowService?.OpenMainWindow();
                _logger.LogInformation("[MainWindowViewModel.Windows] Main window ensured visible and activated");
            }
            else
            {
                _windowService?.HideMainWindow();
                _logger.LogInformation("[MainWindowViewModel.Windows] Main window hidden");
            }

            // --- Аналоговое окно ---
            if (_showAnalogClock)
            {
                _windowService?.OpenAnalogClockWindow();
                _logger.LogInformation("[MainWindowViewModel.Windows] Analog clock window ensured visible and activated");
            }
            else
            {
                _windowService?.HideAnalogClockWindow();
                _logger.LogInformation("[MainWindowViewModel.Windows] Analog clock window hidden");
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
        return WindowPositionHelper.GetWindowPosition(_settingsService, false);
    }

    /// <summary>
    /// Сохраняет позицию главного окна.
    /// </summary>
    /// <param name="left">Координата Left.</param>
    /// <param name="top">Координата Top.</param>
    public void SaveWindowPosition(double left, double top)
    {
        WindowPositionHelper.SaveWindowPosition(_settingsService, left, top, false);
    }

    private void UpdateAnalogClockSize()
    {
        if (AnalogClockWindow != null)
        {
            AnalogClockWindow.Width = _analogClockSize;
            AnalogClockWindow.Height = _analogClockSize;
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