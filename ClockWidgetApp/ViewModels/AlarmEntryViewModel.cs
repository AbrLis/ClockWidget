using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClockWidgetApp.Models;

namespace ClockWidgetApp.ViewModels
{
    /// <summary>
    /// ViewModel для отдельного будильника (только два состояния: включен/выключен).
    /// </summary>
    public class AlarmEntryViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Persist-модель, связанная с этим ViewModel.
        /// </summary>
        public AlarmPersistModel Model { get; }

        /// <summary>
        /// Уникальный идентификатор будильника.
        /// </summary>
        public Guid Id => Model.Id;

        /// <summary>
        /// Время срабатывания будильника.
        /// </summary>
        public TimeSpan AlarmTime
        {
            get => Model.AlarmTime;
            set { Model.AlarmTime = value; OnPropertyChanged(); }
        }
        private bool _isEnabled;
        /// <summary>
        /// Включён ли будильник.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnTrayStateChanged?.Invoke(this, _isEnabled);
                    if (!_isEnabled)
                        NextTriggerDateTime = null;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStartAvailable));
                    OnPropertyChanged(nameof(IsStopAvailable));
                    OnPropertyChanged(nameof(NextTriggerDateTime));
                    OnStateChanged?.Invoke(); // Автосохранение состояния
                }
            }
        }
        /// <summary>
        /// Дата и время следующего срабатывания будильника (если включён).
        /// </summary>
        public DateTime? NextTriggerDateTime { get; private set; }

        /// <summary>
        /// Делегат, вызываемый при изменении состояния активности будильника (для автосохранения).
        /// </summary>
        public Action? OnStateChanged { get; set; }
        /// <summary>
        /// Делегат, вызываемый при изменении состояния активности будильника для управления иконкой трея.
        /// </summary>
        public Action<AlarmEntryViewModel, bool>? OnTrayStateChanged { get; set; }

        /// <summary>
        /// Команда для включения будильника.
        /// </summary>
        public ICommand StartCommand { get; }
        /// <summary>
        /// Команда для выключения будильника.
        /// </summary>
        public ICommand StopCommand { get; }
        /// <summary>
        /// Доступна ли кнопка запуска.
        /// </summary>
        public bool IsStartAvailable => !IsEnabled;
        /// <summary>
        /// Доступна ли кнопка остановки.
        /// </summary>
        public bool IsStopAvailable => IsEnabled;

        public AlarmEntryViewModel(AlarmPersistModel model)
        {
            Model = model;
            AlarmTime = model.AlarmTime;
            IsEnabled = model.IsEnabled;
            NextTriggerDateTime = model.NextTriggerDateTime;
            StartCommand = new RelayCommand(_ => Start(), _ => !IsEnabled);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsEnabled);
            if (IsEnabled && NextTriggerDateTime == null)
                UpdateNextTrigger();
            if (!IsEnabled)
                NextTriggerDateTime = null;
        }

        public void ToggleEnabled()
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled)
                UpdateNextTrigger();
            OnPropertyChanged(nameof(IsEnabled));
            OnPropertyChanged(nameof(IsStartAvailable));
            OnPropertyChanged(nameof(IsStopAvailable));
            OnPropertyChanged(nameof(NextTriggerDateTime));
        }

        public void UpdateNextTrigger()
        {
            if (!IsEnabled)
            {
                NextTriggerDateTime = null;
                OnPropertyChanged(nameof(NextTriggerDateTime));
                return;
            }
            var now = DateTime.Now;
            var todayTrigger = new DateTime(now.Year, now.Month, now.Day, AlarmTime.Hours, AlarmTime.Minutes, 0);
            NextTriggerDateTime = todayTrigger > now ? todayTrigger : todayTrigger.AddDays(1);
            OnPropertyChanged(nameof(NextTriggerDateTime));
        }

        public void Start()
        {
            if (!IsEnabled)
            {
                IsEnabled = true;
                UpdateNextTrigger();
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(IsStartAvailable));
                OnPropertyChanged(nameof(IsStopAvailable));
            }
        }

        public void Stop()
        {
            if (IsEnabled)
            {
                IsEnabled = false;
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(IsStartAvailable));
                OnPropertyChanged(nameof(IsStopAvailable));
            }
        }

        public void ClearNextTriggerDateTime()
        {
            NextTriggerDateTime = null;
            OnPropertyChanged(nameof(NextTriggerDateTime));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}