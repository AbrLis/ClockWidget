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
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
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
                if (_analogClockWindow == null)
                {
                    _logger.LogInformation("[MainWindowViewModel.Windows] Creating analog clock window");
                    _analogClockWindow = new AnalogClockWindow();
                    var (left, top) = GetAnalogClockPosition();
                    _analogClockWindow.Left = left;
                    _analogClockWindow.Top = top;
                    _analogClockWindow.Width = _analogClockSize;
                    _analogClockWindow.Height = _analogClockSize;
                    _analogClockWindow.Topmost = _analogClockTopmost;
                    _analogClockWindow.Show();
                }
                else if (!_analogClockWindow.IsVisible)
                {
                    _analogClockWindow.Show();
                }
                _analogClockWindow.Activate();
                _logger.LogInformation("[MainWindowViewModel.Windows] Analog clock window shown and activated at position: Left={0}, Top={1}", _analogClockWindow.Left, _analogClockWindow.Top);
            }
            else if (_analogClockWindow != null && _analogClockWindow.IsVisible)
            {
                _logger.LogInformation("[MainWindowViewModel.Windows] Hiding analog clock window");
                _analogClockWindow.Hide();
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

    private (double Left, double Top) GetAnalogClockPosition()
    {
        return WindowPositionHelper.GetWindowPosition(_settingsService, true);
    }

    private void UpdateAnalogClockSize()
    {
        if (_analogClockWindow != null)
        {
            _analogClockWindow.Width = _analogClockSize;
            _analogClockWindow.Height = _analogClockSize;
        }
    }

    private void UpdateAnalogClockSettings(WidgetSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }
        settings = WidgetSettings.ValidateSettings(settings);
        if (settings.ShowAnalogClock)
        {
            if (_analogClockWindow == null)
            {
                _logger.LogInformation("[MainWindowViewModel.Windows] Creating analog clock window");
                _analogClockWindow = new AnalogClockWindow();
                var (left, top) = GetAnalogClockPosition();
                _analogClockWindow.Left = left;
                _analogClockWindow.Top = top;
                _analogClockWindow.Width = settings.AnalogClockSize;
                _analogClockWindow.Height = settings.AnalogClockSize;
                _analogClockWindow.Topmost = _analogClockTopmost;
                _analogClockWindow.Show();
            }
            else if (!_analogClockWindow.IsVisible)
            {
                _analogClockWindow.Show();
            }
            _analogClockWindow.Activate();
            _logger.LogInformation("[MainWindowViewModel.Windows] Analog clock window shown and activated at position: Left={0}, Top={1}", _analogClockWindow.Left, _analogClockWindow.Top);
        }
        else if (_analogClockWindow != null && _analogClockWindow.IsVisible)
        {
            _logger.LogInformation("[MainWindowViewModel.Windows] Hiding analog clock window");
            _analogClockWindow.Hide();
        }
    }
} 