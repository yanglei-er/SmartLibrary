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

        public delegate void ComplateDelegate(Image i, string u, BitmapImage b);
        public static event ComplateDelegate OnComplate = delegate { };

        static ImageQueue()
        {
            _storage.BookShelfPictureLoadigCompleted += LoadingCompleted;
            Thread t = new(new ThreadStart(DownloadImage))
            {
                Name = "下载图片",
                IsBackground = true
            };
            t.Start();
        }

        private static void LoadingCompleted(ImageQueueInfo t)
        {
            BitmapImage? bitmapImage = null;
            if (!string.IsNullOrEmpty(t.Url))
            {
                using BinaryReader reader = new(File.Open(t.Url, FileMode.Open));

                FileInfo fi = new(t.Url);
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
            if (bitmapImage != null)
            {
                if (bitmapImage.CanFreeze)
                {
                    bitmapImage.Freeze();
                }

                t.Image.Dispatcher.BeginInvoke(new Action<ImageQueueInfo, BitmapImage>((i, bmp) =>
                {
                    OnComplate(i.Image, i.Url, bmp);
                }), [t, bitmapImage]);
            }
            downloadingCount--;
            autoEvent.Set();
        }

        private static void DownloadImage()
        {
            while (true)
            {
                ImageQueueInfo? t = null;
                lock (Stacks)
                {
                    if (Stacks.Count > 0)
                    {
                        t = Stacks.Dequeue();
                    }
                }
                if (t != null)
                {
                    if(downloadingCount < 6)
                    {
                        _storage.GetBookShelfPicture(t);
                        downloadingCount++;
                    }
                    else
                    {
                        autoEvent.WaitOne();
                    }
                }
                if (Stacks.Count > 0)
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
