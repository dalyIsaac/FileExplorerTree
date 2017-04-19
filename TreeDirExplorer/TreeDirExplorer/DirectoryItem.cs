using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeDirExplorer
{
    public class DirectoryItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Indentation { get; set; }
        public string LastModifiedDate { get; set; }
        public string LastModifiedTime { get; set; }
        public string Size { get; set; }
        public string ImageLocation { get; set; }

        public DirectoryItem(string path, int TabIndex)
        {
            Indentation = new string(' ', TabIndex * 8);
            Name = path.Split('\\').Last();
            Path = path;
            DateTime lastmod = System.IO.File.GetLastWriteTime(path);
            LastModifiedDate = lastmod.ToString("yyyy-MM-dd");
            LastModifiedTime = lastmod.ToString("HH:mm:ss");
            long size;
            try
            {
                size = new System.IO.FileInfo(path).Length;
            }
            catch (Exception)
            {
                size = GetDirectorySize(path);
                ImageLocation = "open.ico";
            }
            Size = SizeSuffix(size);
        }

        private static long GetDirectorySize(string folderPath)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", System.IO.SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private static string SizeSuffix(long value, int decimalPlaces = 2)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "  0.00 bytes"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
