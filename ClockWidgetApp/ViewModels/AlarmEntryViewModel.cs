using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

/// <summary>
/// ViewModel для отдельного будильника.
/// </summary>
public class AlarmEntryViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Время срабатывания будильника.
    /// </summary>
    public TimeSpan AlarmTime { get; set; }
    /// <summary>
    /// Активен ли будильник.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Строка для отображения времени будильника.
    /// </summary>
    public string DisplayTime => AlarmTime.ToString(@"hh\:mm\:ss");

    // Команды для управления будильником
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand DeactivateCommand { get; }

    private bool _isRunning;
    public bool IsRunning { get => _isRunning; set { _isRunning = value; OnPropertyChanged(); } }

    public bool IsStartAvailable => !IsRunning && IsActive;
    public bool IsStopAvailable => IsRunning && IsActive;
    public bool IsHideAvailable => !IsWidgetVisible;

    private bool _isWidgetVisible = true;
    public bool IsWidgetVisible { get => _isWidgetVisible; set { _isWidgetVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsHideAvailable)); } }

    public event Action<AlarmEntryViewModel>? RequestDelete;
    public event Action<AlarmEntryViewModel>? RequestDeactivate;

    public ICommand ToggleWidgetVisibilityCommand { get; }

    /// <summary>
    /// Конструктор AlarmEntryViewModel.
    /// </summary>
    /// <param name="alarmTime">Время срабатывания будильника.</param>
    public AlarmEntryViewModel(TimeSpan alarmTime)
    {
        AlarmTime = alarmTime;
        StartCommand = new RelayCommand(_ => { if (IsStartAvailable) Start(); });
        StopCommand = new RelayCommand(_ => { if (IsStopAvailable) Stop(); });
        DeleteCommand = new RelayCommand(_ => RequestDelete?.Invoke(this));
        DeactivateCommand = new RelayCommand(_ => { if (IsHideAvailable) Deactivate(); });
        ToggleWidgetVisibilityCommand = new RelayCommand(_ => ToggleWidgetVisibility());
    }

    /// <summary>
    /// Запускает будильник.
    /// </summary>
    public void Start() { IsRunning = true; OnPropertyChanged(nameof(IsStartAvailable)); OnPropertyChanged(nameof(IsStopAvailable)); }
    /// <summary>
    /// Останавливает будильник.
    /// </summary>
    public void Stop() { IsRunning = false; OnPropertyChanged(nameof(IsStartAvailable)); OnPropertyChanged(nameof(IsStopAvailable)); }
    /// <summary>
    /// Деактивирует будильник (например, скрывает его виджет).
    /// </summary>
    public void Deactivate() { IsActive = false; OnPropertyChanged(nameof(IsActive)); OnPropertyChanged(nameof(IsStartAvailable)); OnPropertyChanged(nameof(IsStopAvailable)); OnPropertyChanged(nameof(IsHideAvailable)); RequestDeactivate?.Invoke(this); }
    /// <summary>
    /// Переключает видимость виджета будильника.
    /// </summary>
    public void ToggleWidgetVisibility()
    {
        IsWidgetVisible = !IsWidgetVisible;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 