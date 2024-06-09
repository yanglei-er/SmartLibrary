using System.Globalization;
using System.Windows.Data;

namespace Shared.Converters
{
    public sealed class IsCustomizedAccentColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                if ((string)parameter == "System")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if ((string)parameter == "System")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "System")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
