using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PixelBox.BindingConverters;

internal class PointToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Point p)
            return string.Empty;

        return $"X:{p.X} Y:{p.Y}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}