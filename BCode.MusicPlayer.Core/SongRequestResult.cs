using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Core
{
    public class SongRequestResult
    {
        public SongRequestResult()
        {
            Songs = new List<Song>();
            ErrorMessages = new List<string>();
            WarningMessages = new List<string>();
        }

        public List<Song> Songs { get; }
        public List<string> ErrorMessages { get; }     
        public List<string> WarningMessages { get; }
        public bool IsSuccessful => ErrorMessages.Count == 0;
        public string Result => IsSuccessful ? $"Found {Songs.Count} song(s)" : $"{string.Join(", ", ErrorMessages)}";
    }
}
