﻿using SmartLibrary.Helpers;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SmartLibrary.Converters
{
    internal sealed class BoolToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/cross.png", UriKind.Absolute));
            }
            else
            {
                return new BitmapImage(new Uri($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/tick.png", UriKind.Absolute));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}