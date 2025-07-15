using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Services;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для окна настроек. Связывает настройки главного окна с UI.
/// </summary>
public sealed class SettingsWindowViewModel : INotifyPropertyChanged
{
    #region Private Fields
    /// <summary>Главная ViewModel для передачи настроек.</summary>
    private readonly MainWindowViewModel _mainViewModel;
    /// <summary>Сервис доступа к данным приложения.</summary>
    private readonly IAppDataService _appDataService;
    /// <summary>Логгер для событий ViewModel.</summary>
    private readonly ILogger<SettingsWindowViewModel> _logger;
    /// <summary>ViewModel для таймеров и будильников.</summary>
    private readonly TimersAndAlarmsViewModel _timersAndAlarmsViewModel;
    /// <summary>Индекс выбранной вкладки.</summary>
    private int _selectedTabIndex;
    #endregion

    #region Constructor
    /// <summary>
    /// Создаёт новый экземпляр SettingsWindowViewModel.
    /// </summary>
    public SettingsWindowViewModel(MainWindowViewModel mainViewModel, IAppDataService appDataService, TimersAndAlarmsViewModel timersAndAlarmsViewModel, ILogger<SettingsWindowViewModel> logger)
    {
        _mainViewModel = mainViewModel;
        _appDataService = appDataService;
        _timersAndAlarmsViewModel = timersAndAlarmsViewModel;
        _logger = logger;
        TimersVm = _timersAndAlarmsViewModel.TimersVm;
        AlarmsVm = _timersAndAlarmsViewModel.AlarmsVm;
        LongTimersVm = _timersAndAlarmsViewModel.LongTimersVm;
        _logger.LogInformation("[SettingsWindowViewModel] Settings window view model initialized");
        Localized = LocalizationManager.GetLocalizedStrings();
        LocalizationManager.LanguageChanged += (_, _) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Localized));
        };
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        LongTimersVm.LongTimers.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(LongTimersVm));
        };
    }
    #endregion

    #region Public Properties
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Сравнивает два значения double с заданной точностью (эпсилон).
    /// </summary>
    private static bool AreClose(double a, double b, double epsilon = 1e-6)
        => Math.Abs(a - b) < epsilon;

    /// <summary>
    /// Прозрачность фона главного окна. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public double BackgroundOpacity
    {
        get => _mainViewModel.BackgroundOpacity;
        set
        {
            if (AreClose(_mainViewModel.BackgroundOpacity, value)) return;
            _logger.LogInformation($"[SettingsWindowViewModel] Background opacity changed: {value}");
            _mainViewModel.BackgroundOpacity = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// Показывать секунды. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool ShowSeconds
    {
        get => _mainViewModel.ShowSeconds;
        set
        {
            if (_mainViewModel.ShowSeconds != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Show seconds changed: {value}");
                _mainViewModel.ShowSeconds = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Показывать цифровые часы. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool ShowDigitalClock
    {
        get => _mainViewModel.ShowDigitalClock;
        set
        {
            if (_mainViewModel.ShowDigitalClock != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Show digital clock changed: {value}");
                _mainViewModel.ShowDigitalClock = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Показывать аналоговые часы. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool ShowAnalogClock
    {
        get => _mainViewModel.ShowAnalogClock;
        set
        {
            if (_mainViewModel.ShowAnalogClock != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Show analog clock changed: {value}");
                _mainViewModel.ShowAnalogClock = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Размер аналоговых часов. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public double AnalogClockSize
    {
        get => _mainViewModel.AnalogClockSize;
        set
        {
            if (AreClose(_mainViewModel.AnalogClockSize, value)) return;
            _logger.LogInformation($"[SettingsWindowViewModel] Analog clock size changed: {value}");
            _mainViewModel.AnalogClockSize = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// Аналоговые часы поверх всех окон. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool AnalogClockTopmost
    {
        get => _mainViewModel.AnalogClockTopmost;
        set
        {
            if (_mainViewModel.AnalogClockTopmost != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] AnalogClockTopmost changed: {value}");
                _mainViewModel.AnalogClockTopmost = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Цифровые часы поверх всех окон. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool DigitalClockTopmost
    {
        get => _mainViewModel.DigitalClockTopmost;
        set
        {
            if (_mainViewModel.DigitalClockTopmost != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] DigitalClockTopmost changed: {value}");
                _mainViewModel.DigitalClockTopmost = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Воспроизводить звук кукушки каждый час. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool CuckooEveryHour
    {
        get => _mainViewModel.CuckooEveryHour;
        set
        {
            if (_mainViewModel.CuckooEveryHour != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] CuckooEveryHour changed: {value}");
                _mainViewModel.CuckooEveryHour = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Воспроизводить сигнал каждые полчаса. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public bool HalfHourChimeEnabled
    {
        get => _mainViewModel.HalfHourChimeEnabled;
        set
        {
            if (_mainViewModel.HalfHourChimeEnabled != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] HalfHourChimeEnabled changed: {value}");
                _mainViewModel.HalfHourChimeEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Размер шрифта. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public double FontSize
    {
        get => _mainViewModel.FontSize;
        set
        {
            if (AreClose(_mainViewModel.FontSize, value)) return;
            _logger.LogInformation($"[SettingsWindowViewModel] Font size changed: {value}");
            _mainViewModel.FontSize = value;
            OnPropertyChanged();
        }
    }
    public string Language
    {
        get => LocalizationManager.CurrentLanguage;
        set
        {
            if (LocalizationManager.CurrentLanguage != value)
            {
                LocalizationManager.SetLanguage(value);
                _appDataService.Data.WidgetSettings.Language = value;
            }
        }
    }
    public LocalizedStrings Localized { get; private set; }
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set { _selectedTabIndex = value; OnPropertyChanged(); }
    }
    public RelayCommand SelectTabCommand => new RelayCommand(idx =>
    {
        if (idx is int i)
            SelectedTabIndex = i;
    });
    public RelayCommand ShowLogsCommand => new RelayCommand(_ => ShowLogs());
    public RelayCommand CloseAppCommand => new RelayCommand(_ => CloseApp());
    public TimersViewModel TimersVm { get; }
    public AlarmsViewModel AlarmsVm { get; }
    public LongTimersViewModel LongTimersVm { get; private set; }

    /// <summary>
    /// Команда для удаления длинного таймера с подтверждением.
    /// </summary>
    public RelayCommand DeleteLongTimerCommand => new RelayCommand(obj =>
    {
        if (obj is LongTimerEntryViewModel timer)
        {
            if (DialogHelper.ConfirmLongTimerDelete())
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Пользователь подтвердил удаление длинного таймера: {timer.Name} ({timer.TargetDateTime})");
                timer.Dispose();
                var persist = LongTimersVm.LongTimerModels.FirstOrDefault(m => m.TargetDateTime == timer.TargetDateTime && m.Name == timer.Name);
                if (persist != null)
                    LongTimersVm.LongTimerModels.Remove(persist);
            }
            else
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Пользователь отменил удаление длинного таймера: {timer.Name} ({timer.TargetDateTime})");
            }
        }
    });

    /// <summary>
    /// Команда для удаления обычного таймера без подтверждения.
    /// </summary>
    public RelayCommand DeleteTimerCommand => new RelayCommand(obj =>
    {
        if (obj is TimerEntryViewModel timer)
        {
            TimersVm.TimerEntries.Remove(timer);
            // Также удаляем из PersistModel
            var persist = TimersVm.Timers.FirstOrDefault(m => m.Duration == timer.Duration);
            if (persist != null)
                TimersVm.Timers.Remove(persist);
        }
    });

    /// <summary>
    /// Команда для удаления будильника без подтверждения.
    /// </summary>
    public RelayCommand DeleteAlarmCommand => new RelayCommand(obj =>
    {
        if (obj is AlarmEntryViewModel alarm)
        {
            AlarmsVm.Alarms.Remove(alarm);
            var persist = AlarmsVm.AlarmModels.FirstOrDefault(m =>
                m.AlarmTime == alarm.AlarmTime &&
                m.IsEnabled == alarm.IsEnabled &&
                m.NextTriggerDateTime == alarm.NextTriggerDateTime);
            if (persist != null)
                AlarmsVm.AlarmModels.Remove(persist);
        }
    });
    #endregion

    #region Private Methods
    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocalizationManager.CurrentLanguage))
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Localized));
        }
        else
        {
            OnPropertyChanged(e.PropertyName);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Открывает последний лог-файл приложения.
    /// </summary>
    private void ShowLogs()
    {
        try
        {
            string logsDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClockWidget",
                "logs");
            if (!System.IO.Directory.Exists(logsDir))
            {
                System.Windows.MessageBox.Show(Localized.SettingsWindow_LogsNotFound, Localized.SettingsWindow_Logs, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var logFiles = System.IO.Directory.GetFiles(logsDir, "clock-widget-*.log");
            if (logFiles.Length == 0)
            {
                System.Windows.MessageBox.Show(Localized.SettingsWindow_LogsNotFound, Localized.SettingsWindow_Logs, MessageBoxButton.OK, MessageBoxImage.Information);
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
            _logger.LogError(ex, "[SettingsWindowViewModel] Ошибка при открытии файла логов");
            System.Windows.MessageBox.Show($"Ошибка при открытии файла логов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Завершает работу приложения.
    /// </summary>
    private void CloseApp()
    {
        try
        {
            _logger.LogInformation("[SettingsWindowViewModel] Shutting down application");
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindowViewModel] Error during application shutdown");
            System.Windows.Application.Current.Shutdown();
        }
    }

    #endregion
}