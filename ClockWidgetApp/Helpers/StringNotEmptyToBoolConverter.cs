using System.Globalization;
using System.Windows.Data;

namespace ClockWidgetApp.Helpers
{
    /// <summary>
    /// Возвращает true, если строка не пуста.
    /// </summary>
    public class StringNotEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string s && !string.IsNullOrWhiteSpace(s);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
} 