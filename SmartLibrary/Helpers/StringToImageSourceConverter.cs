using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SmartLibrary.Helpers
{
    internal sealed class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            if (!string.IsNullOrEmpty(path))
            {
                return new BitmapImage(new Uri(path, UriKind.Absolute));
            }
            else
            {
                return null;
            }
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
