using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PixelBox.Demo.BindingConverters;

public class RectangleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not double width ||
            values[1] is not double height)
            return Rect.Empty;

        return new Rect(0, 0, width, height);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}