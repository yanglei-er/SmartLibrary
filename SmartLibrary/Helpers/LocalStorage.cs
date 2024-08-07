﻿using Shared.Helpers;
using SmartLibrary.Models;
using System.Drawing;
using System.Drawing.Imaging;
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
                        LoadingCompleted($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/LoadingPictureError.png");
                    }
                }
            }
            else
            {
                LoadingCompleted(ResourceManager.EmptyImage);
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
                using MemoryStream stream = new(picture);
                using Image image = Image.FromStream(stream);
                Image a = ImageProcess.Resize(image, 270, 390);
                a.Save(path, ImageFormat.Jpeg);
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
                    using Image image = Image.FromFile(path);
                    Image a = ImageProcess.Resize(image, 270, 390);
                    a.Save(path, ImageFormat.Jpeg);
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
                LoadingCompleted(ResourceManager.EmptyImage);
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
                    if (ValidImageURL(path, out _))
                    {
                        if (await SavePictureAsync(localFilePath, path))
                        {
                            await Task.Delay(300);
                            LoadingCompleted(Path.GetFullPath(localFilePath));
                        }
                        else
                        {
                            LoadingCompleted($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/LoadingPictureError.png");
                        }

                    }
                    else
                    {
                        LoadingCompleted($"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/LoadingPictureError.png");
                    }
                }
            }
        }

        public async void GetBookShelfPicture(ImageQueueInfo imageInfo)
        {
            if (string.IsNullOrEmpty(imageInfo.Url))
            {
                imageInfo.Url = ResourceManager.EmptyImage;
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
                    if (ValidImageURL(imageInfo.Url, out _))
                    {
                        if (await SavePictureAsync(localFilePath, imageInfo.Url))
                        {
                            await Task.Delay(300);
                            imageInfo.Url = Path.GetFullPath(localFilePath);
                            BookShelfPictureLoadigCompleted(imageInfo);
                        }
                        else
                        {
                            imageInfo.Url = $"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/LoadingPictureError.png";
                            BookShelfPictureLoadigCompleted(imageInfo);
                        }
                    }
                    else
                    {
                        imageInfo.Url = $"pack://application:,,,/Assets/DynamicPic/{ResourceManager.CurrentTheme}/LoadingPictureError.png";
                        BookShelfPictureLoadigCompleted(imageInfo);
                    }
                }
            }
        }

        private static bool ValidImageURL(string s, out Uri? resultURI)
        {
            if (!UriRegex().IsMatch(s))
            {
                s = "http://" + s;
            }
            if (Uri.TryCreate(s, UriKind.Absolute, out resultURI))
            {
                if (resultURI.Scheme == Uri.UriSchemeHttp || resultURI.Scheme == Uri.UriSchemeHttps)
                {
                    if (s.EndsWith("jpg") || s.EndsWith("png"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}