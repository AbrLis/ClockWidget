using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Timers;
using ClockWidgetApp.Services;
using System.Windows.Threading;

namespace ClockWidgetApp.ViewModels;

/// <summary>
/// ViewModel для отдельного длинного таймера.
/// </summary>
public class LongTimerEntryViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ISoundService _soundService;
    private DispatcherTimer _uiTimer;
    private readonly DateTime _initialTargetDateTime;
    // Удалена переменная IsActive и все связанные с ней свойства и уведомления.
    // Длинный таймер всегда считается активным.

    /// <summary>
    /// Дата и время срабатывания таймера.
    /// </summary>
    public DateTime TargetDateTime { get; set; }

    /// <summary>
    /// Оставшееся время до срабатывания таймера.
    /// </summary>
    public TimeSpan Remaining => TargetDateTime - DateTime.Now;

    /// <summary>
    /// Строка для отображения оставшегося времени в формате Xл:Yм:Zд:hh:mm:ss.
    /// </summary>
    public string DisplayTime => FormatRemaining(Remaining);

    /// <summary>
    /// Тултип для иконки трея: имя таймера и оставшееся время на второй строке.
    /// </summary>
    public string TrayTooltip => $"{(string.IsNullOrWhiteSpace(Name) ? "(Без имени)" : Name)}\n{DisplayTime}";

    /// <summary>
    /// Команда удаления таймера.
    /// </summary>
    public ICommand DeleteCommand { get; }

    /// <summary>
    /// Событие запроса удаления таймера.
    /// </summary>
    public event Action<LongTimerEntryViewModel>? RequestDelete;

    public string Name { get; set; } = string.Empty;

    public LongTimerEntryViewModel(DateTime targetDateTime, ISoundService soundService, string name = "")
    {
        TargetDateTime = targetDateTime;
        _initialTargetDateTime = targetDateTime;
        _soundService = soundService;
        Name = name;
        DeleteCommand = new RelayCommand(_ => RequestDelete?.Invoke(this));
        _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uiTimer.Tick += (s, e) =>
        {
            OnPropertyChanged(nameof(Remaining));
            OnPropertyChanged(nameof(DisplayTime));
            OnPropertyChanged(nameof(TrayTooltip)); // Обновляем тултип для трея
        };
        _uiTimer.Start();
    }

    /// <summary>
    /// Сбрасывает таймер к исходной дате.
    /// </summary>
    public void Reset()
    {
        TargetDateTime = _initialTargetDateTime;
        OnPropertyChanged(nameof(TargetDateTime));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(DisplayTime));
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(DisplayTime));
        if (Remaining <= TimeSpan.Zero)
        {
            // Воспроизведение звука окончания таймера
            var baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(baseDir))
            {
                string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", "timer.mp3");
                _soundService.PlaySoundInstance(soundPath, true);
            }
            // TODO: Вызвать окно оповещения, как в TimerEntryViewModel
        }
    }

    public void Dispose()
    {
        _uiTimer?.Stop();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Форматирует оставшееся время в строку Xл:Yм:Zд:hh:mm:ss.
    /// </summary>
    private static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero) return "00:00:00";
        int years = remaining.Days / 365;
        int months = (remaining.Days % 365) / 30;
        int days = (remaining.Days % 365) % 30;
        string result = "";
        if (years > 0) result += $"{years}л:";
        if (months > 0) result += $"{months}м:";
        if (days > 0) result += $"{days}д:";
        result += remaining.ToString(@"hh\:mm\:ss");
        return result;
    }
} 