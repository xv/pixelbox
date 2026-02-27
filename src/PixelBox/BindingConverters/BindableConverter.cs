
using System.Globalization;
using System.Windows.Data;

namespace PixelBox.BindingConverters;

/// <summary>
/// A <see cref="MultiBinding"/> bridge that allows an <see cref="IValueConverter"/>
/// to be supplied via binding.
/// </summary>
/// 
/// <remarks>
/// The first binding provides the <b>value</b> to convert, and the second
/// binding provides the <b><see cref="IValueConverter"/> instance</b> to apply.
/// </remarks>
internal class BindableConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is not IValueConverter converter)
            return string.Empty;

        return converter.Convert(values[0], targetType, parameter, culture);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}