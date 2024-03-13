using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SmartLibrary.Converters
{
    internal sealed class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            BitmapImage? bitmapImage = null;
            if (!string.IsNullOrEmpty(path))
            {
                using BinaryReader reader = new(File.Open(path, FileMode.Open));

                FileInfo fi = new(path);
                byte[] bytes = reader.ReadBytes((int)fi.Length);
                reader.Close();

                bitmapImage = new()
                {
                    CacheOption = BitmapCacheOption.OnLoad
                };

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(bytes);
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}