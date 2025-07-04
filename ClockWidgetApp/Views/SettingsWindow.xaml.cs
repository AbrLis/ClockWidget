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
            // Устанавливаем DataContext для вкладки 'Alarms & Timers' на Singleton-экземпляр
            if (TimersAlarmsGrid != null)
                TimersAlarmsGrid.DataContext = TimersAndAlarmsViewModel.Instance;
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
        try
        {
            _logger.LogDebug("[SettingsWindow] Close widget button clicked");
            foreach (Window window in System.Windows.Application.Current.Windows)
                window.Close();
            _logger.LogDebug("[SettingsWindow] Shutting down application");
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Error during application shutdown");
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Кнопка открытия логов.
    /// </summary>
    private void ShowLogsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string logsDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget",
                "logs");
            if (!System.IO.Directory.Exists(logsDir))
            {
                System.Windows.MessageBox.Show(_viewModel.Localized.SettingsWindow_LogsNotFound, _viewModel.Localized.SettingsWindow_Logs, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var logFiles = System.IO.Directory.GetFiles(logsDir, "clock-widget-*.log");
            if (logFiles.Length == 0)
            {
                System.Windows.MessageBox.Show(_viewModel.Localized.SettingsWindow_LogsNotFound, _viewModel.Localized.SettingsWindow_Logs, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var lastLog = logFiles
                .Select(f => new System.IO.FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .First().FullName;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = lastLog,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindow] Ошибка при открытии файла логов");
            System.Windows.MessageBox.Show($"Ошибка при открытии файла логов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
    /// Клик по таймеру для редактирования.
    /// </summary>
    private void TimerItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is System.Windows.Controls.Grid grid && grid.DataContext is TimerEntryViewModel timer)
        {
            var vm = ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance;
            if (vm.TimersVM.EditTimerCommand.CanExecute(timer))
                vm.TimersVM.EditTimerCommand.Execute(timer);
        }
    }

    /// <summary>
    /// Корректирует значения времени таймера при потере фокуса.
    /// </summary>
    private void TimerTimeBox_LostFocus(object sender, RoutedEventArgs e)
    {
        TimersAndAlarmsViewModel.Instance.TimersVM.CorrectTimerTime();
    }

    /// <summary>
    /// Корректирует значения времени будильника при потере фокуса.
    /// </summary>
    private void AlarmTimeBox_LostFocus(object sender, RoutedEventArgs e)
    {
        TimersAndAlarmsViewModel.Instance.AlarmsVM.CorrectAlarmTime();
    }

    /// <summary>
    /// Клик по строке будильника для редактирования.
    /// </summary>
    private void AlarmItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Grid grid && grid.DataContext is AlarmEntryViewModel alarm)
        {
            var vm = ClockWidgetApp.ViewModels.TimersAndAlarmsViewModel.Instance;
            if (vm.AlarmsVM.EditAlarmCommand.CanExecute(alarm))
                vm.AlarmsVM.EditAlarmCommand.Execute(alarm);
        }
    }

    /// <summary>
    /// Программно выбирает вкладку 'Будильники и таймеры'.
    /// </summary>
    public void SelectTimersTab()
    {
        if (MainTabControl != null && MainTabControl.Items.Count > 1)
            MainTabControl.SelectedIndex = 1;
    }

    /// <summary>
    /// Программно выбирает вкладку 'Общие настройки'.
    /// </summary>
    public void SelectGeneralTab()
    {
        if (MainTabControl != null && MainTabControl.Items.Count > 0)
            MainTabControl.SelectedIndex = 0;
    }

    #endregion
} 