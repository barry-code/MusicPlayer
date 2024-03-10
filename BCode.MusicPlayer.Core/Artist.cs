using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public class Artist
    {
        public Guid ArtistId { get; set; }
        public string Name { get; set; }

        public IList<Album> Albums { get; set; }
    }
}
