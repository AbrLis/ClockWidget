using System;
using System.Globalization;
using System.Windows.Data;

namespace ClockWidgetApp.Helpers;

/// <summary>
/// Конвертер для позиционирования центральной точки циферблата.
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
    /// Конвертирует координату центра в позицию для отображения точки.
    /// </summary>
    /// <param name="value">Координата центра (X или Y).</param>
    /// <param name="targetType">Тип целевого значения.</param>
    /// <param name="parameter">Параметр конвертации.</param>
    /// <param name="culture">Культура.</param>
    /// <returns>Позиция для отображения точки.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double centerCoordinate)
        {
            // Вычитаем половину размера центральной точки для правильного позиционирования
            return centerCoordinate - AnalogClockConstants.ClockDimensions.CENTER_DOT_SIZE / 2;
        }
        
        return value;
    }

    /// <summary>
    /// Обратная конвертация (не используется).
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not implemented for CenterDotPositionConverter");
    }
} 