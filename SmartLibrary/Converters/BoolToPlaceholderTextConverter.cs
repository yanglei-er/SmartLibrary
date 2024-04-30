using System.Globalization;
using System.Windows.Data;

namespace SmartLibrary.Converters
{
    internal sealed class BoolToPlaceholderTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "请扫描或输入13位ISBN码";
            }
            else
            {
                return "请输入13位ISBN码";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
