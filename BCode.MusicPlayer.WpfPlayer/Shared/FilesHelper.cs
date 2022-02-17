using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public static class FilesHelper
    {
        public static List<string> GetAllFileExtensions(string path)
        {
            List<string> fileExtensionsFound = new List<string>();

            fileExtensionsFound = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Select(f => Path.GetExtension(f)).Distinct().OrderBy(f => f).ToList();

            return fileExtensionsFound;
        }
    }
}
