using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockWidgetApp.Models;
using ClockWidgetApp.Services;

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
            LongTimers.Add(CreateViewModel(model));
        LongTimerModels.CollectionChanged += LongTimerModels_CollectionChanged;
    }

    private LongTimerEntryViewModel CreateViewModel(LongTimerPersistModel model)
    {
        var vm = new LongTimerEntryViewModel(model.TargetDateTime, _soundService, model.Name);
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.Duration))
                model.Duration = vm.Duration;
            if (e.PropertyName == nameof(vm.Name))
                model.Name = vm.Name;
        };
        return vm;
    }

    private void LongTimerModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (LongTimerPersistModel model in e.NewItems)
            {
                var vm = CreateViewModel(model);
                LongTimers.Insert(LongTimerModels.IndexOf(model), vm);
            }
        }
        if (e.OldItems != null)
        {
            foreach (LongTimerPersistModel model in e.OldItems)
            {
                var vm = LongTimers.FirstOrDefault(x => x.Duration == model.Duration && x.Name == model.Name);
                if (vm != null)
                    LongTimers.Remove(vm);
            }
        }
    }

    /// <summary>
    /// Открывает окно для добавления нового длинного таймера над указанной позицией (например, над кнопкой '+').
    /// </summary>
    public void ShowInputWindowAt(System.Windows.Point screenPosition)
    {
        var inputWindow = new Views.LongTimerInputWindow
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.Manual
        };
        // Окно появляется строго над кнопкой: центрируем по X, нижний край окна совпадает с верхом кнопки
        inputWindow.Left = screenPosition.X - inputWindow.Width / 2;
        inputWindow.Top = screenPosition.Y - inputWindow.Height;
        inputWindow.ShowInTaskbar = false;
        inputWindow.Topmost = true;
        if (inputWindow.ShowDialog() == true)
        {
            var selectedDateTime = inputWindow.SelectedDateTime;
            var timerName = inputWindow.TimerName;
            var timer = new LongTimerEntryViewModel(selectedDateTime, _soundService, timerName);
            LongTimers.Add(timer);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}