using System.Globalization;
using System.Windows.Data;

namespace Shared.Converters
{
    public sealed class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Helpers.Utils.StringToImageSource((string)value);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}