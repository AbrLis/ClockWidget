using System.Windows;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
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
            _logger.LogInformation("[MainWindowViewModel.Windows] Updating windows visibility: Digital={0}, Analog={1}", _showDigitalClock, _showAnalogClock);
            var mainWindow = MainWindow.Instance;
            if (mainWindow != null)
            {
                var newVisibility = _showDigitalClock ? Visibility.Visible : Visibility.Hidden;
                if (mainWindow.Visibility != newVisibility)
                {
                    mainWindow.Visibility = newVisibility;
                    if (newVisibility == Visibility.Visible)
                    {
                        mainWindow.Show();
                        mainWindow.Activate();
                        mainWindow.Topmost = _digitalClockTopmost;
                        _logger.LogInformation("[MainWindowViewModel.Windows] Main window shown and activated");
                    }
                    else
                    {
                        mainWindow.Hide();
                        _logger.LogInformation("[MainWindowViewModel.Windows] Main window hidden");
                    }
                }
            }
            if (_showAnalogClock)
            {
                var analogWindow = AnalogClockWindow.Instance;
                if (analogWindow == null)
                {
                    // Создаём окно через DI
                    ((App)System.Windows.Application.Current).ShowAnalogClockWindow();
                    analogWindow = AnalogClockWindow.Instance;
                }
                if (analogWindow != null)
                {
                    analogWindow.Width = _analogClockSize;
                    analogWindow.Height = _analogClockSize;
                    analogWindow.Topmost = _analogClockTopmost;
                    if (!analogWindow.IsVisible)
                    {
                        analogWindow.Show();
                    }
                    analogWindow.Activate();
                    _logger.LogInformation("[MainWindowViewModel.Windows] Analog clock window shown and activated at position: Left={0}, Top={1}", analogWindow.Left, analogWindow.Top);
                }
            }
            else if (AnalogClockWindow.Instance != null && AnalogClockWindow.Instance.IsVisible)
            {
                _logger.LogInformation("[MainWindowViewModel.Windows] Hiding analog clock window");
                AnalogClockWindow.Instance.Hide();
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
        if (AnalogClockWindow.Instance != null)
        {
            AnalogClockWindow.Instance.Width = _analogClockSize;
            AnalogClockWindow.Instance.Height = _analogClockSize;
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