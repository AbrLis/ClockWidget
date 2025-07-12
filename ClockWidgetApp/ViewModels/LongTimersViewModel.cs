using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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

    private readonly ISoundService _soundService;

    /// <summary>
    /// Команда для отображения окна добавления длинного таймера.
    /// </summary>
    public ICommand ShowInputCommand { get; }

    /// <summary>
    /// Конструктор ViewModel длинных таймеров.
    /// </summary>
    public LongTimersViewModel(ISoundService soundService)
    {
        _soundService = soundService;
        ShowInputCommand = new RelayCommand(_ => ShowInputWindow());
    }

    /// <summary>
    /// Открывает окно для добавления нового длинного таймера по центру экрана.
    /// </summary>
    private void ShowInputWindow()
    {
        var inputWindow = new Views.LongTimerInputWindow();
        inputWindow.ShowInTaskbar = false;
        if (inputWindow.ShowDialog() == true)
        {
            var selectedDateTime = inputWindow.SelectedDateTime;
            var timerName = inputWindow.TimerName;
            var timer = new LongTimerEntryViewModel(selectedDateTime, _soundService, timerName);
            LongTimers.Add(timer);
        }
    }

    /// <summary>
    /// Открывает окно для добавления нового длинного таймера над указанной позицией (например, над кнопкой '+').
    /// </summary>
    public void ShowInputWindowAt(System.Windows.Point screenPosition)
    {
        var inputWindow = new Views.LongTimerInputWindow();
        inputWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
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