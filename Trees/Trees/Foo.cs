using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trees
{
    public class Items
    {
        public Items(string path, bool isFolder)
        {
            // Path
            Path = path;

            // Name
            string[] fileNameParts = path.Split('\\');
            Name = fileNameParts.Last();

            // IsFolder
            IsFolder = isFolder;

            // LastModified
            LastModified = System.IO.File.GetLastWriteTime(path);

            if (isFolder)
            {
                Children = new List<Items>();
                foreach (string childFolder in Directory.GetDirectories(Path))
                {
                    Children.Add(new Items(childFolder, true));
                }
                foreach (string childFile in Directory.GetFiles(Path))
                {
                    Children.Add(new Items(childFile, false));
                }
            }
        }

        public string Path { get; set; }
        public string Name { get; set; }
        public ulong FileSize { get; set; }
        public bool IsFolder { get; set; }
        public DateTime LastModified { get; set; }
        public IList<Items> Children { get; set; }
    }
}
