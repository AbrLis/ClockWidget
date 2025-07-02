using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

/// <summary>
/// ViewModel для отдельного будильника (только два состояния: включен/выключен).
/// </summary>
public class AlarmEntryViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Время срабатывания будильника.
    /// </summary>
    public TimeSpan AlarmTime { get; set; }
    /// <summary>
    /// Включён ли будильник.
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// Дата и время следующего срабатывания будильника (если включён).
    /// </summary>
    public DateTime? NextTriggerDateTime { get; set; }

    /// <summary>
    /// Команда для включения будильника.
    /// </summary>
    public ICommand StartCommand { get; }
    /// <summary>
    /// Команда для выключения будильника.
    /// </summary>
    public ICommand StopCommand { get; }
    /// <summary>
    /// Доступна ли кнопка запуска.
    /// </summary>
    public bool IsStartAvailable => !IsEnabled;
    /// <summary>
    /// Доступна ли кнопка остановки.
    /// </summary>
    public bool IsStopAvailable => IsEnabled;

    public ICommand ToggleEnabledCommand { get; }

    public AlarmEntryViewModel(TimeSpan alarmTime, bool isEnabled = false, DateTime? nextTriggerDateTime = null)
    {
        AlarmTime = alarmTime;
        IsEnabled = isEnabled;
        NextTriggerDateTime = nextTriggerDateTime;
        ToggleEnabledCommand = new RelayCommand(_ => ToggleEnabled());
        StartCommand = new RelayCommand(_ => Start(), _ => !IsEnabled);
        StopCommand = new RelayCommand(_ => Stop(), _ => IsEnabled);
        if (IsEnabled && NextTriggerDateTime == null)
            UpdateNextTrigger();
    }

    public void ToggleEnabled()
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
            UpdateNextTrigger();
        else
            NextTriggerDateTime = null;
        OnPropertyChanged(nameof(IsEnabled));
        OnPropertyChanged(nameof(NextTriggerDateTime));
        OnPropertyChanged(nameof(IsStartAvailable));
        OnPropertyChanged(nameof(IsStopAvailable));
    }

    public void UpdateNextTrigger()
    {
        var now = DateTime.Now;
        var todayTrigger = new DateTime(now.Year, now.Month, now.Day, AlarmTime.Hours, AlarmTime.Minutes, 0);
        NextTriggerDateTime = todayTrigger > now ? todayTrigger : todayTrigger.AddDays(1);
        OnPropertyChanged(nameof(NextTriggerDateTime));
    }

    public void Start()
    {
        if (!IsEnabled)
        {
            IsEnabled = true;
            UpdateNextTrigger();
            OnPropertyChanged(nameof(IsEnabled));
            OnPropertyChanged(nameof(IsStartAvailable));
            OnPropertyChanged(nameof(IsStopAvailable));
        }
    }

    public void Stop()
    {
        if (IsEnabled)
        {
            IsEnabled = false;
            NextTriggerDateTime = null;
            OnPropertyChanged(nameof(IsEnabled));
            OnPropertyChanged(nameof(IsStartAvailable));
            OnPropertyChanged(nameof(IsStopAvailable));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 