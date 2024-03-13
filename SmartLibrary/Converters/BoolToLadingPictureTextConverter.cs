using System.Globalization;
using System.Windows.Data;

namespace SmartLibrary.Converters
{
    public sealed class BoolToLadingPictureTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "正在加载图片";
            }
            else
            {
                return "添加图片";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
