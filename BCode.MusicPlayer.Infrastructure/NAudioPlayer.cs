using BCode.MusicPlayer.Core;
using DynamicData;
using NAudio.Wave;

namespace BCode.MusicPlayer.Infrastructure
{
    public abstract class NAudioPlayer : IPlayer
    {
        protected WaveOutEvent _outputDevice;
        protected WaveStream _audioFile;
        protected const float MAX_VOLUME = 1.0f;
        protected const int SKIP_INTERVAL = 10;
        protected bool isManualStop = false;
        protected CancellationTokenSource _mainCancelTokenSource;
        protected readonly object songTimeLock = new Object();
        protected int _playlistSongCount;
        protected bool disposedValue;

        protected NAudioPlayer()
        {
            Initialize();

            _mainCancelTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!_mainCancelTokenSource.Token.IsCancellationRequested)
                {
                    UpdateSongTime();
                    Thread.Sleep(500);
                }
            });
        }

        public virtual event EventHandler<PlayerEvent> PlayerEvent;

        protected IList<ISong> _playlist;
        public virtual IList<ISong> PlayList
        {
            get { return _playlist; }
            set { _playlist = value; }
        }

        protected ISong _currentSong;
        public virtual ISong CurrentSong
        {
            get { return _currentSong; }
            set { _currentSong = value; }
        }

        protected ISong _nextUp;
        public virtual ISong NextUp
        {
            get { return _nextUp; }
            set { _nextUp = value; }
        }

        protected Status _status;
        public virtual Status Status 
        {
            get { return _status; }            
            set 
            {
                if (_status != value)
                {
                    _status = value;
                    IsPlaying = _status == Status.Playing;
                }            
            }
        }

        protected float _currentVolume;
        public virtual float CurrentVolume
        {
            get { return _currentVolume; }
            set 
            {
                if (_currentVolume != value)
                {
                    _currentVolume = value;
                    AdjustVolume();
                }
            }
        }

        protected TimeSpan _currentElapsedTime;
        public virtual TimeSpan CurrentElapsedTime
        {
            get { return _currentElapsedTime; }
            set 
            {
                if (_currentElapsedTime != value)
                {
                    _currentElapsedTime = value;
                }                
            }
        }

        protected bool _isPlaying;
        public virtual bool IsPlaying
        {
            get { return _isPlaying; }
            set 
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    if (_isPlaying)
                    {
                        PublishEvent($"Now playing [{CurrentSong.Name}]");
                    }
                }                
            }
        }

        protected bool _isMuted;
        public virtual bool IsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; }
        }


        public virtual void Initialize()
        {            
            CurrentVolume = 10f;
        }

        public virtual void Play()
        {
            ISong songToPlay = CurrentSong ?? PlayList?.FirstOrDefault();

            if (_outputDevice?.PlaybackState == PlaybackState.Paused)
                Resume();
            else
                Play(songToPlay);
        }

        public virtual void Play(int playListIndex)
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

        public virtual void Pause()
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

        public virtual void Stop()
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

        public virtual void Next()
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

        public virtual void Previous()
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

        public virtual void RemoveSongFromPlayList(ISong song)
        {
            PlayList.Remove(song);
            PublishEvent($"Removed song [{song.Name}] from playlist");
        }

        public virtual void ClearPlayList()
        {
            _playlistSongCount = 0;
            isManualStop = true;
            NextUp = null;
            Cleanup();
            PlayList.Clear();
            PublishEvent("Cleared Playlist");
        }

        public virtual void VolumeUp()
        {
            if (CurrentVolume >= MAX_VOLUME)
                return;

            CurrentVolume = Math.Min(1.0f, CurrentVolume + 0.05f);

            if (_outputDevice is null)
                return;

            _outputDevice.Volume = CurrentVolume;
        }

        public virtual void VolumeDown()
        {
            if (CurrentVolume <= 0.05f)
                return;

            CurrentVolume = CurrentVolume - 0.05f < 0.0f ? 0.0f : CurrentVolume - 0.05f;

            if (_outputDevice is null)
                return;

            _outputDevice.Volume = CurrentVolume;
        }

        public virtual void Dispose()
        {
            _mainCancelTokenSource?.Cancel();
            Cleanup();
        }        


        protected void Play(ISong song)
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

        protected void Resume()
        {
            _outputDevice.Play();
            Status = Status.Playing;
            PublishEvent($"Now playing [{CurrentSong.Name}]");
        }

        protected void OnPlaybackStopped(object sender, StoppedEventArgs args)
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

        protected void NewSong()
        {
            Cleanup();

            Play(NextUp);
        }

        protected void PublishEvent(string message, Core.PlayerEvent.Type type = Core.PlayerEvent.Type.Information, Exception ex = null)
        {
            var errorDetails = (type == Core.PlayerEvent.Type.Error ? (ex is null ? string.Empty : ex.Message) : string.Empty);
            var msg = string.IsNullOrEmpty(errorDetails) ? message : message + " - " + errorDetails;

            PlayerEvent?.Invoke(this, new PlayerEvent(msg, type));
        }

        protected void Cleanup()
        {
            CurrentSong = null;
            _outputDevice?.Dispose();
            _audioFile?.Dispose();
            _outputDevice = null;
            _audioFile = null;
        }

        protected void UpdateSongTime()
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

        protected ISong GetSongFromFile(string path)
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

        protected async Task<ICollection<ISong>> GetSongsFromFiles(ICollection<string> files, CancellationToken addSongsCancelToken)
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

        protected async Task<ICollection<ISong>> GetSongsFromFolder(string folderPath, CancellationToken addSongsCancelToken)
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
                            .Where(f => Constants.AudioFileExtensions.Contains(Path.GetExtension(f), StringComparer.CurrentCultureIgnoreCase))
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

        protected void AdjustVolume()
        {
            IsMuted = CurrentVolume <= 0;

            float actualVolume = CurrentVolume / 100f;

            if (_outputDevice is not null)
            {
                _outputDevice.Volume = actualVolume;
            }
        }
    }
}
