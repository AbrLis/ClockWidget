using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;
using Serilog;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для управления коллекцией длинных таймеров, их добавления, редактирования, удаления и управления.
/// </summary>
public class LongTimersViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Коллекция длинных таймеров.
    /// </summary>
    public ObservableCollection<LongTimerEntryViewModel> LongTimers { get; } = new();
    public ObservableCollection<LongTimerPersistModel> LongTimerModels => _appDataService.Data.LongTimers;
    private readonly IAppDataService _appDataService;
    private readonly ISoundService _soundService;

    /// <summary>
    /// Конструктор ViewModel длинных таймеров.
    /// </summary>
    /// <param name="appDataService">Сервис доступа к данным приложения.</param>
    /// <param name="soundService">Сервис воспроизведения звука.</param>
    public LongTimersViewModel(IAppDataService appDataService, ISoundService soundService)
    {
        _appDataService = appDataService;
        _soundService = soundService;
        LongTimers.Clear();
        foreach (var model in LongTimerModels)
        {
            var vm = CreateViewModel(model);
            SubscribeToTimer(vm);
            LongTimers.Add(vm);
        }
        LongTimerModels.CollectionChanged += LongTimerModels_CollectionChanged;
    }

    private LongTimerEntryViewModel CreateViewModel(LongTimerPersistModel model)
    {
        var vm = new LongTimerEntryViewModel(model, _soundService);
        // PropertyChanged больше не нужен для синхронизации, persist обновляется напрямую
        return vm;
    }

    /// <summary>
    /// Подписывается на событие PropertyChanged таймера для автосохранения (debounce).
    /// </summary>
    private void SubscribeToTimer(LongTimerEntryViewModel timer)
    {
        timer.PropertyChanged += Timer_PropertyChanged;
    }

    /// <summary>
    /// Отписывается от события PropertyChanged таймера.
    /// </summary>
    private void UnsubscribeFromTimer(LongTimerEntryViewModel timer)
    {
        timer.PropertyChanged -= Timer_PropertyChanged;
    }

    /// <summary>
    /// Обработчик изменения свойств таймера: вызывает отложенное автосохранение только по ключевым свойствам.
    /// </summary>
    private void Timer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Сохраняем только при изменении ключевых свойств через debounce
        if (e.PropertyName == nameof(LongTimerEntryViewModel.TargetDateTime) ||
            e.PropertyName == nameof(LongTimerEntryViewModel.Name))
        {
            Log.Information($"[LongTimersViewModel] PropertyChanged: {e.PropertyName} у таймера '{(sender as LongTimerEntryViewModel)?.Name}' — вызов debounce-сохранения");
            // Используем ScheduleTimersAndAlarmsSave для предотвращения блокировки UI
            if (_appDataService is AppDataService concreteService)
                concreteService.ScheduleTimersAndAlarmsSave();
            // Если интерфейс, можно добавить метод в IAppDataService
        }
    }

    private void LongTimerModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (LongTimerPersistModel model in e.NewItems)
            {
                var vm = CreateViewModel(model);
                SubscribeToTimer(vm);
                LongTimers.Insert(LongTimerModels.IndexOf(model), vm);
            }
        }
        if (e.OldItems != null)
        {
            foreach (LongTimerPersistModel model in e.OldItems)
            {
                var vm = LongTimers.FirstOrDefault(x => x.TargetDateTime == model.TargetDateTime && x.Name == model.Name);
                if (vm != null)
                {
                    UnsubscribeFromTimer(vm);
                    LongTimers.Remove(vm);
                }
            }
        }
    }

    /// <summary>
    /// Создаёт и позиционирует окно ввода длинного таймера, возвращает окно после показа.
    /// </summary>
    /// <param name="screenPosition">Позиция для центрирования окна (null — по центру владельца).</param>
    /// <param name="timer">ViewModel для редактирования (null — для создания нового).</param>
    /// <returns>Окно LongTimerInputWindow после показа (ShowDialog).</returns>
    private Views.LongTimerInputWindow CreateAndShowInputWindow(System.Windows.Point? screenPosition, LongTimerEntryViewModel? timer = null)
    {
        var inputWindow = new Views.LongTimerInputWindow
        {
            WindowStartupLocation = screenPosition.HasValue ? System.Windows.WindowStartupLocation.Manual : System.Windows.WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Topmost = true
        };
        if (timer != null)
        {
            inputWindow.SelectedDate = timer.TargetDateTime.Date;
            inputWindow.SelectedHour = timer.TargetDateTime.Hour.ToString();
            inputWindow.SelectedMinute = timer.TargetDateTime.Minute.ToString();
            inputWindow.SelectedSecond = timer.TargetDateTime.Second.ToString();
            inputWindow.TimerName = timer.Name;
        }
        if (screenPosition.HasValue)
        {
            // Позиционируем окно после загрузки, чтобы корректно вычислить размеры
            inputWindow.Loaded += (s, e) =>
            {
                inputWindow.Left = screenPosition.Value.X - inputWindow.ActualWidth / 2;
                inputWindow.Top = screenPosition.Value.Y - inputWindow.ActualHeight / 2;
            };
        }
        return inputWindow;
    }

    /// <summary>
    /// Открывает окно для добавления нового длинного таймера так, чтобы центр окна совпадал с позицией клика.
    /// </summary>
    public void ShowInputWindowAt(System.Windows.Point screenPosition)
    {
        var inputWindow = CreateAndShowInputWindow(screenPosition);
        if (inputWindow.ShowDialog() == true)
        {
            var selectedDateTime = inputWindow.SelectedDateTime;
            var timerName = inputWindow.TimerName;
            var persistModel = new LongTimerPersistModel { TargetDateTime = selectedDateTime, Name = timerName };
            LongTimerModels.Add(persistModel);
        }
    }

    /// <summary>
    /// Открывает окно для редактирования существующего длинного таймера так, чтобы центр окна совпадал с позицией клика.
    /// После успешного редактирования сохранение происходит только через debounce PropertyChanged.
    /// </summary>
    public void EditLongTimer(LongTimerEntryViewModel timer, System.Windows.Point? screenPosition = null)
    {
        var inputWindow = CreateAndShowInputWindow(screenPosition, timer);
        if (inputWindow.ShowDialog() == true)
        {
            Log.Information($"[LongTimersViewModel] Начато редактирование таймера '{timer.Name}'");
            timer.TargetDateTime = inputWindow.SelectedDateTime;
            timer.Name = inputWindow.TimerName;
            timer.OnPropertyChanged(nameof(timer.TargetDateTime));
            timer.OnPropertyChanged(nameof(timer.Name));
            timer.OnPropertyChanged(nameof(timer.DisplayTime));
            timer.OnPropertyChanged(nameof(timer.TrayTooltip));
            Log.Information($"[LongTimersViewModel] Завершено редактирование таймера '{timer.Name}', сохранение будет выполнено через debounce PropertyChanged");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}