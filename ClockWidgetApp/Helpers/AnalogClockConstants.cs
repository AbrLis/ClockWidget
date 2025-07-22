namespace ClockWidgetApp.Helpers;

/// <summary>
/// Константы для аналоговых часов.
/// </summary>
public static class AnalogClockConstants
{
    /// <summary>
    /// Константы позиционирования циферблата.
    /// </summary>
    public static class Positioning
    {
        /// <summary>
        /// X-координата центра циферблата.
        /// </summary>
        public const double ClockCenterX = 125;

        /// <summary>
        /// Y-координата центра циферблата.
        /// </summary>
        public const double ClockCenterY = 125;

        /// <summary>
        /// Радиус циферблата в пикселях.
        /// </summary>
        public const double ClockRadius = 125;
    }

    /// <summary>
    /// Константы размеров рисок на циферблате.
    /// </summary>
    public static class TickSizes
    {
        /// <summary>
        /// Длина часовых рисок (для отметок 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11).
        /// </summary>
        public const double HourTickLength = 15;

        /// <summary>
        /// Длина минутных рисок (для промежуточных минут).
        /// </summary>
        public const double MinuteTickLength = 8;

        /// <summary>
        /// Толщина часовых рисок.
        /// </summary>
        public const double HourTickThickness = 3;

        /// <summary>
        /// Толщина минутных рисок.
        /// </summary>
        public const double MinuteTickThickness = 2;
    }

    /// <summary>
    /// Константы размеров циферблата.
    /// </summary>
    public static class ClockDimensions
    {
        /// <summary>
        /// Размер центральной точки циферблата.
        /// </summary>
        public const double CenterDotSize = 6;

        /// <summary>
        /// Ширина и высота Canvas для циферблата.
        /// </summary>
        public const double CanvasSize = 250;

        /// <summary>
        /// Отступ от края окна до Canvas.
        /// </summary>
        public const double CanvasMargin = 25;
    }
}