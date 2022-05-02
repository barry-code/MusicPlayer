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
        protected CancellationTokenSource _mainCancelTokenSource;

        public async Task<SongRetrievalResult> ListAllSongs(string folderPath, CancellationToken cancelToken)
        {
            return await Task.Run(() =>
            {
                var result = new SongRetrievalResult();
                result.Songs = new List<ISong>();

                if (string.IsNullOrEmpty(folderPath))
                {
                    result.ErrorMessage = "Path cannot be empty";
                    return result;
                }

                if (!Directory.Exists(folderPath))
                {
                    result.ErrorMessage = $"Path not found {folderPath}";
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
                        result.ErrorMessage = $"No files found containing extensions {string.Join(",", Constants.AudioFileExtensions)}";
                        return result;
                    }

                    for (int i = 0; i < files?.Count; i++)
                    {
                        var s = GetSongFromFile(files[i]);
                        if (s is not null)
                        {
                            result.Songs.Add(s);
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
                    result.ErrorMessage = "Cancelled getting songs";
                    return result;
                }

                result.IsSuccessful = true;
                return result;

            }, _mainCancelTokenSource.Token);
        }

        public Task UpdateSong(ISong song)
        {
            return Task.CompletedTask;
        }

        private ISong GetSongFromFile(string filePath)
        {
            ISong song;

            if (string.IsNullOrEmpty(filePath))
                PublishEvent($"Cannot get song from empty file", Core.PlayerEvent.Type.Error, Core.PlayerEvent.Category.PlayerState, null);

            if (!File.Exists(filePath))
                PublishEvent($"File not found [{filePath}]", Core.PlayerEvent.Type.Error, Core.PlayerEvent.Category.PlayerState, new FileNotFoundException(filePath));

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
                PublishEvent($"Error getting song information [{filePath}]", Core.PlayerEvent.Type.Error, Core.PlayerEvent.Category.PlayerState, ex);
                return null;
            }
        }
    }
}
