using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace SmartLibrary.Helpers
{
    public sealed class LocalStorage
    {
        public static string SearchPicture(string isbn, string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string localFilePath = @".\pictures\" + isbn + ".jpg";
                if (File.Exists(localFilePath))
                {
                    return Path.GetFullPath(localFilePath);
                }
                else
                {
                    return path;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private static async void SavePictureAsync(string path, string url)
        {
            byte[] picture = await Network.GetPicture(url);
            if (picture.Length > 0)
            {
                if (!Directory.Exists(@".\pictures"))
                {
                    Directory.CreateDirectory(@".\pictures");
                }
                using Image image = Image.Load(picture);
                image.Mutate(a => a.Resize(new ResizeOptions() { Size = new(270, 390), Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop }));
                _ = image.SaveAsJpegAsync(path, new JpegEncoder() { Quality = 100 });
            }
        }

        public static string AddPicture(string isbn, string? path)
        {
            string localFilePath = @".\pictures\" + isbn + ".jpg";
            if (string.IsNullOrEmpty(path))
            {
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }
                return string.Empty;
            }
            else
            {
                if (File.Exists(path))
                {
                    using Image image = Image.Load(path);
                    image.Mutate(a => a.Resize(new ResizeOptions() { Size = new(270, 390), Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop }));
                    image.SaveAsJpegAsync(localFilePath, new JpegEncoder() { Quality = 100 });
                    return localFilePath;
                }
                else
                {
                    SavePictureAsync(localFilePath, path);
                    return path;
                }
            }
        }

        public static string GetPicture(string isbn, string? path)
        {
            string localFilePath = @".\pictures\" + isbn + ".jpg";
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            else
            {
                if (File.Exists(localFilePath))
                {
                    return Path.GetFullPath(localFilePath);
                }
                else
                {
                    SavePictureAsync(localFilePath, path);
                    return path;
                }
            }
        }
    }
}