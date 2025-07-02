using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для управления коллекцией будильников, их добавления, редактирования, удаления, валидации и коррекции времени.
/// </summary>
public class AlarmsViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Коллекция будильников.
    /// </summary>
    public ObservableCollection<AlarmEntryViewModel> Alarms { get; } = new();

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
    /// Команда для отображения поля ввода будильника.
    /// </summary>
    public ICommand ShowAlarmInputCommand { get; }
    /// <summary>
    /// Команда для добавления будильника.
    /// </summary>
    public ICommand AddAlarmCommand { get; }
    /// <summary>
    /// Команда для отмены ввода времени для будильника.
    /// </summary>
    public ICommand CancelAlarmInputCommand { get; }
    /// <summary>
    /// Команда для редактирования будильника.
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

    private AlarmEntryViewModel? _editingAlarm;

    public AlarmsViewModel()
    {
        NewAlarmHours = "";
        NewAlarmMinutes = "";
        ShowAlarmInputCommand = new RelayCommand(_ => IsAlarmInputVisible = true);
        AddAlarmCommand = new RelayCommand(_ => AddAlarm(), _ => IsNewAlarmValid);
        CancelAlarmInputCommand = new RelayCommand(_ => CancelAlarmInput());
        EditAlarmCommand = new RelayCommand(a =>
        {
            if (a is AlarmEntryViewModel alarm && alarm != null)
                EditAlarm(alarm);
        });
        ApplyEditAlarmCommand = new RelayCommand(_ => ApplyEditAlarm(), _ => IsEditingAlarm && IsNewAlarmValid);
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

    /// <summary>
    /// Автоматически корректирует значения часов и минут для будильника в допустимые диапазоны.
    /// </summary>
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

    private static bool TryParseOrZero(string? value, out int result)
    {
        if (string.IsNullOrWhiteSpace(value)) { result = 0; return true; }
        return int.TryParse(value, out result);
    }

    private void CancelAlarmInput()
    {
        IsAlarmInputVisible = false;
        NewAlarmHours = "";
        NewAlarmMinutes = "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 