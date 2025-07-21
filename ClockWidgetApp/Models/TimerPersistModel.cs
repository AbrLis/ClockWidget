using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClockWidgetApp.Models
{
    /// <summary>
    /// Модель для сериализации таймера.
    /// </summary>
    public class TimerPersistModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Уникальный идентификатор таймера.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        private TimeSpan _duration;
        /// <summary>
        /// Длительность таймера.
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set { if (_duration != value) { _duration = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Время последнего запуска таймера (UTC). Если null — таймер ни разу не запускался.
        /// </summary>
        public DateTime? LastStartedUtc { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Модель для сериализации будильника.
    /// </summary>
    public class AlarmPersistModel
    {
        /// <summary>
        /// Уникальный идентификатор будильника.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Время срабатывания будильника.
        /// </summary>
        public TimeSpan AlarmTime { get; set; }
        /// <summary>
        /// Включён ли будильник (true = включён, false = выключен).
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// Дата и время следующего срабатывания будильника (если включён).
        /// </summary>
        public DateTime? NextTriggerDateTime { get; set; }
    }
}