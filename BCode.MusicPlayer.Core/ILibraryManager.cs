using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Core
{
    public interface ILibraryManager : IDisposable
    {
        Task<SongRequestResult> GetAllSongs(string folderPath, CancellationToken cancelToken);
        Task<SongRequestResult> GetAllSongs(ICollection<string> files, CancellationToken cancelToken);
        Song GetSongFromFile(string filePath);
        Task UpdateSong(Song song);
    }
}
