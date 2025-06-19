namespace ClockWidgetApp.Models;

/// <summary>
/// Класс, представляющий одну риску на циферблате аналоговых часов.
/// </summary>
public class ClockTick
{
    /// <summary>
    /// Создаёт новую риску.
    /// </summary>
    /// <param name="startX">Начальная X.</param>
    /// <param name="startY">Начальная Y.</param>
    /// <param name="endX">Конечная X.</param>
    /// <param name="endY">Конечная Y.</param>
    /// <param name="thickness">Толщина риски.</param>
    public ClockTick(double startX, double startY, double endX, double endY, double thickness)
    {
        X1 = startX;
        Y1 = startY;
        X2 = endX;
        Y2 = endY;
        Thickness = thickness;
    }

    /// <summary>
    /// X-координата начальной точки риски.
    /// </summary>
    public double X1 { get; set; }
    
    /// <summary>
    /// Y-координата начальной точки риски.
    /// </summary>
    public double Y1 { get; set; }
    
    /// <summary>
    /// X-координата конечной точки риски.
    /// </summary>
    public double X2 { get; set; }
    
    /// <summary>
    /// Y-координата конечной точки риски.
    /// </summary>
    public double Y2 { get; set; }
    
    /// <summary>
    /// Толщина риски.
    /// </summary>
    public double Thickness { get; set; }

    /// <summary>
    /// X-координата начала риски.
    /// </summary>
    public double StartX => X1;

    /// <summary>
    /// Y-координата начала риски.
    /// </summary>
    public double StartY => Y1;

    /// <summary>
    /// X-координата конца риски.
    /// </summary>
    public double EndX => X2;

    /// <summary>
    /// Y-координата конца риски.
    /// </summary>
    public double EndY => Y2;
} 