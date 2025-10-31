using System.Globalization;
using System.Windows.Data;

namespace PixelBox.BindingConverters;

internal class EllipseRadiusConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || 
            values[0] is not double w || 
            values[1] is not double h)
            return 0.0;

        return Math.Min(w, h) / 2.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}