namespace ClockWidgetApp.Models
{
    using System;

    /// <summary>
    /// Модель для сериализации длинного таймера. Сохраняет только дату и время окончания.
    /// </summary>
    public class LongTimerPersistModel
    {
        /// <summary>
        /// Уникальный идентификатор длинного таймера.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Дата и время окончания длинного таймера.
        /// </summary>
        public DateTime TargetDateTime { get; set; }

        /// <summary>
        /// Название длинного таймера.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
} 