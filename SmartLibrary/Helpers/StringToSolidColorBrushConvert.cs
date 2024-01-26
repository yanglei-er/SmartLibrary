using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartLibrary.Helpers
{
    internal sealed class StringToSolidColorBrushConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
