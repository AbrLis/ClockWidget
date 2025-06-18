namespace ClockWidgetApp.Models;

/// <summary>
/// Модель для представления риски на циферблате часов.
/// </summary>
public class ClockTick
{
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
    /// Создает новый экземпляр риски.
    /// </summary>
    public ClockTick(double x1, double y1, double x2, double y2, double thickness)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Thickness = thickness;
    }
} 