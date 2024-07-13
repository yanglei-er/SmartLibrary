using Shared.Helpers;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Shared.Converters
{
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageProcess.BitmapToBitmapImage((Bitmap)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
