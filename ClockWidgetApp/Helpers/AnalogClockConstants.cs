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
        public const double CLOCK_CENTER_X = 125;
        
        /// <summary>
        /// Y-координата центра циферблата.
        /// </summary>
        public const double CLOCK_CENTER_Y = 125;
        
        /// <summary>
        /// Радиус циферблата в пикселях.
        /// </summary>
        public const double CLOCK_RADIUS = 125;
    }
    
    /// <summary>
    /// Константы размеров рисок на циферблате.
    /// </summary>
    public static class TickSizes
    {
        /// <summary>
        /// Длина часовых рисок (для отметок 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11).
        /// </summary>
        public const double HOUR_TICK_LENGTH = 15;
        
        /// <summary>
        /// Длина минутных рисок (для промежуточных минут).
        /// </summary>
        public const double MINUTE_TICK_LENGTH = 8;
        
        /// <summary>
        /// Толщина часовых рисок.
        /// </summary>
        public const double HOUR_TICK_THICKNESS = 3;
        
        /// <summary>
        /// Толщина минутных рисок.
        /// </summary>
        public const double MINUTE_TICK_THICKNESS = 2;
    }
    
    /// <summary>
    /// Константы размеров циферблата.
    /// </summary>
    public static class ClockDimensions
    {
        /// <summary>
        /// Ширина и высота Canvas для циферблата.
        /// </summary>
        public const double CANVAS_SIZE = 250;
        
        /// <summary>
        /// Отступ от края окна до Canvas.
        /// </summary>
        public const double CANVAS_MARGIN = 25;
        
        /// <summary>
        /// Размер центральной точки циферблата.
        /// </summary>
        public const double CENTER_DOT_SIZE = 6;
    }
} 