using System.IO;

namespace SmartLibrary.Helpers
{
    public sealed class FileOccupancy
    {
        public static string GetDirectorySize(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo folder = new(path);
                return FormatBytes(FolderSize(folder));
            }
            else
            {
                return "0 bytes";
            }
        }

        public static string GetFileSize(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fileInfo = new(path);
                return FormatBytes(fileInfo.Length);
            }
            else
            {
                return "0 bytes";
            }
        }

        private static long FolderSize(DirectoryInfo folder)
        {
            long totalSizeOfDir = 0;

            // 获取目录中的所有文件
            FileInfo[] allFiles = folder.GetFiles();

            // 循环遍历每个文件并获取其大小
            foreach (FileInfo file in allFiles)
            {
                totalSizeOfDir += file.Length;

                // 在这里计算长度。
            }

            DirectoryInfo[] subFolders = folder.GetDirectories();

            // 在这里，我们查看文件中是否存在子文件夹或目录。
            foreach (DirectoryInfo dir in subFolders)
            {
                totalSizeOfDir += FolderSize(dir);

                // 在这里，我们递归调用来检查所有子文件夹。
            }
            return totalSizeOfDir;

            // 我们返回总大小在这里。
        }

        private static string FormatBytes(long bytes)
        {
            /*此方法基本上用于确定文件的大小。它首先确定我们是否必须在字节，KB，MB或GB中完成。如果大小在1KB和1MB之间，那么我们将计算KB的大小。同样，如果在MB和GB之间，则将其计算为MB。*/
            string[] sizes = ["bytes", "KB", "MB", "GB", "TB"];

            // 在这里，我们将所有大小存储在字符串中。
            int order = 0;

            // 我们使用零初始化顺序，以便它不返回一些垃圾值。
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }
    }
}
