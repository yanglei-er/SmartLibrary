using System.Globalization;
using System.Windows.Data;

namespace SmartLibrary.Converters
{
    internal sealed class BoolToAppearanceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Primary";
            }
            else
            {
                return "Secondary";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
