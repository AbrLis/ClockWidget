using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ClockWidgetApp.Helpers;
using System.Windows;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для окна настроек. Связывает настройки главного окна с UI.
/// </summary>
public class SettingsWindowViewModel : INotifyPropertyChanged
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
        _logger.LogInformation("[SettingsWindowViewModel] Settings window view model initialized");
        Localized = LocalizationManager.GetLocalizedStrings();
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Localized));
        };
        _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        foreach (var timer in LongTimersVM.LongTimers)
            timer.RequestDelete += OnLongTimerRequestDelete;
        LongTimersVM.LongTimers.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    (item as LongTimerEntryViewModel)!.RequestDelete += OnLongTimerRequestDelete;
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    (item as LongTimerEntryViewModel)!.RequestDelete -= OnLongTimerRequestDelete;
        };
    }
    #endregion

    #region Public Properties
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Прозрачность фона главного окна. Связывает значение из MainWindowViewModel с UI окна настроек.
    /// </summary>
    public double BackgroundOpacity
    {
        get => _mainViewModel.BackgroundOpacity;
        set
        {
            if (_mainViewModel.BackgroundOpacity != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Background opacity changed: {value}");
                _mainViewModel.BackgroundOpacity = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Показывать секунды. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Показывать цифровые часы. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Показывать аналоговые часы. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Размер аналоговых часов. Dirty-флаг выставляется в MainWindowViewModel.
    /// </summary>
    public double AnalogClockSize
    {
        get => _mainViewModel.AnalogClockSize;
        set
        {
            if (System.Math.Abs(_mainViewModel.AnalogClockSize - value) > 0.001)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Analog clock size changed: {value}");
                _mainViewModel.AnalogClockSize = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Аналоговые часы поверх всех окон. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Цифровые часы поверх всех окон. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Воспроизводить звук кукушки каждый час. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Воспроизводить сигнал каждые полчаса. Dirty-флаг выставляется в MainWindowViewModel.
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
    /// Размер шрифта. Dirty-флаг выставляется в MainWindowViewModel.
    /// </summary>
    public double FontSize
    {
        get => _mainViewModel.FontSize;
        set
        {
            if (_mainViewModel.FontSize != value)
            {
                _logger.LogInformation($"[SettingsWindowViewModel] Font size changed: {value}");
                _mainViewModel.FontSize = value;
                OnPropertyChanged();
            }
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
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();
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
    public TimersViewModel TimersVM => _timersAndAlarmsViewModel.TimersVM;
    public AlarmsViewModel AlarmsVM => _timersAndAlarmsViewModel.AlarmsVM;
    public LongTimersViewModel LongTimersVM => _timersAndAlarmsViewModel.LongTimersVM;
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
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
        catch (System.Exception ex)
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
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "[SettingsWindowViewModel] Error during application shutdown");
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Обработчик запроса на удаление длинного таймера с подтверждением.
    /// </summary>
    private void OnLongTimerRequestDelete(LongTimerEntryViewModel timer)
    {
        if (Helpers.DialogHelper.ConfirmLongTimerDelete())
        {
            _logger.LogInformation($"[SettingsWindowViewModel] Пользователь подтвердил удаление длинного таймера: {timer.Name} ({timer.TargetDateTime})");
            timer.Dispose();
            LongTimersVM.LongTimers.Remove(timer);
        }
        else
        {
            _logger.LogInformation($"[SettingsWindowViewModel] Пользователь отменил удаление длинного таймера: {timer.Name} ({timer.TargetDateTime})");
        }
    }
    #endregion
} 