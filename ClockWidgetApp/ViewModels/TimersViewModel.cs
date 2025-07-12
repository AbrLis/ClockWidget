using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для управления коллекцией таймеров, их добавления, редактирования, удаления, валидации и коррекции времени.
/// </summary>
public class TimersViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Коллекция таймеров.
    /// </summary>
    public ObservableCollection<TimerEntryViewModel> Timers { get; } = new();

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
    /// Команда для отображения поля ввода таймера.
    /// </summary>
    public ICommand ShowTimerInputCommand { get; }
    /// <summary>
    /// Команда для добавления таймера.
    /// </summary>
    public ICommand AddTimerCommand { get; }
    /// <summary>
    /// Команда для отмены ввода времени для таймера.
    /// </summary>
    public ICommand CancelTimerInputCommand { get; }
    /// <summary>
    /// Команда для редактирования таймера.
    /// </summary>
    public ICommand EditTimerCommand { get; }
    /// <summary>
    /// Команда для применения изменений таймера.
    /// </summary>
    public ICommand ApplyEditTimerCommand { get; }
    /// <summary>
    /// Находится ли ViewModel в режиме редактирования таймера.
    /// </summary>
    public bool IsEditingTimer => _editingTimer != null;

    private TimerEntryViewModel? _editingTimer;

    public TimersViewModel()
    {
        NewTimerHours = "";
        NewTimerMinutes = "";
        NewTimerSeconds = "";
        ShowTimerInputCommand = new RelayCommand(_ => IsTimerInputVisible = true);
        AddTimerCommand = new RelayCommand(_ => AddTimer(), _ => IsNewTimerValid);
        CancelTimerInputCommand = new RelayCommand(_ => CancelTimerInput());
        EditTimerCommand = new RelayCommand(t =>
        {
            if (t is TimerEntryViewModel timer && timer != null)
                EditTimer(timer);
        });
        ApplyEditTimerCommand = new RelayCommand(_ => ApplyEditTimer(), _ => IsEditingTimer && IsNewTimerValid);
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
            timer.RequestDeactivate += t => t.IsActive = false;
            Timers.Insert(0, timer);
            IsTimerInputVisible = false;
            NewTimerHours = "";
            NewTimerMinutes = "";
            NewTimerSeconds = "";
        }
    }

    /// <summary>
    /// Переводит ViewModel в режим редактирования выбранного таймера.
    /// </summary>
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

    /// <summary>
    /// Применяет изменения к редактируемому таймеру.
    /// </summary>
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

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 