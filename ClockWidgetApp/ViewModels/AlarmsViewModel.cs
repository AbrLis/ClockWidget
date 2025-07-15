using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClockWidgetApp.Helpers;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;

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
    public ObservableCollection<AlarmPersistModel> AlarmModels => _appDataService.Data.Alarms;
    private readonly IAppDataService _appDataService;

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
    /// Сообщение о дублирующемся будильнике.
    /// </summary>
    private string _duplicateAlarmNotification = string.Empty;
    public string DuplicateAlarmNotification
    {
        get => _duplicateAlarmNotification;
        set { _duplicateAlarmNotification = value; OnPropertyChanged(); }
    }

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

    public AlarmsViewModel(IAppDataService appDataService)
    {
        _appDataService = appDataService;
        Alarms.Clear();
        foreach (var model in AlarmModels)
            Alarms.Add(CreateViewModel(model));
        AlarmModels.CollectionChanged += AlarmModels_CollectionChanged;
        NewAlarmHours = "";
        NewAlarmMinutes = "";
        ShowAlarmInputCommand = new RelayCommand(_ => IsAlarmInputVisible = true);
        AddAlarmCommand = new RelayCommand(_ => AddAlarm(), _ => IsNewAlarmValid);
        CancelAlarmInputCommand = new RelayCommand(_ => CancelAlarmInput());
        EditAlarmCommand = new RelayCommand(a =>
        {
            if (a is AlarmEntryViewModel alarm)
                EditAlarm(alarm);
        });
        ApplyEditAlarmCommand = new RelayCommand(_ => ApplyEditAlarm(), _ => IsEditingAlarm && IsNewAlarmValid);
    }

    private AlarmEntryViewModel CreateViewModel(AlarmPersistModel model)
    {
        var vm = new AlarmEntryViewModel(model.AlarmTime, model.IsEnabled);
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.AlarmTime))
                model.AlarmTime = vm.AlarmTime;
            if (e.PropertyName == nameof(vm.IsEnabled))
                model.IsEnabled = vm.IsEnabled;
        };
        return vm;
    }

    private void AlarmModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (AlarmPersistModel model in e.NewItems)
            {
                var vm = CreateViewModel(model);
                Alarms.Insert(AlarmModels.IndexOf(model), vm);
            }
        }
        if (e.OldItems != null)
        {
            foreach (AlarmPersistModel model in e.OldItems)
            {
                var vm = Alarms.FirstOrDefault(x => x.AlarmTime == model.AlarmTime && x.IsEnabled == model.IsEnabled);
                if (vm != null)
                    Alarms.Remove(vm);
            }
        }
    }

    /// <summary>
    /// Добавляет новый будильник после валидации времени. Если будильник с таким временем уже есть, не добавляет и уведомляет пользователя.
    /// </summary>
    private void AddAlarm()
    {
        CorrectAlarmTime();
        if (IsNewAlarmValid)
        {
            TryParseOrZero(NewAlarmHours, out int h);
            TryParseOrZero(NewAlarmMinutes, out int m);
            var ts = new TimeSpan(h, m, 0);
            // Проверка на дублирующийся будильник
            if (AlarmModels.Any(a => a.AlarmTime == ts))
            {
                ShowDuplicateAlarmNotification();
                return;
            }
            var persistModel = new AlarmPersistModel { AlarmTime = ts, IsEnabled = false };
            AlarmModels.Insert(0, persistModel);
            IsAlarmInputVisible = false;
            NewAlarmHours = "";
            NewAlarmMinutes = "";
            DuplicateAlarmNotification = string.Empty;
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
            if (_editingAlarm.IsEnabled)
                _editingAlarm.UpdateNextTrigger();
            _editingAlarm.OnPropertyChanged(nameof(_editingAlarm.AlarmTime));
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

    private async void ShowDuplicateAlarmNotification(string? message = null)
    {
        DuplicateAlarmNotification = message ?? LocalizationManager.GetLocalizedStrings().Alarms_DuplicateNotification;
        await Task.Delay(Constants.DuplicateAlarmNotificationDurationMs);
        DuplicateAlarmNotification = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
} 