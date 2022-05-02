using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Core
{
    public interface ILibraryManager
    {
        Task<SongRetrievalResult> ListAllSongs(string folderPath, CancellationToken cancelToken);
        Task UpdateSong(ISong song);
    }
}
