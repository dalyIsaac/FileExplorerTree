using System;
using System.Linq;

namespace TreeDirExplorer
{
    /// <summary>
    /// Contains information about items inside a directory
    /// </summary>
    public class DirectoryItem
    {
        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Item path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The level of indentation for the tree
        /// </summary>
        public string Indentation { get; set; }

        /// <summary>
        /// Last modified date (yyyy-MM-dd)
        /// </summary>
        public string LastModifiedDate { get; set; }

        /// <summary>
        /// Last modified time (HH:mm:ss)
        /// </summary>
        public string LastModifiedTime { get; set; }

        /// <summary>
        /// Virtual size of item (byte/KB/MB/GB/TB/PB/EB/ZB/YB)
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Contains a string with the image's location inside the root application folder
        /// </summary>
        public string ImageLocation { get; set; }

        /// <summary>
        /// Color to display text
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Underlines folders
        /// </summary>
        public string TextDecoration { get; set; }

        /// <summary>
        /// Creates a new DirectoryItem with all properties
        /// </summary>
        /// <param name="path"></param>
        /// <param name="TabIndex"></param>
        public DirectoryItem(string path, int TabIndex)
        {
            Indentation = new string(' ', TabIndex * 8);
            Name = path.Split('\\').Last();
            Path = path;
            DateTime lastmod = System.IO.File.GetLastWriteTime(path); // Gets the date and time of the last time the file/folder was modified
            LastModifiedDate = lastmod.ToString("yyyy-MM-dd");
            LastModifiedTime = lastmod.ToString("HH:mm:ss");
            long size;
            try // Item is a file
            {
                size = new System.IO.FileInfo(path).Length;
                Color = "#000000";
            }
            catch (Exception) // Item is a folder
            {
                size = GetDirectorySize(path);
                ImageLocation = "Icons/chevron.ico";
                Color = "#0461f7";
                TextDecoration = "Underline";
            }
            Size = SizeSuffix(size);
        }

        /// <summary>
        /// Gets the size of the folder
        /// </summary>
        /// <param name="folderPath">Path of the folder</param>
        /// <returns></returns>
        private static long GetDirectorySize(string folderPath)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", System.IO.SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        /// <summary>
        /// Array of all the possible suffixes for a size string
        /// </summary>
        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// Converts the size of the item to a user friendly string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
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
