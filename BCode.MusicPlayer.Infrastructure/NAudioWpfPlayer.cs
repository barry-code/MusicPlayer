using NAudio.Wave;
using ReactiveUI;
using System;
using System.Linq;
using DynamicData;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive;
using BCode.MusicPlayer.Core;
using System.Diagnostics;

namespace BCode.MusicPlayer.Infrastructure
{
    public class NAudioWpfPlayer : ReactiveObject, IPlayer, IDisposable
    {
        private WaveOutEvent _outputDevice;
        private WaveStream _audioFile;
        private const float MAX_VOLUME = 1.0f;
        private const int SKIP_INTERVAL = 10;        
        private bool isManualStop = false;
        private CancellationTokenSource _mainCancelTokenSource;
        private readonly object songTimeLock = new Object();
        private int _playlistSongCount;

        public NAudioWpfPlayer()
        {
            _mainCancelTokenSource = new CancellationTokenSource();

            CurrentVolume = 10f;

            Task.Run(() =>
            {
                while (!_mainCancelTokenSource.Token.IsCancellationRequested)
                {                    
                    UpdateSongTime();
                    Thread.Sleep(500);
                }
            });

            this.WhenAnyValue(x => x.Status)
                .DistinctUntilChanged()
                .Subscribe<Status>(x =>
                {
                    IsPlaying = Status == Status.Playing;
                });

            this.WhenAnyValue(x => x.CurrentSong, x => x.Status)
                .Where(x => x.Item1 != null && x.Item2 == Status.Playing)
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    UpdateSongTime();
                    PublishEvent($"Now playing [{CurrentSong.Name}]");
                });

            this.WhenAnyValue(x => x.CurrentVolume)
                .DistinctUntilChanged()
                .Subscribe(x => AdjustVolume());
        }

        public event EventHandler<PlayerEvent> PlayerEvent;

        public IList<ISong> PlayList { get; set; } = new ObservableCollection<ISong>();

        private ISong _currentSong;
        public ISong CurrentSong
        {
            get => _currentSong;
            set => this.RaiseAndSetIfChanged(ref _currentSong, value);
        }

        private ISong _nextUp;
        public ISong NextUp
        {
            get => _nextUp;
            set => this.RaiseAndSetIfChanged(ref _nextUp, value);
        }

        private Status _status;
        public Status Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private float _currentVolume;
        public float CurrentVolume
        {
            get => _currentVolume;
            set => this.RaiseAndSetIfChanged(ref _currentVolume, value);
        }

        private TimeSpan _currentElapsedTime;
        public TimeSpan CurrentElapsedTime
        {
            get => _currentElapsedTime;
            set => this.RaiseAndSetIfChanged(ref _currentElapsedTime, value);
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
        }

        private bool _isMuted;       
        public bool IsMuted
        {
            get => _isMuted;
            set => this.RaiseAndSetIfChanged(ref _isMuted, value);
        }

        public void OnLoad()
        {

        }

        public void Play()
        {
            ISong songToPlay = CurrentSong ?? PlayList?.FirstOrDefault();

            if (_outputDevice?.PlaybackState == PlaybackState.Paused)
                Resume();
            else
                Play(songToPlay);
        }

        public void Play(int playListIndex)
        {
            ISong songToPlay = PlayList?[playListIndex];

            if (songToPlay is null)
                return;

            if (CurrentSong is null)
            {
                CurrentSong = songToPlay;
                Play();
                return;
            }

            NextUp = songToPlay;
            Stop();
        }

        private void Play(ISong song)
        {
            try
            {
                if (song is null)
                    return;

                if (!File.Exists(song.Path))
                    PublishEvent($"Song file not found [{song.Path}]", Core.PlayerEvent.Type.Error, new FileNotFoundException(song.Path));

                if (_outputDevice == null)
                {
                    _outputDevice = new WaveOutEvent();
                    _outputDevice.Volume = CurrentVolume / 100f;
                    _outputDevice.PlaybackStopped += OnPlaybackStopped;
                }

                if (_audioFile == null)
                {
                    try
                    {
                        _audioFile = new AudioFileReader(song.Path);
                    }
                    catch (Exception)
                    {
                        _audioFile?.Dispose();

                        _audioFile = new MediaFoundationReader(song.Path);                        
                    }
                    
                }

                _outputDevice.Init(_audioFile);
                _outputDevice.Play();
                CurrentSong = song;
                Status = Status.Playing;
            }
            catch (Exception ex)
            {
                Status = Status.Stopped;
                PublishEvent($"Unable to play song [{song.Name}]", Core.PlayerEvent.Type.Error, ex);
            }
        }

        public void Pause()
        {
            try
            {
                isManualStop = true;
                _outputDevice?.Pause();
                Status = Status.Paused;
                PublishEvent("Paused");
            }
            catch (Exception ex)
            {
                Status = Status.Stopped;
                PublishEvent($"Unable to pause", Core.PlayerEvent.Type.Error, ex);
            }

        }        

        private void Resume()
        {
            _outputDevice.Play();
            Status = Status.Playing;
            PublishEvent($"Resumed playing [{CurrentSong.Name}]");
        }

        public void Next()
        {
            NextUp = null;

            if (PlayList.Count <= 0)
                return;

            var currentIndex = PlayList.IndexOf(CurrentSong);

            var nextIndex = currentIndex + 1;

            if (nextIndex > PlayList.Count - 1)
                return;

            NextUp = PlayList[nextIndex];

            if (NextUp is null)
                return;

            if (_outputDevice is null)
            {
                Play();
                return;
            }

            if (_outputDevice.PlaybackState == PlaybackState.Playing || _outputDevice.PlaybackState == PlaybackState.Paused)
            {
                Stop();
            }
        }

        public void Previous()
        {
            NextUp = null;

            if (PlayList.Count <= 0)
                return;

            var currentIndex = PlayList.IndexOf(CurrentSong);

            var prevIndex = currentIndex - 1;

            if (prevIndex < 0)
                return;

            NextUp = PlayList[prevIndex];

            if (NextUp is null)
                return;

            if (_outputDevice is null)
            {
                Play();
                return;
            }

            if (_outputDevice.PlaybackState == PlaybackState.Playing || _outputDevice.PlaybackState == PlaybackState.Paused)
            {
                Stop();
            }
        }

        public void Stop()
        {
            try
            {
                isManualStop = true;
                _outputDevice?.Stop();
                Cleanup();
                PublishEvent("Stopped");
            }
            catch (Exception ex)
            {
                Status = Status.Stopped;
                PublishEvent($"Unable to stop", Core.PlayerEvent.Type.Error, ex);
            }            
        }

        public void AddSongToPlayList(ISong song)
        {
            _playlistSongCount++;
            song.Order = _playlistSongCount;
            PlayList.Add(song);
            //PublishEvent($"Added song [{song.Name}] to playlist");
        }

        public void AddSongToPlayList(string filePath)
        {
            var song = GetSongFromFile(filePath);

            if (song is not null)
            {
                AddSongToPlayList(song);
            }           
        }

        public void AddSongsToPlayList(ICollection<ISong> songs)
        {
            foreach (var song in songs)
            {
                _playlistSongCount++;
                song.Order = _playlistSongCount;
            }
            PlayList.AddRange(songs);
            PublishEvent($"Added {songs.Count} songs to playlist");
        }

        public async Task AddSongsToPlayList(string folderPath, CancellationToken addSongsCancelToken)
        {
            var songs = await GetSongsFromFolder(folderPath, addSongsCancelToken);

            AddSongsToPlayList(songs);
        }

        public async Task AddSongsToPlayList(ICollection<string> files, CancellationToken addSongsCancelToken)
        {
            var songs = await GetSongsFromFiles(files, addSongsCancelToken);

            AddSongsToPlayList(songs);
        }

        public void RemoveSongFromPlayList(ISong song)
        {
            PlayList.Remove(song);
            PublishEvent($"Removed song [{song.Name}] from playlist");
        }

        public void ClearPlayList()
        {
            _playlistSongCount = 0;
            isManualStop = true;
            NextUp = null;
            Cleanup();
            PlayList.Clear();
            PublishEvent("Cleared Playlist");
        }

        public void VolumeUp()
        {
            if (CurrentVolume >= MAX_VOLUME)
                return;

            CurrentVolume = Math.Min(1.0f, CurrentVolume + 0.05f);

            if (_outputDevice is null)
                return;

            _outputDevice.Volume = CurrentVolume;
        }

        public void VolumeDown()
        {           
            if (CurrentVolume <= 0.05f)
                return;

            CurrentVolume = CurrentVolume - 0.05f < 0.0f ? 0.0f : CurrentVolume - 0.05f;

            if (_outputDevice is null)
                return;

            _outputDevice.Volume = CurrentVolume;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            Status = Status.Stopped;

            if (isManualStop)
            {
                PublishEvent("Stopped");
            }

            if (!isManualStop)
            {                
                Next();
            }

            isManualStop = false;

            if (NextUp != null)
            {
                NewSong();
            }
        }

        public void Dispose()
        {
            _mainCancelTokenSource?.Cancel();
            Cleanup();
        }

        private void NewSong()
        {
            Cleanup();

            Play(NextUp);
        }

        private void Cleanup()
        {
            CurrentSong = null;            
            _outputDevice?.Dispose();
            _audioFile?.Dispose();
            _outputDevice = null;
            _audioFile = null;
        }

        public void SkipAhead()
        {
            if (_outputDevice is null)
                return;

            if (_audioFile is null)
                return;

            _outputDevice.Pause();
            _audioFile.Skip(SKIP_INTERVAL);
            _outputDevice.Play();

            UpdateSongTime();
        }

        public void SkipBack()
        {
            if (_outputDevice is null)
                return;

            if (_audioFile is null)
                return;

            _audioFile.Skip(-SKIP_INTERVAL);

            UpdateSongTime();
        }

        public void SkipTo(int seconds)
        {
            if (_outputDevice is null)
                return;

            if (_audioFile is null)
                return;                       
            
            _outputDevice.Pause();
            _audioFile.CurrentTime = TimeSpan.FromSeconds(seconds);
            _outputDevice.Play();

            UpdateSongTime();
        }

        private ISong GetSongFromFile(string path)
        {
            ISong song;

            if (string.IsNullOrEmpty(path))
                PublishEvent($"Cannot get song from empty file", Core.PlayerEvent.Type.Error, null);

            if (!File.Exists(path))
                PublishEvent($"File not found [path]", Core.PlayerEvent.Type.Error, new FileNotFoundException(path));

            try
            {
                using (TagLib.File file = TagLib.File.Create(new FileAbstraction(path)))
                {
                    song = new Song();

                    song.Name = !string.IsNullOrEmpty(file.Tag.Title) ? file.Tag.Title : $"Unknown Song {song.SongId}";
                    song.Path = path;
                    song.Extension = Path.GetExtension(path);
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
                PublishEvent($"Error getting song information [{path}]", Core.PlayerEvent.Type.Error, ex);
                return null;                
            }
        }

        private async Task<ICollection<ISong>> GetSongsFromFolder(string folderPath, CancellationToken addSongsCancelToken)
        {
            var songsFound = new List<ISong>();

            if (string.IsNullOrEmpty(folderPath))
            {
                return songsFound;
            }

            songsFound = await Task.Run(() =>
            {
                var songList = new List<ISong>();

                try
                {
                    if (addSongsCancelToken.IsCancellationRequested)
                    {
                        addSongsCancelToken.ThrowIfCancellationRequested();
                    }

                    var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                            .Where(f => Constants.AudioFileExtensions.Contains(Path.GetExtension(f),StringComparer.CurrentCultureIgnoreCase))
                            .ToList();

                    if (files?.Count == 0)
                    {
                        return songList;
                    }
                    
                    foreach (var songFile in files)
                    {
                        var s = GetSongFromFile(songFile);
                        if (s is not null)
                        {
                            songList.Add(s);
                        }

                        if (addSongsCancelToken.IsCancellationRequested)
                        {
                            addSongsCancelToken.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    songList.Clear();
                    PublishEvent("Cancelled getting songs");
                }

                return songList;

            }, _mainCancelTokenSource.Token);

            return songsFound;
        }

        private async Task<ICollection<ISong>> GetSongsFromFiles(ICollection<string> files, CancellationToken addSongsCancelToken)
        {
            var songsFound = new List<ISong>();

            if (files is null)
                return songsFound;

            if (files.Count == 0)
                return songsFound;

            songsFound = await Task.Run(() =>
            {
                var songList = new List<ISong>();

                try
                {
                    if (addSongsCancelToken.IsCancellationRequested)
                        addSongsCancelToken.ThrowIfCancellationRequested();

                    foreach (var file in files)
                    {
                        var s = GetSongFromFile(file);
                        if (s is not null)
                        {
                            songList.Add(s);
                        }

                        if (addSongsCancelToken.IsCancellationRequested)
                            addSongsCancelToken.ThrowIfCancellationRequested();
                    }
                }
                catch (OperationCanceledException)
                {
                    songList.Clear();
                    PublishEvent("Cancelled getting songs");
                }

                return songList;
            });            

            return songsFound;
        }

        private void AdjustVolume()
        {
            IsMuted = CurrentVolume <= 0;

            float actualVolume = CurrentVolume / 100f;

            if (_outputDevice is not null)
            {
                _outputDevice.Volume = actualVolume;
            }
        }

        private void UpdateSongTime()
        {
            lock (songTimeLock)
            {
                if (CurrentSong is null || _audioFile is null)
                {
                    CurrentElapsedTime = TimeSpan.Zero;
                    return;
                }

                CurrentElapsedTime = _audioFile.CurrentTime;
            }
        }

        private void PublishEvent(string message, Core.PlayerEvent.Type type = Core.PlayerEvent.Type.Information, Exception ex = null)
        {
            var errorDetails = (type == Core.PlayerEvent.Type.Error ? (ex is null ? string.Empty : ex.Message) : string.Empty);
            var msg = string.IsNullOrEmpty(errorDetails) ? message : message + " - " + errorDetails;

            PlayerEvent?.Invoke(this, new PlayerEvent(msg, type));
        }
    }

    public class FileAbstraction : TagLib.File.IFileAbstraction
    {
        public FileAbstraction(string file)
        {
            Name = file;
        }

        public string Name { get; }

        public Stream ReadStream => new FileStream(Name, FileMode.Open);

        public Stream WriteStream => new FileStream(Name, FileMode.Open);

        public void CloseStream(Stream stream)
        {
            stream.Close();
        }
    }
}
