using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Data;
using ClockWidgetApp.Helpers;
using System.Globalization;
using System.Timers;
using System.Windows;
using ClockWidgetApp.Views;
using ClockWidgetApp.Services;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для вкладки "Будильники и таймеры". Управляет логикой добавления, отображения и управления таймерами и будильниками.
/// </summary>
public class TimersAndAlarmsViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Singleton-экземпляр ViewModel для вкладки 'Будильники и таймеры'.
    /// </summary>
    public static TimersAndAlarmsViewModel Instance { get; } = new TimersAndAlarmsViewModel();

    /// <summary>
    /// Локализованные строки для UI.
    /// </summary>
    public LocalizedStrings Localized { get; private set; } = LocalizationManager.GetLocalizedStrings();

    /// <summary>
    /// Коллекция таймеров.
    /// </summary>
    public ObservableCollection<TimerEntryViewModel> Timers { get; } = new();

    /// <summary>
    /// Коллекция будильников.
    /// </summary>
    public ObservableCollection<AlarmEntryViewModel> Alarms { get; } = new();

    /// <summary>
    /// Показывать ли поле ввода для нового таймера.
    /// </summary>
    private bool _isTimerInputVisible;
    public bool IsTimerInputVisible
    {
        get => _isTimerInputVisible;
        set { _isTimerInputVisible = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Показывать ли поле ввода для нового будильника.
    /// </summary>
    private bool _isAlarmInputVisible;
    public bool IsAlarmInputVisible
    {
        get => _isAlarmInputVisible;
        set { _isAlarmInputVisible = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Введённое пользователем время для таймера.
    /// </summary>
    private string _newTimerHours = "";
    public string NewTimerHours { get => _newTimerHours; set { _newTimerHours = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); } }
    private string _newTimerMinutes = "";
    public string NewTimerMinutes { get => _newTimerMinutes; set { _newTimerMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); } }
    private string _newTimerSeconds = "";
    public string NewTimerSeconds { get => _newTimerSeconds; set { _newTimerSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); } }
    public bool IsNewTimerValid =>
        TryParseOrZero(NewTimerHours, out var h) &&
        TryParseOrZero(NewTimerMinutes, out var m) &&
        TryParseOrZero(NewTimerSeconds, out var s) &&
        (h > 0 || m > 0 || s > 0) &&
        h >= 0 && m >= 0 && m < 60 && s >= 0 && s < 60;

    /// <summary>
    /// Введённое пользователем время для будильника.
    /// </summary>
    private string _newAlarmHours = "";
    public string NewAlarmHours { get => _newAlarmHours; set { _newAlarmHours = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewAlarmValid)); } }
    private string _newAlarmMinutes = "";
    public string NewAlarmMinutes { get => _newAlarmMinutes; set { _newAlarmMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewAlarmValid)); } }
    public bool IsNewAlarmValid =>
        (!string.IsNullOrWhiteSpace(NewAlarmHours) || !string.IsNullOrWhiteSpace(NewAlarmMinutes)) &&
        TryParseOrZero(NewAlarmHours, out var h) &&
        TryParseOrZero(NewAlarmMinutes, out var m) &&
        (h > 0 || m > 0) &&
        h >= 0 && m >= 0 && m < 60;

    /// <summary>
    /// Команда для отображения поля ввода таймера.
    /// </summary>
    public ICommand ShowTimerInputCommand { get; }
    /// <summary>
    /// Команда для отображения поля ввода будильника.
    /// </summary>
    public ICommand ShowAlarmInputCommand { get; }
    /// <summary>
    /// Команда для добавления таймера.
    /// </summary>
    public ICommand AddTimerCommand { get; }
    /// <summary>
    /// Команда для добавления будильника.
    /// </summary>
    public ICommand AddAlarmCommand { get; }
    /// <summary>
    /// Команда для отмены ввода времени для таймера.
    /// </summary>
    public ICommand CancelTimerInputCommand { get; }
    /// <summary>
    /// Команда для отмены ввода времени для будильника.
    /// </summary>
    public ICommand CancelAlarmInputCommand { get; }

    // --- Режим редактирования таймера ---
    private TimerEntryViewModel? _editingTimer;
    public ICommand EditTimerCommand { get; }
    public ICommand ApplyEditTimerCommand { get; }
    public bool IsEditingTimer => _editingTimer != null;

    private void EditTimer(TimerEntryViewModel? timer)
    {
        if (timer == null) return;
        _editingTimer = timer;
        NewTimerHours = timer.Duration.Hours.ToString("D2");
        NewTimerMinutes = timer.Duration.Minutes.ToString("D2");
        NewTimerSeconds = timer.Duration.Seconds.ToString("D2");
        IsTimerInputVisible = true;
        OnPropertyChanged(nameof(IsEditingTimer));
    }

    private void ApplyEditTimer()
    {
        if (_editingTimer != null && IsNewTimerValid)
        {
            TryParseOrZero(NewTimerHours, out int h);
            TryParseOrZero(NewTimerMinutes, out int m);
            TryParseOrZero(NewTimerSeconds, out int s);
            var ts = new TimeSpan(h, m, s);
            _editingTimer.Duration = ts;
            _editingTimer.Remaining = ts;
            _editingTimer.OnPropertyChanged(nameof(_editingTimer.Duration));
            _editingTimer.OnPropertyChanged(nameof(_editingTimer.Remaining));
            _editingTimer.OnPropertyChanged(nameof(_editingTimer.DisplayTime));
            _editingTimer = null;
            IsTimerInputVisible = false;
            NewTimerHours = "";
            NewTimerMinutes = "";
            NewTimerSeconds = "";
            OnPropertyChanged(nameof(IsEditingTimer));
        }
    }

    // --- Режим редактирования будильника ---
    private AlarmEntryViewModel? _editingAlarm;
    /// <summary>
    /// Команда для начала редактирования будильника.
    /// </summary>
    public ICommand EditAlarmCommand { get; }
    /// <summary>
    /// Команда для применения изменений будильника.
    /// </summary>
    public ICommand ApplyEditAlarmCommand { get; }
    /// <summary>
    /// Находится ли ViewModel в режиме редактирования будильника.
    /// </summary>
    public bool IsEditingAlarm => _editingAlarm != null;

    /// <summary>
    /// Переводит ViewModel в режим редактирования выбранного будильника.
    /// </summary>
    private void EditAlarm(AlarmEntryViewModel? alarm)
    {
        if (alarm == null) return;
        _editingAlarm = alarm;
        NewAlarmHours = alarm.AlarmTime.Hours.ToString("D2");
        NewAlarmMinutes = alarm.AlarmTime.Minutes.ToString("D2");
        IsAlarmInputVisible = true;
        OnPropertyChanged(nameof(IsEditingAlarm));
    }

    /// <summary>
    /// Применяет изменения к редактируемому будильнику.
    /// </summary>
    private void ApplyEditAlarm()
    {
        if (_editingAlarm != null && IsNewAlarmValid)
        {
            TryParseOrZero(NewAlarmHours, out int h);
            TryParseOrZero(NewAlarmMinutes, out int m);
            var ts = new TimeSpan(h, m, 0);
            _editingAlarm.AlarmTime = ts;
            _editingAlarm.OnPropertyChanged(nameof(_editingAlarm.AlarmTime));
            _editingAlarm.OnPropertyChanged(nameof(_editingAlarm.DisplayTime));
            _editingAlarm = null;
            IsAlarmInputVisible = false;
            NewAlarmHours = "";
            NewAlarmMinutes = "";
            OnPropertyChanged(nameof(IsEditingAlarm));
        }
    }

    // В TimersAndAlarmsViewModel добавить таймер для проверки будильников
    private System.Timers.Timer? _alarmCheckTimer;

    private TimersAndAlarmsViewModel()
    {
        Localized = LocalizationManager.GetLocalizedStrings();
        LocalizationManager.LanguageChanged += (s, e) =>
        {
            Localized = LocalizationManager.GetLocalizedStrings();
            OnPropertyChanged(nameof(Localized));
            OnPropertyChanged(nameof(NewTimerHours));
            OnPropertyChanged(nameof(NewTimerMinutes));
            OnPropertyChanged(nameof(NewTimerSeconds));
            OnPropertyChanged(nameof(NewAlarmHours));
            OnPropertyChanged(nameof(NewAlarmMinutes));
        };
        NewTimerHours = "";
        NewTimerMinutes = "";
        NewTimerSeconds = "";
        NewAlarmHours = "";
        NewAlarmMinutes = "";
        ShowTimerInputCommand = new RelayCommand(_ => IsTimerInputVisible = true);
        ShowAlarmInputCommand = new RelayCommand(_ => IsAlarmInputVisible = true);
        AddTimerCommand = new RelayCommand(_ => AddTimer(), _ => IsNewTimerValid);
        AddAlarmCommand = new RelayCommand(_ => AddAlarm(), _ => IsNewAlarmValid);
        CancelTimerInputCommand = new RelayCommand(_ => CancelTimerInput());
        CancelAlarmInputCommand = new RelayCommand(_ => CancelAlarmInput());
        EditTimerCommand = new RelayCommand(t =>
        {
            if (t is TimerEntryViewModel timer && timer != null)
                EditTimer(timer);
        });
        ApplyEditTimerCommand = new RelayCommand(_ => ApplyEditTimer(), _ => IsEditingTimer && IsNewTimerValid);
        EditAlarmCommand = new RelayCommand(a =>
        {
            if (a is AlarmEntryViewModel alarm && alarm != null)
                EditAlarm(alarm);
        });
        ApplyEditAlarmCommand = new RelayCommand(_ => ApplyEditAlarm(), _ => IsEditingAlarm && IsNewAlarmValid);
        _alarmCheckTimer = new System.Timers.Timer(1000);
        _alarmCheckTimer.Elapsed += AlarmCheckTimer_Elapsed;
        _alarmCheckTimer.AutoReset = true;
        _alarmCheckTimer.Start();
    }

    private void AlarmCheckTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;
        foreach (var alarm in Alarms)
        {
            if (alarm.IsRunning && alarm.IsActive &&
                alarm.AlarmTime.Hours == now.Hour && alarm.AlarmTime.Minutes == now.Minute && now.Second == 0)
            {
                // Воспроизвести alarm.mp3 через отдельный плеер
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
                    string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", "alarm.mp3");
                    var soundHandle = soundService.PlaySoundInstance(soundPath, true);
                    var notification = new TimerNotificationWindow(soundHandle, alarm.AlarmTime.ToString(@"hh\:mm"), "alarm");
                    notification.Show();
                    alarm.Stop(); // Ставим на паузу после срабатывания
                });
            }
        }
    }

    /// <summary>
    /// Добавляет новый таймер после валидации времени.
    /// </summary>
    private void AddTimer()
    {
        if (IsNewTimerValid)
        {
            TryParseOrZero(NewTimerHours, out int h);
            TryParseOrZero(NewTimerMinutes, out int m);
            TryParseOrZero(NewTimerSeconds, out int s);
            var ts = new TimeSpan(h, m, s);
            var timer = new TimerEntryViewModel(ts);
            timer.RequestDelete += t => { t.Dispose(); Timers.Remove(t); };
            timer.RequestDeactivate += t => t.IsActive = false;
            Timers.Insert(0, timer);
            IsTimerInputVisible = false;
            NewTimerHours = "";
            NewTimerMinutes = "";
            NewTimerSeconds = "";
        }
    }

    /// <summary>
    /// Добавляет новый будильник после валидации времени.
    /// </summary>
    private void AddAlarm()
    {
        CorrectAlarmTime();
        if (IsNewAlarmValid)
        {
            TryParseOrZero(NewAlarmHours, out int h);
            TryParseOrZero(NewAlarmMinutes, out int m);
            var ts = new TimeSpan(h, m, 0);
            var alarm = new AlarmEntryViewModel(ts);
            alarm.RequestDelete += a => Alarms.Remove(a);
            alarm.RequestDeactivate += a => a.IsActive = false;
            Alarms.Insert(0, alarm);
            IsAlarmInputVisible = false;
            NewAlarmHours = "";
            NewAlarmMinutes = "";
        }
    }

    /// <summary>
    /// Преобразует строку времени в TimeSpan (форматы: HH:mm, HH:mm:ss, mm:ss, ss).
    /// </summary>
    public static bool TryParseTimeSpan(string input, out TimeSpan result)
    {
        return TimeSpan.TryParse(input, out result);
    }

    private static bool TryParseOrZero(string? value, out int result)
    {
        if (string.IsNullOrWhiteSpace(value)) { result = 0; return true; }
        return int.TryParse(value, out result);
    }

    private void CancelTimerInput()
    {
        IsTimerInputVisible = false;
        NewTimerHours = "";
        NewTimerMinutes = "";
        NewTimerSeconds = "";
    }

    private void CancelAlarmInput()
    {
        IsAlarmInputVisible = false;
        NewAlarmHours = "";
        NewAlarmMinutes = "";
    }

    // Автокоррекция времени для будильника
    public void CorrectAlarmTime()
    {
        if (TryParseOrZero(NewAlarmHours, out var h))
        {
            if (h > 23) NewAlarmHours = "23";
            else if (h < 0) NewAlarmHours = "0";
        }
        if (TryParseOrZero(NewAlarmMinutes, out var m))
        {
            if (m > 59) NewAlarmMinutes = "59";
            else if (m < 0) NewAlarmMinutes = "0";
        }
    }

    /// <summary>
    /// Автоматически корректирует значения часов, минут и секунд для таймера в допустимые диапазоны.
    /// </summary>
    public void CorrectTimerTime()
    {
        if (TryParseOrZero(NewTimerHours, out var h))
        {
            if (h > 23) NewTimerHours = "23";
            else if (h < 0) NewTimerHours = "0";
        }
        if (TryParseOrZero(NewTimerMinutes, out var m))
        {
            if (m > 59) NewTimerMinutes = "59";
            else if (m < 0) NewTimerMinutes = "0";
        }
        if (TryParseOrZero(NewTimerSeconds, out var s))
        {
            if (s > 59) NewTimerSeconds = "59";
            else if (s < 0) NewTimerSeconds = "0";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

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

/// <summary>
/// ViewModel для отдельного будильника.
/// </summary>
public class AlarmEntryViewModel : INotifyPropertyChanged
{
    public TimeSpan AlarmTime { get; set; }
    public bool IsActive { get; set; } = true;
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

    public AlarmEntryViewModel(TimeSpan alarmTime)
    {
        AlarmTime = alarmTime;
        StartCommand = new RelayCommand(_ => { if (IsStartAvailable) Start(); });
        StopCommand = new RelayCommand(_ => { if (IsStopAvailable) Stop(); });
        DeleteCommand = new RelayCommand(_ => RequestDelete?.Invoke(this));
        DeactivateCommand = new RelayCommand(_ => { if (IsHideAvailable) Deactivate(); });
        ToggleWidgetVisibilityCommand = new RelayCommand(_ => ToggleWidgetVisibility());
    }

    public void Start() { IsRunning = true; OnPropertyChanged(nameof(IsStartAvailable)); OnPropertyChanged(nameof(IsStopAvailable)); }
    public void Stop() { IsRunning = false; OnPropertyChanged(nameof(IsStartAvailable)); OnPropertyChanged(nameof(IsStopAvailable)); }
    public void Deactivate() { IsActive = false; OnPropertyChanged(nameof(IsActive)); OnPropertyChanged(nameof(IsStartAvailable)); OnPropertyChanged(nameof(IsStopAvailable)); OnPropertyChanged(nameof(IsHideAvailable)); RequestDeactivate?.Invoke(this); }
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

/// <summary>
/// Простая реализация ICommand для биндинга команд.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}

/// <summary>
/// Инвертирует bool в Visibility.
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => (value is bool b && !b) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Возвращает true, если строка не пуста.
/// </summary>
public class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Конвертер bool -> Opacity (True=1.0, False=0.5)
/// </summary>
public class BooleanToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value is bool b && b) ? 1.0 : 0.5;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
} 