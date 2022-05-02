using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public interface IArtist
    {
        Guid ArtistId { get; set; }
        string Name { get; set; }

        IList<IAlbum> Albums { get; set; }
    }
}
