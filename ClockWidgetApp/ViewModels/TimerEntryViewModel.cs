using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Timers;
using ClockWidgetApp.Services;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.ViewModels
{
    /// <summary>
    /// ViewModel для отдельного таймера.
    /// </summary>
    public class TimerEntryViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Private fields
        /// <summary>
        /// Таймер для отсчёта времени.
        /// </summary>
        private System.Timers.Timer? _timer;
        /// <summary>
        /// Оставшееся время таймера.
        /// </summary>
        private TimeSpan _remaining;
        /// <summary>
        /// Флаг, показывающий, запущен ли таймер.
        /// </summary>
        private bool _isRunning;
        /// <summary>
        /// Флаг, показывающий, видим ли виджет таймера.
        /// </summary>
        private bool _isWidgetVisible = true;
        /// <summary>
        /// Активен ли таймер.
        /// </summary>
        private bool _isActive = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Persist-модель, связанная с этим ViewModel.
        /// </summary>
        public TimerPersistModel Model { get; }

        /// <summary>
        /// Уникальный идентификатор таймера.
        /// </summary>
        public Guid Id => Model.Id;

        /// <summary>
        /// Конструктор TimerEntryViewModel.
        /// </summary>
        public TimerEntryViewModel(TimerPersistModel model)
        {
            Model = model;
            Duration = model.Duration;
            Remaining = model.Duration;
            StartCommand = new RelayCommand(_ => { if (IsStartAvailable) Start(); });
            StopCommand = new RelayCommand(_ => { if (IsStopAvailable) Stop(); });
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Длительность таймера.
        /// </summary>
        public TimeSpan Duration
        {
            get => Model.Duration;
            set { Model.Duration = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Оставшееся время таймера.
        /// </summary>
        public TimeSpan Remaining
        {
            get => _remaining;
            set { _remaining = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayTime)); }
        }

        /// <summary>
        /// Активен ли таймер.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Строка для отображения оставшегося времени.
        /// </summary>
        public string DisplayTime => Remaining.ToString(@"hh\:mm\:ss");

        /// <summary>
        /// Команда запуска таймера.
        /// </summary>
        public ICommand StartCommand { get; }
        /// <summary>
        /// Команда остановки таймера.
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Флаг, показывающий, запущен ли таймер.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Доступен ли запуск таймера.
        /// </summary>
        public bool IsStartAvailable => !IsRunning;
        /// <summary>
        /// Доступна ли остановка таймера.
        /// </summary>
        public bool IsStopAvailable => IsRunning && IsActive;
        /// <summary>
        /// Доступно ли скрытие таймера.
        /// </summary>
        public bool IsHideAvailable => !IsWidgetVisible;

        /// <summary>
        /// Видим ли виджет таймера.
        /// </summary>
        public bool IsWidgetVisible
        {
            get => _isWidgetVisible;
            set { _isWidgetVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsHideAvailable)); }
        }

        /// <summary>
        /// Событие запроса деактивации таймера.
        /// </summary>
        public event Action<TimerEntryViewModel>? RequestDeactivate;
        /// <summary>
        /// Событие, возникающее при запуске таймера.
        /// </summary>
        public event Action<TimerEntryViewModel>? Started;
        #endregion

        #region Public methods
        /// <summary>
        /// Запускает таймер.
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            if (Remaining <= TimeSpan.Zero) return;
            IsActive = true;
            IsRunning = true;
            // Обновляем время последнего запуска
            Model.LastStartedUtc = DateTime.UtcNow;
            OnPropertyChanged(nameof(IsStartAvailable));
            OnPropertyChanged(nameof(IsStopAvailable));
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += Timer_Elapsed;
                _timer.AutoReset = true;
            }
            _timer.Start();
            // Оповещаем подписчиков о запуске
            Started?.Invoke(this);
        }

        /// <summary>
        /// Останавливает таймер.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnPropertyChanged(nameof(IsStartAvailable));
            OnPropertyChanged(nameof(IsStopAvailable));
            _timer?.Stop();
        }

        /// <summary>
        /// Деактивирует таймер (например, скрывает его виджет).
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(IsStartAvailable));
            OnPropertyChanged(nameof(IsStopAvailable));
            OnPropertyChanged(nameof(IsHideAvailable));
            RequestDeactivate?.Invoke(this);
        }

        /// <summary>
        /// Сбрасывает таймер к начальному значению и останавливает его.
        /// </summary>
        public void Reset()
        {
            Stop();
            Remaining = Duration;
            OnPropertyChanged(nameof(Remaining));
            OnPropertyChanged(nameof(DisplayTime));
        }

        /// <summary>
        /// Освобождает ресурсы таймера.
        /// </summary>
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
        #endregion

        #region Event handlers and private methods
        /// <summary>
        /// Обработчик тика таймера.
        /// </summary>
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (Remaining.TotalSeconds > 0)
            {
                Remaining = Remaining - TimeSpan.FromSeconds(1);
            }
            if (Remaining.TotalSeconds <= 0)
            {
                Remaining = TimeSpan.Zero;
                Stop();
                Remaining = Duration;
                // Воспроизведение звука и показ окна уведомления
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var app = System.Windows.Application.Current as App;
                    if (app?.Services is not { } services)
                        return;
                    var soundService = services.GetService(typeof(ISoundService)) as ISoundService;
                    if (soundService == null)
                        return;
                    var baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    if (string.IsNullOrEmpty(baseDir))
                        return;
                    string soundPath = System.IO.Path.Combine(baseDir, "Resources", "Sounds", "timer.mp3");
                    var soundHandle = soundService.PlaySoundInstance(soundPath, true);
                    var notification = Views.TimerNotificationWindow.CreateWithCloseCallback(soundHandle, Duration.ToString(@"hh\:mm\:ss"));
                    notification.Show();
                });
            }
            OnPropertyChanged(nameof(DisplayTime));
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Уведомляет об изменении свойства для биндинга.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}