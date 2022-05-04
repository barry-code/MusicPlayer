using BCode.MusicPlayer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Infrastructure
{
    public class LibraryManager : ILibraryManager
    {
        private bool disposedValue;
        private readonly CancellationTokenSource _mainCancelTokenSource;
        private Task<SongRequestResult> _getSongsTask;

        public LibraryManager()
        {
            _mainCancelTokenSource = new CancellationTokenSource();
        }

        public async Task<SongRequestResult> GetAllSongs(string folderPath, CancellationToken cancelToken)
        {
            if (_getSongsTask?.Status == TaskStatus.Running)
            {
                _getSongsTask?.Wait();
            }

            _getSongsTask = Task.Run(() =>
            {
                var result = new SongRequestResult();

                if (string.IsNullOrEmpty(folderPath))
                {
                    result.ErrorMessages.Add("Path cannot be empty");
                    return result;
                }

                if (!Directory.Exists(folderPath))
                {
                    result.ErrorMessages.Add($"Path not found {folderPath}");
                    return result;
                }

                try
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                    }

                    var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                            .Where(f => Constants.AudioFileExtensions.Contains(Path.GetExtension(f), StringComparer.CurrentCultureIgnoreCase))
                            .ToList();

                    if (files is null || files?.Count == 0)
                    {
                        result.ErrorMessages.Add($"No files found containing extensions {string.Join(",", Constants.AudioFileExtensions)}");
                        return result;
                    }

                    for (int i = 0; i < files?.Count; i++)
                    {
                        try
                        {
                            var s = GetSongFromFile(files[i]);
                            if (s is not null)
                            {
                                result.Songs.Add(s);
                            }
                        }
                        catch (Exception e)
                        {
                            result.ErrorMessages.Add(e.Message);
                        }

                        if (cancelToken.IsCancellationRequested)
                        {
                            cancelToken.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    result.Songs.Clear();
                    result.ErrorMessages.Add("Cancelled getting songs");
                    return result;
                }

                return result;

            }, _mainCancelTokenSource.Token);
            
            return await _getSongsTask;
        }

        public async Task<SongRequestResult> GetAllSongs(ICollection<string> files, CancellationToken cancelToken)
        {
            if (_getSongsTask?.Status == TaskStatus.Running)
            {
                _getSongsTask?.Wait();
            }

            _getSongsTask = Task.Run(() =>
            {
                var result = new SongRequestResult();

                if (files is null)
                    return result;

                if (files.Count == 0)
                    return result;

                try
                {
                    if (cancelToken.IsCancellationRequested)
                        cancelToken.ThrowIfCancellationRequested();

                    foreach (var file in files)
                    {
                        try
                        {
                            var s = GetSongFromFile(file);
                            if (s is not null)
                            {
                                result.Songs.Add(s);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorMessages.Add(ex.Message);
                        }                        

                        if (cancelToken.IsCancellationRequested)
                            cancelToken.ThrowIfCancellationRequested();
                    }
                }
                catch (OperationCanceledException)
                {
                    result.Songs.Clear();
                    result.ErrorMessages.Add("Cancelled getting songs");
                }

                return result;
            }, _mainCancelTokenSource.Token);

            return await _getSongsTask;
        }

        public Task UpdateSong(ISong song)
        {
            return Task.CompletedTask;
        }

        public ISong GetSongFromFile(string filePath)
        {
            ISong song;

            if (string.IsNullOrEmpty(filePath))
                throw new Exception($"Cannot get song from empty file");

            if (!File.Exists(filePath))
                throw new Exception($"File not found [{filePath}]");

            try
            {
                using (TagLib.File file = TagLib.File.Create(new FileAbstraction(filePath)))
                {
                    song = new Song();

                    song.Name = !string.IsNullOrEmpty(file.Tag.Title) ? file.Tag.Title : $"{Path.GetFileNameWithoutExtension(file.Name)}";
                    song.Path = filePath;
                    song.Extension = Path.GetExtension(filePath);
                    song.Size = file.Length;
                    song.ArtistName = string.IsNullOrEmpty(file.Tag.FirstPerformer) ? file.Tag.FirstAlbumArtistSort : file.Tag.FirstPerformer;
                    song.AlbumName = string.IsNullOrEmpty(file.Tag.Album) ? file.Tag.AlbumSort : file.Tag.Album;
                    song.Year = file.Tag.Year == 0 ? String.Empty : file.Tag.Year.ToString();
                    song.Duration = file.Properties.Duration;
                }

                return song;
            }
            catch (Exception ex)
            {
                throw new Exception($"{filePath} Error getting song information: {ex.Message}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _mainCancelTokenSource?.Cancel();
                    _getSongsTask?.Wait();
                    _mainCancelTokenSource?.Dispose();
                    _getSongsTask?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~LibraryManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
