using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace SmartLibrary.Helpers
{
    public sealed class LocalStorage
    {
        public static string GetPictureUrl(string isbn, string? path)
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
                image.Mutate(a => a.Resize(new ResizeOptions() { Size = new(180, 260), Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop }));
                _ = image.SaveAsJpegAsync(path, new JpegEncoder() { Quality = 100 });
            }
        }

        public static string GetPictureLocalPath(string isbn, string path)
        {
            string localFilePath = @".\pictures\" + isbn + ".jpg";
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    if (Path.GetDirectoryName(Path.GetFullPath(path)) == Path.GetDirectoryName(Path.GetFullPath(@".\pictures")))
                    {
                        if (Path.GetFileNameWithoutExtension(path) != isbn)
                        {
                            File.Move(path, localFilePath);
                        }
                    }
                    else
                    {
                        using Image image = Image.Load(path);
                        image.Mutate(a => a.Resize(180, 260));
                        _ = image.SaveAsJpegAsync(localFilePath, new JpegEncoder() { Quality = 100 });
                    }
                    return localFilePath;
                }
                else
                {
                    SavePictureAsync(localFilePath, path);
                    return path;
                }
            }
            else
            {
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }
                return string.Empty;
            }
        }
    }
}
