using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для управления коллекцией таймеров, их добавления, редактирования, удаления, валидации и коррекции времени.
/// </summary>
public class TimersViewModel : INotifyPropertyChanged
{
    private readonly IAppDataService _appDataService;

    /// <summary>
    /// Коллекция ViewModel для UI-таймеров.
    /// </summary>
    public ObservableCollection<TimerEntryViewModel> TimerEntries { get; } = new();

    /// <summary>
    /// Коллекция таймеров из модели данных приложения.
    /// </summary>
    public ObservableCollection<TimerPersistModel> Timers => _appDataService.Data.Timers;

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
        h >= 0 && m is >= 0 and < 60 && s is >= 0 and < 60;

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
    /// Команда для применения изменений таймера.
    /// </summary>
    public ICommand ApplyEditTimerCommand { get; }
    /// <summary>
    /// Находится ли ViewModel в режиме редактирования таймера.
    /// </summary>
    public bool IsEditingTimer => _editingTimer != null;

    private TimerEntryViewModel? _editingTimer;

    /// <summary>
    /// Callback для управления иконкой трея (устанавливается извне).
    /// </summary>
    public Action<TimerEntryViewModel, bool>? OnTimerTrayStateChanged { get; set; }

    public TimersViewModel(IAppDataService appDataService)
    {
        _appDataService = appDataService;
        NewTimerHours = "";
        NewTimerMinutes = "";
        NewTimerSeconds = "";
        ShowTimerInputCommand = new RelayCommand(_ => IsTimerInputVisible = true);
        AddTimerCommand = new RelayCommand(_ => AddTimer(), _ => IsNewTimerValid);
        CancelTimerInputCommand = new RelayCommand(_ => CancelTimerInput());
        ApplyEditTimerCommand = new RelayCommand(_ => ApplyEditTimer(), _ => IsEditingTimer && IsNewTimerValid);
        // Синхронизируем TimerEntries с Timers
        TimerEntries.Clear();
        foreach (var model in Timers)
        {
            var vm = CreateViewModel(model);
            TimerEntries.Add(vm);
        }
        Timers.CollectionChanged += Timers_CollectionChanged;
    }

    private TimerEntryViewModel CreateViewModel(TimerPersistModel model)
    {
        var vm = new TimerEntryViewModel(model);
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.Duration))
                model.Duration = vm.Duration;
            if (e.PropertyName == nameof(vm.IsRunning))
            {
                OnTimerTrayStateChanged?.Invoke(vm, vm.IsRunning);
            }
        };
        return vm;
    }

    private void Timers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TimerPersistModel model in e.NewItems)
            {
                var vm = CreateViewModel(model);
                TimerEntries.Insert(Timers.IndexOf(model), vm);
            }
        }
        if (e.OldItems != null)
        {
            foreach (TimerPersistModel model in e.OldItems)
            {
                var vm = TimerEntries.FirstOrDefault(x => x.Model == model);
                if (vm != null)
                    TimerEntries.Remove(vm);
            }
        }
    }

    /// <summary>
    /// Добавляет новый таймер после валидации времени.
    /// </summary>
    private void AddTimer()
    {
        if (!IsNewTimerValid) return;
        _ = TryParseOrZero(NewTimerHours, out int h);
        _ = TryParseOrZero(NewTimerMinutes, out int m);
        _ = TryParseOrZero(NewTimerSeconds, out int s);
        var ts = new TimeSpan(h, m, s);
        var model = new TimerPersistModel { Duration = ts };
        Timers.Insert(0, model); // вызовет добавление в TimerEntries через CollectionChanged
        IsTimerInputVisible = false;
        NewTimerHours = "";
        NewTimerMinutes = "";
        NewTimerSeconds = "";
    }

    /// <summary>
    /// Применяет изменения к редактируемому таймеру.
    /// </summary>
    private void ApplyEditTimer()
    {
        if (_editingTimer == null || !IsNewTimerValid) return;
        _ = TryParseOrZero(NewTimerHours, out int h);
        _ = TryParseOrZero(NewTimerMinutes, out int m);
        _ = TryParseOrZero(NewTimerSeconds, out int s);
        var ts = new TimeSpan(h, m, s);
        _editingTimer.Duration = ts;
        _editingTimer = null;
        IsTimerInputVisible = false;
        NewTimerHours = "";
        NewTimerMinutes = "";
        NewTimerSeconds = "";
        OnPropertyChanged(nameof(IsEditingTimer));
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

    /// <summary>
    /// Переводит ViewModel в режим редактирования выбранного таймера.
    /// </summary>
    public void EditTimer(TimerPersistModel? model)
    {
        if (model == null) return;
        var vm = TimerEntries.FirstOrDefault(x => x.Duration == model.Duration);
        if (vm == null) return;
        _editingTimer = vm;
        NewTimerHours = vm.Duration.Hours.ToString("D2");
        NewTimerMinutes = vm.Duration.Minutes.ToString("D2");
        NewTimerSeconds = vm.Duration.Seconds.ToString("D2");
        IsTimerInputVisible = true;
        OnPropertyChanged(nameof(IsEditingTimer));
    }

    public void EditTimer(TimerEntryViewModel? vm)
    {
        if (vm == null) return;
        _editingTimer = vm;
        NewTimerHours = vm.Duration.Hours.ToString("D2");
        NewTimerMinutes = vm.Duration.Minutes.ToString("D2");
        NewTimerSeconds = vm.Duration.Seconds.ToString("D2");
        IsTimerInputVisible = true;
        OnPropertyChanged(nameof(IsEditingTimer));
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
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}