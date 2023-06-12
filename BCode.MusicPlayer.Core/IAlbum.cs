using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public interface IAlbum
    {
        Guid AlbumId { get; set; }
        string Name { get; set; }
        int Year { get; set; }
        int ArtistId { get; set; }
        IList<Song> Songs { get; set; }
    }
}
