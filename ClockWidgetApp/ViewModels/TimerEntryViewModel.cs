using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Timers;
using ClockWidgetApp.Services;
using ClockWidgetApp.Views;
using ClockWidgetApp;

/// <summary>
/// ViewModel для отдельного таймера.
/// </summary>
public class TimerEntryViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Длительность таймера.
    /// </summary>
    public TimeSpan Duration { get; set; }
    private TimeSpan _remaining;
    /// <summary>
    /// Оставшееся время таймера.
    /// </summary>
    public TimeSpan Remaining { get => _remaining; set { _remaining = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayTime)); } }
    /// <summary>
    /// Активен ли таймер.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Строка для отображения оставшегося времени.
    /// </summary>
    public string DisplayTime => Remaining.ToString(@"hh\:mm\:ss");

    // Команды для управления таймером
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

    public event Action<TimerEntryViewModel>? RequestDelete;
    public event Action<TimerEntryViewModel>? RequestDeactivate;

    public ICommand ToggleWidgetVisibilityCommand { get; }

    private System.Timers.Timer? _timer;

    /// <summary>
    /// Конструктор TimerEntryViewModel.
    /// </summary>
    /// <param name="duration">Длительность таймера.</param>
    public TimerEntryViewModel(TimeSpan duration)
    {
        Duration = duration;
        Remaining = duration;
        StartCommand = new RelayCommand(_ => { if (IsStartAvailable) Start(); });
        StopCommand = new RelayCommand(_ => { if (IsStopAvailable) Stop(); });
        DeleteCommand = new RelayCommand(_ => RequestDelete?.Invoke(this));
        DeactivateCommand = new RelayCommand(_ => { if (IsHideAvailable) Deactivate(); });
        ToggleWidgetVisibilityCommand = new RelayCommand(_ => ToggleWidgetVisibility());
    }

    /// <summary>
    /// Запускает таймер.
    /// </summary>
    public void Start()
    {
        if (IsRunning) return;
        if (Remaining <= TimeSpan.Zero)
            return;
        IsRunning = true;
        OnPropertyChanged(nameof(IsStartAvailable));
        OnPropertyChanged(nameof(IsStopAvailable));
        if (_timer == null)
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
        }
        _timer.Start();
    }
    /// <summary>
    /// Останавливает таймер.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        OnPropertyChanged(nameof(IsStartAvailable));
        OnPropertyChanged(nameof(IsStopAvailable));
        _timer?.Stop();
    }
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (Remaining.TotalSeconds > 0)
        {
            Remaining = Remaining - TimeSpan.FromSeconds(1);
            if (Remaining.TotalSeconds <= 0)
            {
                Remaining = TimeSpan.Zero;
                Stop();
                Remaining = Duration;
                // --- Воспроизведение звука и показ окна уведомления ---
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var app = System.Windows.Application.Current as App;
                    if (app?.Services is not { } services)
                        return;
                    var soundService = services.GetService(typeof(ISoundService)) as ISoundService;
                    if (soundService == null)
                        return;
                    var baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (string.IsNullOrEmpty(baseDir))
                        return;
                    string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", "timer.mp3");
                    var soundHandle = soundService.PlaySoundInstance(soundPath, true);
                    var notification = new TimerNotificationWindow(soundHandle, Duration.ToString(@"hh\:mm\:ss"), "timer");
                    notification.Show();
                });
                // --- конец ---
            }
        }
        else
        {
            Stop();
            Remaining = Duration;
        }
        // Обновляем DisplayTime
        OnPropertyChanged(nameof(DisplayTime));
    }
    /// <summary>
    /// Деактивирует таймер (например, скрывает его виджет).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(IsStartAvailable));
        OnPropertyChanged(nameof(IsStopAvailable));
        OnPropertyChanged(nameof(IsHideAvailable));
        RequestDeactivate?.Invoke(this);
    }
    public void ToggleWidgetVisibility()
    {
        IsWidgetVisible = !IsWidgetVisible;
    }
    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 