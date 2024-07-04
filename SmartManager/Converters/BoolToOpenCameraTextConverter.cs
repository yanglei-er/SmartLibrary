using System.Globalization;
using System.Windows.Data;

namespace SmartManager.Converters
{
    public sealed class BoolToOpenCameraTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "关闭摄像头";
            }
            else
            {
                return "打开摄像头";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
