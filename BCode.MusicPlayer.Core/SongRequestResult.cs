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
            Songs = new List<ISong>();
            ErrorMessages = new List<string>();
        }

        public List<ISong> Songs { get; }
        public List<string> ErrorMessages { get; }        
        public bool IsSuccessful => ErrorMessages.Count == 0;
        public string Result => IsSuccessful ? $"Found {Songs.Count} song(s)" : $"{string.Join(", ", ErrorMessages)}";
    }
}
