using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PixelBox.BindingConverters;

internal class CenterPointConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not double x ||
            values[1] is not double y)
            return new Point(0, 0);

        return new Point(x / 2.0, y / 2.0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => 
        throw new NotSupportedException();
}