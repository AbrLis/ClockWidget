namespace ClockWidgetApp.Helpers
{
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Конвертер bool -> Opacity (True=1.0, False=0.5)
    /// </summary>
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? 1.0 : 0.5;
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}