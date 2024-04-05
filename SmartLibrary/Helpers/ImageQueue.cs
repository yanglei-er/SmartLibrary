using SmartLibrary.Models;
using System.IO;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace SmartLibrary.Helpers
{
    public static class ImageQueue
    {
        private static readonly AutoResetEvent autoEvent = new(false);
        private static readonly Queue<ImageQueueInfo> Stacks = new();

        private static readonly LocalStorage _storage = new();
        private static int downloadingCount = 0;

        public delegate void ComplateDelegate(Image image, BitmapImage bitmap);
        public static event ComplateDelegate OnComplate = delegate { };

        static ImageQueue()
        {
            _storage.BookShelfPictureLoadigCompleted += LoadingCompleted;
            Thread thread = new(new ThreadStart(DownloadImage))
            {
                Name = "下载图片",
                IsBackground = true
            };
            thread.Start();
        }

        private static void LoadingCompleted(ImageQueueInfo imageInfo)
        {
            BitmapImage? bitmapImage = null;
            if (imageInfo.Url.StartsWith("pack"))
            {
                bitmapImage = new(new Uri(imageInfo.Url));
            }
            else
            {
                using BinaryReader reader = new(File.Open(imageInfo.Url, FileMode.Open));
                FileInfo fi = new(imageInfo.Url);
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

            if (bitmapImage.CanFreeze)
            {
                bitmapImage.Freeze();
            }

            imageInfo.Image.Dispatcher.BeginInvoke(new Action<ImageQueueInfo, BitmapImage>((image, bitmap) =>
            {
                OnComplate(image.Image, bitmap);
            }), [imageInfo, bitmapImage]);

            downloadingCount--;
            autoEvent.Set();
        }

        private static void DownloadImage()
        {
            while (true)
            {
                lock (Stacks)
                {
                    if (Stacks.Count > 0)
                    {
                        ImageQueueInfo? t = Stacks.Dequeue();
                        _storage.GetBookShelfPicture(t);
                        downloadingCount++;
                    }
                }
                if (Stacks.Count > 0 && downloadingCount < 6)
                {
                    continue;
                }
                else
                {
                    autoEvent.WaitOne();
                }
            }
        }

        public static void Queue(Image img, string isbn, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                lock (Stacks)
                {
                    Stacks.Enqueue(new ImageQueueInfo { Url = url, Isbn = isbn, Image = img });
                    autoEvent.Set();
                }
            }
        }
    }
}
