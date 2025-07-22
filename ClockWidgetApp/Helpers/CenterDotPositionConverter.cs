using System.Globalization;
using System.Windows.Data;

namespace ClockWidgetApp.Helpers;

/// <summary>
/// Конвертер для вычисления позиции центральной точки аналоговых часов.
/// Вычитает половину размера точки из координат центра для правильного позиционирования.
/// </summary>
public class CenterDotPositionConverter : IValueConverter
{
    /// <summary>
    /// Единственный экземпляр конвертера.
    /// </summary>
    public static readonly CenterDotPositionConverter Instance = new();

    /// <summary>
    /// Приватный конструктор для синглтона.
    /// </summary>
    private CenterDotPositionConverter() { }

    /// <summary>
    /// Преобразует входное значение для позиции точки.
    /// </summary>
    /// <param name="value">Входное значение.</param>
    /// <param name="targetType">Тип назначения.</param>
    /// <param name="parameter">Параметр конвертации.</param>
    /// <param name="culture">Культура.</param>
    /// <returns>Преобразованное значение.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double centerCoordinate)
        {
            // Вычитаем половину размера центральной точки для правильного позиционирования
            return centerCoordinate - AnalogClockConstants.ClockDimensions.CenterDotSize / 2;
        }
        return value;
    }

    /// <summary>
    /// Обратная конвертация (не используется).
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not implemented for CenterDotPositionConverter");
    }
}