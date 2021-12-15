using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public interface ISong
    {
        int SongId { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        string ArtistName { get; set; }
        string AlbumName { get; set; }
        uint Order { get; set; }
        string Year { get; set; }
        TimeSpan Duration { get; set; }
        IList<Genre> Genres { get; set; }

        bool Equals(object obj);

    }
}
