namespace ClockWidgetApp.Models
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Агрегирующая модель для хранения всех данных приложения: настроек виджета, таймеров, будильников и длинных таймеров.
    /// </summary>
    public class AppDataModel
    {
        /// <summary>
        /// Настройки виджета (основные параметры отображения и поведения).
        /// </summary>
        public WidgetSettings WidgetSettings { get; set; } = new WidgetSettings();

        /// <summary>
        /// Коллекция обычных таймеров.
        /// </summary>
        public ObservableCollection<TimerPersistModel> Timers { get; set; } = new ObservableCollection<TimerPersistModel>();

        /// <summary>
        /// Коллекция будильников.
        /// </summary>
        public ObservableCollection<AlarmPersistModel> Alarms { get; set; } = new ObservableCollection<AlarmPersistModel>();

        /// <summary>
        /// Коллекция длинных таймеров.
        /// </summary>
        public ObservableCollection<LongTimerPersistModel> LongTimers { get; set; } = new ObservableCollection<LongTimerPersistModel>();
    }
} 