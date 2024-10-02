using System;
using System.IO;

namespace Cokee.ClassService.Helper
{
    public static class FileSystemHelper
    {
        public static class DirHelper
        {
            public static string CalcDirBytes(string path)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);
                long count = 0;
                foreach (var item in dirinfo.GetFiles())
                {
                    count += item.Length;
                }
                return FileSize.Format(count);
            }
            public static bool MakeExist(string? path)
            {
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                        throw;
                    }
                }
                else return true;
            }
        }
        public static class FileSize
        {
            public static string Format(long bytes, string formatString = "{0:0.00}")
            {
                int counter = 0;
                double number = bytes;

                // 最大单位就是 PB 了，而 PB 是第 5 级，从 0 开始数
                // "Bytes", "KB", "MB", "GB", "TB", "PB"
                const int maxCount = 5;

                while (Math.Round(number / 1024) >= 1)
                {
                    number = number / 1024;
                    counter++;

                    if (counter >= maxCount)
                    {
                        break;
                    }
                }

                var suffix = counter switch
                {
                    0 => "B",
                    1 => "KB",
                    2 => "MB",
                    3 => "GB",
                    4 => "TB",
                    5 => "PB",
                    // 通过 maxCount 限制了最大的值就是 5 了
                    _ => throw new ArgumentException("")
                };

                return $"{string.Format(formatString, number)}{suffix}";
            }
        }
    }
}
