using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Data;
using ClockWidgetApp.Helpers;

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
    private string _newTimerHours = "0";
    public string NewTimerHours { get => _newTimerHours; set { _newTimerHours = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); } }
    private string _newTimerMinutes = "0";
    public string NewTimerMinutes { get => _newTimerMinutes; set { _newTimerMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); } }
    private string _newTimerSeconds = "0";
    public string NewTimerSeconds { get => _newTimerSeconds; set { _newTimerSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); } }
    public bool IsNewTimerValid => int.TryParse(NewTimerHours, out var h) && int.TryParse(NewTimerMinutes, out var m) && int.TryParse(NewTimerSeconds, out var s) && (h > 0 || m > 0 || s > 0) && h >= 0 && m >= 0 && m < 60 && s >= 0 && s < 60;

    /// <summary>
    /// Введённое пользователем время для будильника.
    /// </summary>
    private string _newAlarmHours = "0";
    public string NewAlarmHours { get => _newAlarmHours; set { _newAlarmHours = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewAlarmValid)); } }
    private string _newAlarmMinutes = "0";
    public string NewAlarmMinutes { get => _newAlarmMinutes; set { _newAlarmMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewAlarmValid)); } }
    private string _newAlarmSeconds = "0";
    public string NewAlarmSeconds { get => _newAlarmSeconds; set { _newAlarmSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewAlarmValid)); } }
    public bool IsNewAlarmValid => int.TryParse(NewAlarmHours, out var h) && int.TryParse(NewAlarmMinutes, out var m) && int.TryParse(NewAlarmSeconds, out var s) && (h > 0 || m > 0 || s > 0) && h >= 0 && m >= 0 && m < 60 && s >= 0 && s < 60;

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
            OnPropertyChanged(nameof(NewAlarmSeconds));
        };
        NewTimerHours = "00";
        NewTimerMinutes = "00";
        NewTimerSeconds = "00";
        NewAlarmHours = "00";
        NewAlarmMinutes = "00";
        NewAlarmSeconds = "00";
        ShowTimerInputCommand = new RelayCommand(_ => IsTimerInputVisible = true);
        ShowAlarmInputCommand = new RelayCommand(_ => IsAlarmInputVisible = true);
        AddTimerCommand = new RelayCommand(_ => AddTimer(), _ => IsNewTimerValid);
        AddAlarmCommand = new RelayCommand(_ => AddAlarm(), _ => IsNewAlarmValid);
    }

    /// <summary>
    /// Добавляет новый таймер после валидации времени.
    /// </summary>
    private void AddTimer()
    {
        if (IsNewTimerValid)
        {
            int h = int.Parse(NewTimerHours); int m = int.Parse(NewTimerMinutes); int s = int.Parse(NewTimerSeconds);
            var ts = new TimeSpan(h, m, s);
            var timer = new TimerEntryViewModel(ts);
            Timers.Insert(0, timer);
            IsTimerInputVisible = false;
            NewTimerHours = "0";
            NewTimerMinutes = "0";
            NewTimerSeconds = "0";
        }
    }

    /// <summary>
    /// Добавляет новый будильник после валидации времени.
    /// </summary>
    private void AddAlarm()
    {
        if (IsNewAlarmValid)
        {
            int h = int.Parse(NewAlarmHours); int m = int.Parse(NewAlarmMinutes); int s = int.Parse(NewAlarmSeconds);
            var ts = new TimeSpan(h, m, s);
            var alarm = new AlarmEntryViewModel(ts);
            Alarms.Insert(0, alarm);
            IsAlarmInputVisible = false;
            NewAlarmHours = "0";
            NewAlarmMinutes = "0";
            NewAlarmSeconds = "0";
        }
    }

    /// <summary>
    /// Преобразует строку времени в TimeSpan (форматы: HH:mm, HH:mm:ss, mm:ss, ss).
    /// </summary>
    public static bool TryParseTimeSpan(string input, out TimeSpan result)
    {
        return TimeSpan.TryParse(input, out result);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// ViewModel для отдельного таймера.
/// </summary>
public class TimerEntryViewModel : INotifyPropertyChanged
{
    public TimeSpan Duration { get; }
    private TimeSpan _remaining;
    public TimeSpan Remaining { get => _remaining; set { _remaining = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayTime)); } }
    public bool IsActive { get; set; } = true;
    public string DisplayTime => Remaining.ToString(@"hh\:mm\:ss");
    public TimerEntryViewModel(TimeSpan duration)
    {
        Duration = duration;
        Remaining = duration;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// ViewModel для отдельного будильника.
/// </summary>
public class AlarmEntryViewModel : INotifyPropertyChanged
{
    public TimeSpan AlarmTime { get; }
    public bool IsActive { get; set; } = true;
    public string DisplayTime => AlarmTime.ToString(@"hh\:mm\:ss");
    public AlarmEntryViewModel(TimeSpan alarmTime)
    {
        AlarmTime = alarmTime;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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