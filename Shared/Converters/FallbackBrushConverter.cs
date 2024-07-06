using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Shared.Converters;

public class FallbackBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            SolidColorBrush brush => brush,
            Color color => new SolidColorBrush(color),
            _ => new SolidColorBrush(Colors.Red)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
