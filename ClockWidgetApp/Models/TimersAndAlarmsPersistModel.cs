namespace ClockWidgetApp.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Модель для сериализации всех таймеров, будильников и длинных таймеров.
    /// </summary>
    public class TimersAndAlarmsPersistModel
    {
        /// <summary>Коллекция обычных таймеров.</summary>
        public List<TimerPersistModel> Timers { get; set; } = new();
        /// <summary>Коллекция будильников.</summary>
        public List<AlarmPersistModel> Alarms { get; set; } = new();
        /// <summary>Коллекция длинных таймеров.</summary>
        public List<LongTimerPersistModel> LongTimers { get; set; } = new();
    }
} 