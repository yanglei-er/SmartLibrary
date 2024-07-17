using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Shared.Helpers
{
    public static class ImageProcess
    {
        public static BitmapImage? StringToBitmapImage(string path)
        {
            BitmapImage? bitmapImage = null;
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith("pack"))
                {
                    bitmapImage = new(new Uri(path));
                }
                else
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
                if (bitmapImage.CanFreeze)
                {
                    bitmapImage.Freeze();
                }
            }
            return bitmapImage;
        }

        public static Bitmap Resize(Image image, int maxWidth, int maxHeight)
        {
            //模版的宽高比例
            double resultRate = (double)maxWidth / maxHeight;
            //原图片的宽高比例
            double initRate = (double)image.Width / image.Height;

            //原图与模版比例相等，直接缩放
            if (resultRate == initRate)
            {
                //按模版大小生成最终图片
                Bitmap resultImage = new(maxWidth, maxHeight);
                using Graphics graphics = Graphics.FromImage(resultImage);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.Clear(System.Drawing.Color.Transparent);
                graphics.DrawImage(image, new Rectangle(-1, -1, maxWidth, maxHeight), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                return resultImage;
            }
            //原图与模版比例不等，裁剪后缩放
            else
            {
                //裁剪对象
                Bitmap tempImage;
                Graphics tempGraphics;

                //定位
                Rectangle fromR = new(0, 0, 0, 0);//原图裁剪定位
                Rectangle toR = new(0, 0, 0, 0);//目标定位

                //宽为标准进行裁剪
                if (resultRate > initRate)
                {
                    //裁剪对象实例化
                    tempImage = new Bitmap(image.Width, (int)Math.Floor(image.Width / resultRate));
                    tempGraphics = Graphics.FromImage(tempImage);

                    //裁剪源定位
                    fromR.X = 0;
                    fromR.Y = (int)Math.Floor((image.Height - image.Width / resultRate) / 2);
                    fromR.Width = image.Width;
                    fromR.Height = (int)Math.Floor(image.Width / resultRate);

                    //裁剪目标定位
                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = image.Width;
                    toR.Height = (int)Math.Floor(image.Width / resultRate);
                }
                //高为标准进行裁剪
                else
                {
                    tempImage = new Bitmap((int)Math.Floor(image.Height * resultRate), image.Height);
                    tempGraphics = Graphics.FromImage(tempImage);

                    fromR.X = (int)Math.Floor((image.Width - image.Height * resultRate) / 2);
                    fromR.Y = 0;
                    fromR.Width = (int)Math.Floor(image.Height * resultRate);
                    fromR.Height = image.Height;

                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = (int)Math.Floor(image.Height * resultRate);
                    toR.Height = image.Height;
                }

                //设置质量
                tempGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                tempGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //裁剪
                tempGraphics.DrawImage(image, toR, fromR, GraphicsUnit.Pixel);

                //按模版大小生成最终图片
                Bitmap resultImage = new(maxWidth, maxHeight);
                using Graphics resultGraphics = Graphics.FromImage(resultImage);
                resultGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                resultGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                resultGraphics.Clear(System.Drawing.Color.Transparent);
                resultGraphics.DrawImage(tempImage, new Rectangle(0, 0, maxWidth, maxHeight), new Rectangle(0, 0, tempImage.Width, tempImage.Height), GraphicsUnit.Pixel);

                tempGraphics.Dispose();
                tempImage.Dispose();
                return resultImage;
            }
        }

        public static void DrawText(this Bitmap image, string text, int x, int y)
        {
            using Graphics graphics = Graphics.FromImage(image);
            using Font font = new("Microsoft YaHei", 16);
            using SolidBrush brush = new(System.Drawing.Color.Red);
            graphics.DrawString(text, font, brush, x-6, y-40);
        }

        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            Bitmap bmp = new(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride); bmp.UnlockBits(data);

            return bmp;
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Jpeg);

            stream.Position = 0;
            BitmapImage result = new();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }

        public static BitmapImage BitmapToPngBitmapImage(Bitmap bitmap)
        {
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Png);

            stream.Position = 0;
            BitmapImage result = new();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {

            using MemoryStream outStream = new();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            Bitmap bitmap = new(outStream);

            return bitmap;
        }

        public static byte[] BitmapImageToByte(BitmapImage bmp)
        {
            byte[] buffer = [];
            JpegBitmapEncoder encoder = new();
            using MemoryStream memoryStream = new();
            encoder.Frames.Add(BitmapFrame.Create(bmp.StreamSource));
            encoder.Save(memoryStream);
            memoryStream.Position = 0;
            if (memoryStream.Length > 0)
            {
                using BinaryReader br = new(memoryStream);
                buffer = br.ReadBytes((int)memoryStream.Length);
            }
            return buffer;
        }

        public static BitmapImage ByteToBitmapImage(byte[] bytes)
        {
            BitmapImage bitmapImage = new()
            {
                CacheOption = BitmapCacheOption.OnLoad
            };
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(bytes);
            bitmapImage.EndInit();

            if (bitmapImage.CanFreeze)
            {
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
    }
}
