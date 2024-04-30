using System.Globalization;
using System.Windows.Data;

namespace SmartLibrary.Converters
{
    internal sealed class BoolToBorrowOrReturnTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "还书";
            }
            else
            {
                return "借阅";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
