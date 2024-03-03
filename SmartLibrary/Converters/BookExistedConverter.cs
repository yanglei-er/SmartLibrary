using System.Globalization;
using System.Windows.Data;

namespace SmartLibrary.Converters
{
    internal sealed class BookExistedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return "编辑";
            }
            else
            {
                return "添加";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
