using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public class Album
    {
        public Guid AlbumId { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int ArtistId { get; set; }
        public IList<Song> Songs { get; set; }
    }
}
