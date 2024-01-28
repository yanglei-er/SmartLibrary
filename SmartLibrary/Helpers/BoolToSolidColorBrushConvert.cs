using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartLibrary.Helpers
{
    internal sealed class BoolToSolidColorBrushConvert : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return new SolidColorBrush(Color.FromRgb(14,176,201));
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
