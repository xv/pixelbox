using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PixelBox.BindingConverters;

internal class ColorToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color c)
            return string.Empty;

        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}