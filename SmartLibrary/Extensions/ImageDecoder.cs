using SmartLibrary.Helpers;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using Image = Wpf.Ui.Controls.Image;

namespace SmartLibrary.Extensions
{
    public partial class ImageDecoder
    {
        #region Isbn Property
        public static readonly DependencyProperty IsbnProperty = DependencyProperty.RegisterAttached("Isbn", typeof(string), typeof(ImageDecoder));

        public static string GetIsbn(Image image)
        {
            return (string)image.GetValue(IsbnProperty);
        }

        public static void SetIsbn(Image image, string value)
        {
            image.SetValue(IsbnProperty, value);
        }
        #endregion Isbn Property

        #region Source Property
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(string), typeof(ImageDecoder), new PropertyMetadata(new PropertyChangedCallback(OnSourceChanged)));

        public static string GetSource(Image image)
        {
            return (string)image.GetValue(SourceProperty);
        }

        public static void SetSource(Image image, string value)
        {
            image.SetValue(SourceProperty, value);
        }
        #endregion Source Property

        static ImageDecoder()
        {
            ImageQueue.OnComplate += ImageQueue_OnComplate;
        }

        private static void ImageQueue_OnComplate(Image i, BitmapImage b)
        {
            i.Source = b;
            Storyboard storyboard = new();
            DoubleAnimation doubleAnimation = new(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(500.0)));
            Storyboard.SetTarget(doubleAnimation, i);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity", []));
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();

            if (i.Parent is Grid grid)
            {
                foreach (object c in grid.Children)
                {
                    if (c is ProgressRing progressring)
                    {
                        progressring.IsIndeterminate = false;
                        progressring.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
            }
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ImageQueue.Queue((Image)o, (string)o.GetValue(IsbnProperty), (string)e.NewValue);
        }
    }
}
