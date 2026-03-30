using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        public static BitmapImage GetImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        public static T DeepCopyOfObject<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
