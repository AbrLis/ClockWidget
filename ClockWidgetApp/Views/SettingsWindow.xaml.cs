using System.Windows;
using ClockWidgetApp.ViewModels;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Helpers;

namespace ClockWidgetApp;

/// <summary>
/// Окно настроек приложения.
/// </summary>
public partial class SettingsWindow : Window
{
    #region Private Fields
    /// <summary>ViewModel окна настроек.</summary>
    private readonly SettingsWindowViewModel _viewModel = null!;
    /// <summary>Логгер для событий окна.</summary>
    private readonly ILogger<SettingsWindow> _logger;
    #endregion

    /// <summary>
    /// Создаёт новое окно настроек.
    /// </summary>
    /// <param name="viewModel">ViewModel окна настроек.</param>
    /// <param name="logger">ILogger<SettingsWindow> для логирования.</param>
    public SettingsWindow(SettingsWindowViewModel viewModel, ILogger<SettingsWindow> logger)
        : base()
    {
        _viewModel = viewModel;
        _logger = logger;
        try
        {
            _logger.LogDebug("[SettingsWindow] Initializing settings window");
            InitializeComponent();
            DataContext = _viewModel;
            LocalizationManager.LanguageChanged += (s, e) =>
            {
                DataContext = null;
                DataContext = _viewModel;
            };
            Closing += SettingsWindow_Closing;
            _logger.LogDebug("[SettingsWindow] Settings window initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error initializing settings window");
            throw;
        }
    }

    /// <summary>
    /// ViewModel для биндинга в XAML и других окон.
    /// </summary>
    public SettingsWindowViewModel ViewModel => _viewModel;

    #region Event Handlers & Private Methods

    /// <summary>
    /// Обработка закрытия окна: скрытие и сброс флага открытия.
    /// </summary>
    private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogDebug("[SettingsWindow] Settings window closing");
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                mainWindow.ViewModel.IsSettingsWindowOpen = false;
            e.Cancel = true;
            this.Hide();
            _logger.LogDebug("[SettingsWindow] Settings window hidden (not closed)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error during settings window closing");
        }
    }

    /// <summary>
    /// Кнопка закрытия приложения.
    /// </summary>
    private void CloseWidgetButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CloseAppCommand.Execute(null);
    }

    /// <summary>
    /// Кнопка открытия логов.
    /// </summary>
    private void ShowLogsButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ShowLogsCommand.Execute(null);
    }

    /// <summary>
    /// Разрешает только числовой ввод в TextBox.
    /// </summary>
    private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        foreach (char c in e.Text)
        {
            if (!char.IsDigit(c))
            {
                e.Handled = true;
                return;
            }
        }
    }

    /// <summary>
    /// Корректирует значения времени таймера при потере фокуса.
    /// </summary>
    private void TimerTimeBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance.TimersVM.CorrectTimerTime();
    }

    /// <summary>
    /// Корректирует значения времени будильника при потере фокуса.
    /// </summary>
    private void AlarmTimeBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance.AlarmsVM.CorrectAlarmTime();
    }

    /// <summary>
    /// Клик по таймеру для редактирования.
    /// </summary>
    private void TimerItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is System.Windows.Controls.Grid grid && grid.DataContext is TimerEntryViewModel timer)
        {
            var vm = ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance.TimersVM;
            if (vm.EditTimerCommand.CanExecute(timer))
                vm.EditTimerCommand.Execute(timer);
        }
    }

    /// <summary>
    /// Клик по строке будильника для редактирования.
    /// </summary>
    private void AlarmItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is System.Windows.Controls.Grid grid && grid.DataContext is AlarmEntryViewModel alarm)
        {
            var vm = ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance.AlarmsVM;
            if (vm.EditAlarmCommand.CanExecute(alarm))
                vm.EditAlarmCommand.Execute(alarm);
        }
    }

    #endregion
} 