using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SmartLibrary.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace SmartLibrary.Helpers
{
    public sealed partial class LocalStorage
    {
        [GeneratedRegex(@"^https?:\/\/", RegexOptions.IgnoreCase, "zh-CN")]
        private static partial Regex UriRegex();

        public delegate void LoadingCompletedEventHandler(string path);
        public event LoadingCompletedEventHandler LoadingCompleted = delegate { };

        public delegate void BookShelfPictureLoadingCompletedEventHandler(ImageQueueInfo t);
        public event BookShelfPictureLoadingCompletedEventHandler BookShelfPictureLoadigCompleted = delegate { };

        private static readonly Network network = Network.Instance;

        public async void SearchPicture(string isbn, string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string localFilePath = @".\pictures\" + isbn + ".jpg";
                string tempFilePath = @".\temp\" + isbn + ".jpg";
                if (File.Exists(localFilePath))
                {
                    LoadingCompleted(Path.GetFullPath(localFilePath));
                }
                else if (File.Exists(tempFilePath))
                {
                    LoadingCompleted(Path.GetFullPath(tempFilePath));
                }
                else
                {
                    if (await SavePictureAsync(tempFilePath, path))
                    {
                        await Task.Delay(300);
                        LoadingCompleted(Path.GetFullPath(tempFilePath));
                    }
                    else
                    {
                        LoadingCompleted("Error");
                    }
                }
            }
            else
            {
                LoadingCompleted(string.Empty);
            }
        }

        private static async Task<bool> SavePictureAsync(string path, string url)
        {
            byte[] picture = await network.GetPicture(url);
            if (picture.Length > 0)
            {
                string localPath = Path.GetDirectoryName(path) ?? string.Empty;
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }
                using Image image = Image.Load(picture);
                image.Mutate(a => a.Resize(new ResizeOptions() { Size = new(270, 390), Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop }));
                await image.SaveAsJpegAsync(path, new JpegEncoder() { Quality = 100 });
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string AddPicture(string isbn, string? path)
        {
            string localFilePath = @".\pictures\" + isbn + ".jpg";
            string tempFilePath = @".\temp\" + isbn + ".jpg";
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
                else if (File.Exists(tempFilePath))
                {
                    File.Move(tempFilePath, localFilePath);
                    return path;
                }
                else
                {
                    _ = SavePictureAsync(localFilePath, path);
                    return path;
                }
            }
        }

        public async void GetPicture(string isbn, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                LoadingCompleted(string.Empty);
            }
            else
            {
                string localFilePath = @".\pictures\" + isbn + ".jpg";
                if (File.Exists(localFilePath))
                {
                    LoadingCompleted(Path.GetFullPath(localFilePath));
                }
                else
                {
                    if (ValidHttpURL(path, out _))
                    {
                        if (await SavePictureAsync(localFilePath, path))
                        {
                            await Task.Delay(300);
                            LoadingCompleted(Path.GetFullPath(localFilePath));
                        }
                        else
                        {
                            LoadingCompleted("Error");
                        }

                    }
                    else
                    {
                        LoadingCompleted("Error");
                    }
                }
            }
        }

        public async void GetBookShelfPicture(ImageQueueInfo imageInfo)
        {
            if (string.IsNullOrEmpty(imageInfo.Url))
            {
                BookShelfPictureLoadigCompleted(imageInfo);
            }
            else
            {
                string localFilePath = @".\pictures\" + imageInfo.Isbn + ".jpg";
                if (File.Exists(localFilePath))
                {
                    imageInfo.Url = Path.GetFullPath(localFilePath);
                    BookShelfPictureLoadigCompleted(imageInfo);
                }
                else
                {
                    if (ValidHttpURL(imageInfo.Url, out _))
                    {
                        if (await SavePictureAsync(localFilePath, imageInfo.Url))
                        {
                            await Task.Delay(300);
                            imageInfo.Url = Path.GetFullPath(localFilePath);
                            BookShelfPictureLoadigCompleted(imageInfo);
                        }
                        else
                        {
                            imageInfo.Url = string.Empty;
                            BookShelfPictureLoadigCompleted(imageInfo);
                        }

                    }
                    else
                    {
                        imageInfo.Url = string.Empty;
                        BookShelfPictureLoadigCompleted(imageInfo);
                    }
                }
            }
        }

        private static bool ValidHttpURL(string s, out Uri? resultURI)
        {
            if (!UriRegex().IsMatch(s))
            {
                s = "http://" + s;
            }
            if (Uri.TryCreate(s, UriKind.Absolute, out resultURI))
            {
                return (resultURI.Scheme == Uri.UriSchemeHttp || resultURI.Scheme == Uri.UriSchemeHttps);
            }
            else
            {
                return false;
            }
        }
    }
}