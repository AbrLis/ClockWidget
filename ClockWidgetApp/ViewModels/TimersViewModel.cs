using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;
using Microsoft.Extensions.Logging;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для управления коллекцией таймеров, их добавления, редактирования, удаления, валидации и коррекции времени.
/// </summary>
public class TimersViewModel : INotifyPropertyChanged
{
    #region Private Fields
    /// <summary>
    /// Сервис доступа к данным приложения.
    /// </summary>
    private readonly IAppDataService _appDataService;

    /// <summary>
    /// Логгер для записи ключевых событий.
    /// </summary>
    private readonly ILogger<TimersViewModel>? _logger;

    /// <summary>
    /// Признак отображения поля ввода таймера.
    /// </summary>
    private bool _isTimerInputVisible;

    /// <summary>
    /// Введённые пользователем часы для таймера.
    /// </summary>
    private string _newTimerHours = string.Empty;
    /// <summary>
    /// Введённые пользователем минуты для таймера.
    /// </summary>
    private string _newTimerMinutes = string.Empty;
    /// <summary>
    /// Введённые пользователем секунды для таймера.
    /// </summary>
    private string _newTimerSeconds = string.Empty;

    /// <summary>
    /// ViewModel редактируемого таймера.
    /// </summary>
    private TimerEntryViewModel? _editingTimer;
    #endregion

    #region Constructor
    /// <summary>
    /// Конструктор TimersViewModel.
    /// </summary>
    /// <param name="appDataService">Сервис данных приложения.</param>
    /// <param name="logger">Логгер для событий (опционально).</param>
    public TimersViewModel(IAppDataService appDataService, ILogger<TimersViewModel>? logger = null)
    {
        _appDataService = appDataService;
        _logger = logger;
        ShowTimerInputCommand = new RelayCommand(_ => IsTimerInputVisible = true);
        AddTimerCommand = new RelayCommand(_ => AddTimer(), _ => IsNewTimerValid);
        CancelTimerInputCommand = new RelayCommand(_ => CancelTimerInput());
        ApplyEditTimerCommand = new RelayCommand(_ => ApplyEditTimer(), _ => IsEditingTimer && IsNewTimerValid);

        // Сортируем Timers по LastStartedUtc (null — в конец)
        var sorted = Timers.OrderByDescending(t => t.LastStartedUtc ?? DateTime.MinValue).ToList();
        Timers.Clear();
        foreach (var t in sorted)
            Timers.Add(t);

        TimerEntries.Clear();
        foreach (var model in Timers)
        {
            var vm = CreateViewModel(model);
            TimerEntries.Add(vm);
        }
        Timers.CollectionChanged += Timers_CollectionChanged;
        _logger?.LogInformation("TimersViewModel инициализирован.");
    }
    #endregion

    #region Public Properties
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
    public bool IsTimerInputVisible
    {
        get => _isTimerInputVisible;
        set { _isTimerInputVisible = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Введённое пользователем время для таймера (часы).
    /// </summary>
    public string NewTimerHours
    {
        get => _newTimerHours;
        set { _newTimerHours = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); }
    }
    /// <summary>
    /// Введённое пользователем время для таймера (минуты).
    /// </summary>
    public string NewTimerMinutes
    {
        get => _newTimerMinutes;
        set { _newTimerMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); }
    }
    /// <summary>
    /// Введённое пользователем время для таймера (секунды).
    /// </summary>
    public string NewTimerSeconds
    {
        get => _newTimerSeconds;
        set { _newTimerSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNewTimerValid)); }
    }

    /// <summary>
    /// Валидно ли введённое время для нового таймера.
    /// </summary>
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

    /// <summary>
    /// Callback для управления иконкой трея (устанавливается извне).
    /// </summary>
    public Action<TimerEntryViewModel, bool>? OnTimerTrayStateChanged { get; set; }
    #endregion

    #region Public Methods
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
    /// Переводит ViewModel в режим редактирования выбранного таймера по модели.
    /// </summary>
    /// <param name="model">Модель таймера.</param>
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
        _logger?.LogInformation($"Редактирование таймера: {vm.Duration}");
    }

    /// <summary>
    /// Переводит ViewModel в режим редактирования выбранного таймера по ViewModel.
    /// </summary>
    /// <param name="vm">ViewModel таймера.</param>
    public void EditTimer(TimerEntryViewModel? vm)
    {
        if (vm == null) return;
        _editingTimer = vm;
        NewTimerHours = vm.Duration.Hours.ToString("D2");
        NewTimerMinutes = vm.Duration.Minutes.ToString("D2");
        NewTimerSeconds = vm.Duration.Seconds.ToString("D2");
        IsTimerInputVisible = true;
        OnPropertyChanged(nameof(IsEditingTimer));
        _logger?.LogInformation($"Редактирование таймера: {vm.Duration}");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Создаёт ViewModel для модели таймера и подписывает обработчики.
    /// </summary>
    /// <param name="model">Модель таймера.</param>
    /// <returns>ViewModel таймера.</returns>
    private TimerEntryViewModel CreateViewModel(TimerPersistModel model)
    {
        var vm = new TimerEntryViewModel(model);
        vm.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(vm.Duration):
                    model.Duration = vm.Duration;
                    break;
                case nameof(vm.IsRunning):
                    OnTimerTrayStateChanged?.Invoke(vm, vm.IsRunning);
                    break;
            }
        };
        // Подписка на запуск таймера
        vm.Started += MoveTimerToTop;
        return vm;
    }

    /// <summary>
    /// Обработчик изменения коллекции таймеров.
    /// </summary>
    private void Timers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
        {
            // Синхронизируем порядок в TimerEntries
            if (e.NewItems?.Count > 0 && e.NewItems[0] is TimerPersistModel model)
            {
                var vm = TimerEntries.FirstOrDefault(x => x.Model == model);
                if (vm != null)
                {
                    int oldIndex = e.OldStartingIndex;
                    int newIndex = e.NewStartingIndex;
                    if (oldIndex != newIndex)
                        TimerEntries.Move(oldIndex, newIndex);
                }
            }
            return;
        }
        if (e.NewItems != null)
        {
            foreach (TimerPersistModel model in e.NewItems)
            {
                var vm = CreateViewModel(model);
                TimerEntries.Insert(Timers.IndexOf(model), vm);
            }
        }

        if (e.OldItems == null) return;
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
    /// Добавляет новый таймер после валидации времени и сортирует коллекцию через Move: запущенные таймеры вверху, новый — после них.
    /// </summary>
    private void AddTimer()
    {
        if (!IsNewTimerValid) return;
        _ = TryParseOrZero(NewTimerHours, out int h);
        _ = TryParseOrZero(NewTimerMinutes, out int m);
        _ = TryParseOrZero(NewTimerSeconds, out int s);
        var ts = new TimeSpan(h, m, s);
        var model = new TimerPersistModel { Duration = ts };
        Timers.Add(model); // Добавляем в конец
        // Находим все запущенные таймеры
        var running = Timers.Where(t => TimerEntries.FirstOrDefault(vm => vm.Model == t)?.IsRunning == true).ToList();
        int targetIndex = running.Count; // Новый таймер должен быть после всех запущенных
        int currentIndex = Timers.IndexOf(model);
        if (currentIndex != targetIndex)
            Timers.Move(currentIndex, targetIndex);
        IsTimerInputVisible = false;
        NewTimerHours = string.Empty;
        NewTimerMinutes = string.Empty;
        NewTimerSeconds = string.Empty;
        _logger?.LogInformation($"Добавлен таймер: {ts}");
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
        NewTimerHours = string.Empty;
        NewTimerMinutes = string.Empty;
        NewTimerSeconds = string.Empty;
        OnPropertyChanged(nameof(IsEditingTimer));
        _logger?.LogInformation($"Изменён таймер: {ts}");
    }

    /// <summary>
    /// Преобразует строку в int, возвращая 0 при ошибке.
    /// </summary>
    /// <param name="value">Строка для преобразования.</param>
    /// <param name="result">Результат преобразования.</param>
    /// <returns>True, если преобразование успешно или строка пуста.</returns>
    private static bool TryParseOrZero(string? value, out int result)
    {
        if (string.IsNullOrWhiteSpace(value)) { result = 0; return true; }
        return int.TryParse(value, out result);
    }

    /// <summary>
    /// Отмена ввода времени для таймера.
    /// </summary>
    private void CancelTimerInput()
    {
        IsTimerInputVisible = false;
        NewTimerHours = string.Empty;
        NewTimerMinutes = string.Empty;
        NewTimerSeconds = string.Empty;
        _logger?.LogInformation("Ввод таймера отменён.");
    }

    /// <summary>
    /// Перемещает указанный таймер и его persist-модель наверх коллекций и сохраняет порядок.
    /// </summary>
    /// <param name="timerVm">ViewModel таймера.</param>
    private void MoveTimerToTop(TimerEntryViewModel timerVm)
    {
        var model = timerVm.Model;
        int oldIndex = Timers.IndexOf(model);
        if (oldIndex > 0)
        {
            Timers.Move(oldIndex, 0);
            TimerEntries.Move(TimerEntries.IndexOf(timerVm), 0);
            // Сохраняем только timers/alarms, не настройки виджета
            _appDataService.ScheduleTimersAndAlarmsSave();
        }
    }
    #endregion

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Уведомляет об изменении свойства для биндинга.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
}