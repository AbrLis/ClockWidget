using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Timers;
using ClockWidgetApp.Services;
using System.Windows.Threading;
using ClockWidgetApp.Views;

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
    private bool _notified = false; // Флаг, чтобы не показывать уведомление повторно
    private TimerNotificationWindow? _notificationWindow;

    /// <summary>
    /// Дата и время срабатывания таймера.
    /// </summary>
    public DateTime TargetDateTime
    {
        get => _targetDateTime;
        set { _targetDateTime = value; OnPropertyChanged(); App.MarkTimersAlarmsDirty(); }
    }
    private DateTime _targetDateTime;

    /// <summary>
    /// Оставшееся время до срабатывания таймера.
    /// </summary>
    public TimeSpan Remaining => TargetDateTime - DateTime.Now;

    /// <summary>
    /// Строка для отображения оставшегося времени в формате Xл:Yм:Zд:hh:mm:ss.
    /// </summary>
    public string DisplayTime => FormatRemaining(Remaining);

    /// <summary>
    /// Тултип для иконки трея: имя таймера (до 10 символов), время окончания и оставшееся время (локализовано).
    /// </summary>
    public string TrayTooltip
    {
        get
        {
            string nameLabel = Helpers.LocalizationManager.GetString("LongTimers_Tooltip_NoName");
            string targetLabel = Helpers.LocalizationManager.GetString("LongTimers_Tooltip_Target");
            string remainingLabel = Helpers.LocalizationManager.GetString("LongTimers_Tooltip_Remaining");
            string name = string.IsNullOrWhiteSpace(Name) ? nameLabel : Name;
            // Обрезаем имя до 10 символов, если оно длиннее
            if (name.Length > 10)
                name = name.Substring(0, 10) + "...";
            return $"{name}\n{targetLabel} {TargetDateTime:dd.MM.yyyy HH:mm:ss}\n{remainingLabel} {DisplayTime}";
        }
    }

    /// <summary>
    /// Тултип для окна настроек: имя таймера и время, на которое он был установлен (локализовано).
    /// </summary>
    public string SettingsTooltip
    {
        get
        {
            string nameLabel = Helpers.LocalizationManager.GetString("LongTimers_Tooltip_NoName");
            string targetLabel = Helpers.LocalizationManager.GetString("LongTimers_Tooltip_Target");
            string name = string.IsNullOrWhiteSpace(Name) ? nameLabel : Name;
            return $"{name}\n{targetLabel} {TargetDateTime:dd.MM.yyyy HH:mm:ss}";
        }
    }

    /// <summary>
    /// Команда удаления таймера.
    /// </summary>
    public ICommand DeleteCommand { get; }

    /// <summary>
    /// Событие запроса удаления таймера.
    /// </summary>
    public event Action<LongTimerEntryViewModel>? RequestDelete;

    /// <summary>
    /// Событие, вызываемое при истечении таймера (для удаления из коллекции).
    /// </summary>
    public event Action<LongTimerEntryViewModel>? RequestExpire;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); App.MarkTimersAlarmsDirty(); }
    }
    private string _name = string.Empty;

    public LongTimerEntryViewModel(DateTime targetDateTime, ISoundService soundService, string name = "")
    {
        TargetDateTime = targetDateTime;
        _initialTargetDateTime = targetDateTime;
        _soundService = soundService;
        Name = name;
        DeleteCommand = new RelayCommand(_ =>
        {
            RequestDelete?.Invoke(this);
        });
        _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uiTimer.Tick += (s, e) =>
        {
            OnPropertyChanged(nameof(Remaining));
            OnPropertyChanged(nameof(DisplayTime));
            OnPropertyChanged(nameof(TrayTooltip)); // Обновляем тултип для трея
            if (!_notified && Remaining <= TimeSpan.Zero)
            {
                _notified = true;
                if (_notificationWindow == null)
                {
                    _notificationWindow = ShowLongTimerNotification(_soundService, Name, TargetDateTime, () => {
                        _notificationWindow = null;
                        RequestExpire?.Invoke(this);
                    });
                }
            }
        };
        _uiTimer.Start();
    }

    /// <summary>
    /// Сбрасывает таймер к исходной дате.
    /// </summary>
    public void Reset()
    {
        TargetDateTime = _initialTargetDateTime;
        _notified = false; // Сброс флага при сбросе таймера
        OnPropertyChanged(nameof(TargetDateTime));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(DisplayTime));
        App.MarkTimersAlarmsDirty();
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
    /// Форматирует оставшееся время в строку Xл:Yм:Zд:hh:mm:ss с локализованными суффиксами.
    /// </summary>
    private static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero) return "00:00:00";
        int years = remaining.Days / 365;
        int months = (remaining.Days % 365) / 30;
        int days = (remaining.Days % 365) % 30;
        // Получаем локализованные суффиксы
        string yearsSuffix = Helpers.LocalizationManager.GetString("LongTimers_Years");
        string monthsSuffix = Helpers.LocalizationManager.GetString("LongTimers_Months");
        string daysSuffix = Helpers.LocalizationManager.GetString("LongTimers_Days");
        string result = "";
        if (years > 0) result += $"{years}{yearsSuffix} :";
        if (months > 0) result += $"{months}{monthsSuffix} :";
        if (days > 0) result += $"{days}{daysSuffix} :";
        result += remaining.ToString(@"hh\:mm\:ss");
        return result;
    }

    /// <summary>
    /// Показывает окно уведомления для длинного таймера и воспроизводит long.mp3 (loop = true), с callback после закрытия.
    /// </summary>
    public static TimerNotificationWindow ShowLongTimerNotification(ISoundService soundService, string name, DateTime targetDateTime, Action? onClosed = null)
    {
        var baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        ISoundHandle soundHandle;
        if (!string.IsNullOrEmpty(baseDir))
        {
            string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", "long.mp3");
            soundHandle = soundService.PlaySoundInstance(soundPath, true);
            if (soundHandle == null)
            {
                soundHandle = new NullSoundHandle();
                Serilog.Log.Warning($"[LongTimerEntryViewModel] Не удалось воспроизвести звук long.mp3 для длинного таймера: {name} ({targetDateTime:dd.MM.yyyy HH:mm:ss})");
            }
        }
        else
        {
            soundHandle = new NullSoundHandle();
            Serilog.Log.Warning($"[LongTimerEntryViewModel] Не удалось определить базовую директорию для long.mp3: {name} ({targetDateTime:dd.MM.yyyy HH:mm:ss})");
        }
        var description = $"{(string.IsNullOrWhiteSpace(name) ? "(Без имени)" : name)}\n{targetDateTime:dd.MM.yyyy HH:mm:ss}";
        TimerNotificationWindow notification = ClockWidgetApp.Views.TimerNotificationWindow.CreateWithCloseCallback(soundHandle, description, "timer");
        if (onClosed != null)
        {
            notification.Closed += (s, e) => onClosed();
        }
        notification.Show();
        notification.Topmost = true;
        return notification;
    }
} 