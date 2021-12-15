using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public interface IArtist
    {
        int ArtistId { get; set; }
        string Name { get; set; }

        IList<IAlbum> Albums { get; set; }
    }
}
