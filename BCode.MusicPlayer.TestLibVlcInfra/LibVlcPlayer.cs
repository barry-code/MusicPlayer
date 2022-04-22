using BCode.MusicPlayer.Core;
using LibVLCSharp.Shared;

namespace BCode.MusicPlayer.TestLibVlcInfra
{
    public class LibVlcPlayer : IPlayer
    {
        protected LibVLC _libVlc;
        protected MediaPlayer _mediaPlayer;
        protected CancellationTokenSource _mainCancelTokenSource;
        protected const int SKIP_INTERVAL_SECONDS = 10;
        protected const int MIN_VOLUME_PERCENT = 0;
        protected const int MAX_VOLUME_PERCENT = 100;
        protected int _playlistSongCount;

        public LibVlcPlayer()
        {
            Initialize();

            _mainCancelTokenSource = new CancellationTokenSource();

            if (_mediaPlayer is not null)
            {
                _mediaPlayer.Paused += HandlePausedState;
                _mediaPlayer.Stopped += HandleStoppedState;
                _mediaPlayer.Playing += HandlePlayingState;
                _mediaPlayer.EndReached += HandleEndReachedState;
                _mediaPlayer.EncounteredError += HandlePlayBackError;
                _mediaPlayer.TimeChanged += HandleTimeChanged;
            }            
        }

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

        protected ISong _nextSong;
        public virtual ISong NextSong
        {
            get { return _nextSong; }
            set { _nextSong = value; }
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
                    if (value < 0)
                    {
                        value = 0;
                        IsMuted = true;
                    }

                    if (value > 100)
                        value = 100;

                    _currentVolume = value;
                    AdjustPlayerVolume();
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

        public event EventHandler<PlayerEvent> PlayerEvent;

        public virtual void AddSongsToPlayList(ICollection<ISong> songs)
        {
            foreach (var song in songs)
            {
                _playlistSongCount++;
                song.Order = _playlistSongCount;
                PlayList.Add(song);
            }

            PublishEvent($"Added {songs.Count} songs to playlist");
        }

        public virtual async Task AddSongsToPlayList(ICollection<string> files, CancellationToken addSongsCancelToken)
        {
            var songs = await GetSongsFromFiles(files, addSongsCancelToken);

            AddSongsToPlayList(songs);
        }

        public virtual async Task AddSongsToPlayList(string folderPath, CancellationToken addSongsCancelToken)
        {
            var songs = await GetSongsFromFolder(folderPath, addSongsCancelToken);

            AddSongsToPlayList(songs);
        }

        public virtual void AddSongToPlayList(ISong song)
        {
            _playlistSongCount++;
            song.Order = _playlistSongCount;
            PlayList.Add(song);
        }

        public virtual void AddSongToPlayList(string filePath)
        {
            var song = GetSongFromFile(filePath);

            if (song is not null)
            {
                AddSongToPlayList(song);
            }
        }

        public virtual void ClearPlayList()
        {
            _playlistSongCount = 0;
            PlayList.Clear();
            PublishEvent("Cleared Playlist");
        }

        public virtual void Dispose()
        {
            if (_mediaPlayer is not null)
            {
                _mediaPlayer.Paused -= HandlePausedState;
                _mediaPlayer.Stopped -= HandleStoppedState;
                _mediaPlayer.Playing -= HandlePlayingState;
                _mediaPlayer.EncounteredError -= HandlePlayBackError;
                _mediaPlayer.TimeChanged -= HandleTimeChanged;
            }            

            _mediaPlayer?.Dispose();
            _libVlc?.Dispose();
        }

        public virtual void Initialize()
        {
            if (_mediaPlayer is not null)
                _mediaPlayer.Dispose();

            if (_libVlc is not null)
                _libVlc.Dispose();
            
            LibVLCSharp.Shared.Core.Initialize();

            _libVlc = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new MediaPlayer(_libVlc);
            
            CurrentVolume = 50;
        }

        public virtual void Next()
        {
            if (PlayList.Count <= 0)
                return;

            var currentIndex = PlayList.IndexOf(CurrentSong);

            var nextIndex = currentIndex + 1;

            if (nextIndex > PlayList.Count - 1)
                return;

            if (Status == Status.Paused)
                NextSong = PlayList?[nextIndex];

            Play(nextIndex);
        }

        public virtual void Pause()
        {
            _mediaPlayer?.Pause();
        }

        public virtual void Play()
        {
            if (Status == Status.Paused && CurrentSong != null && NextSong is null)
            {
                _mediaPlayer?.Play();
                return;
            }

            if (Status == Status.Paused && NextSong is not null)
            {
                CurrentSong = NextSong;
                NextSong = null;
            }

            ISong songToPlay = CurrentSong ?? PlayList?.FirstOrDefault();

            if (songToPlay is null)
                return;

            CurrentSong = songToPlay;

            Media song = new Media(_libVlc, songToPlay.Path);

            _mediaPlayer?.Play(song);
        }

        public virtual void Play(int playListIndex)
        {
            ISong songToPlay = PlayList?[playListIndex];

            if (songToPlay is null)
                return;

            CurrentSong = songToPlay;

            Play();
        }

        public virtual void Previous()
        {
            if (PlayList.Count <= 0)
                return;

            var currentIndex = PlayList.IndexOf(CurrentSong);

            var prevIndex = currentIndex - 1;

            if (prevIndex < 0)
                return;

            if (Status == Status.Paused)
                NextSong = PlayList?[prevIndex];

            Play(prevIndex);
        }

        public virtual void RemoveSongFromPlayList(ISong song)
        {
            PlayList.Remove(song);
            PublishEvent($"Removed song [{song.Name}] from playlist");
        }

        public virtual void SkipAhead()
        {
            if (_mediaPlayer is null)
                return;

            if (!_mediaPlayer.IsPlaying)
                return;

            var currentTime = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
            var newTime = currentTime.TotalSeconds + SKIP_INTERVAL_SECONDS;

            _mediaPlayer.SeekTo(TimeSpan.FromSeconds(newTime));
        }

        public virtual void SkipBack()
        {
            if (_mediaPlayer is null)
                return;

            if (!_mediaPlayer.IsPlaying)
                return;

            var currentTime = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
            var newTime = currentTime.TotalSeconds - SKIP_INTERVAL_SECONDS;

            _mediaPlayer.SeekTo(TimeSpan.FromSeconds(newTime));
        }

        public virtual void SkipTo(int seconds)
        {
            if (_mediaPlayer is null)
                return;

            _mediaPlayer?.SeekTo(TimeSpan.FromSeconds(seconds));
        }

        public virtual void Stop()
        {
            if (_mediaPlayer is null)
                return;

            _mediaPlayer?.Stop();
        }

        public virtual void SetVolume(float volume)
        {
            if (_mediaPlayer is null)
                return;

            CurrentVolume = volume;
        }        

        protected ISong GetSongFromFile(string path)
        {
            ISong song;

            if (string.IsNullOrEmpty(path))
                PublishEvent($"Cannot get song from empty file", Core.PlayerEvent.Type.Error, null);

            if (!File.Exists(path))
                PublishEvent($"File not found [{path}]", Core.PlayerEvent.Type.Error, new FileNotFoundException(path));

            try
            {
                using (TagLib.File file = TagLib.File.Create(new FileAbstraction(path)))
                {
                    song = new Song();

                    song.Name = !string.IsNullOrEmpty(file.Tag.Title) ? file.Tag.Title : $"{Path.GetFileNameWithoutExtension(file.Name)}";
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

                    if (files is null || files?.Count == 0)
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

        protected void AdjustPlayerVolume()
        {
            IsMuted = CurrentVolume <= 0;

            if (_mediaPlayer is not null)
                _mediaPlayer.Volume = (int)CurrentVolume;
        }

        protected void PublishEvent(string message, Core.PlayerEvent.Type type = Core.PlayerEvent.Type.Information, Exception ex = null)
        {
            var errorDetails = (type == Core.PlayerEvent.Type.Error ? (ex is null ? string.Empty : ex.Message) : string.Empty);
            var msg = string.IsNullOrEmpty(errorDetails) ? message : message + " - " + errorDetails;

            PlayerEvent?.Invoke(this, new PlayerEvent(msg, type));
        }

        protected void HandlePausedState(object sender, EventArgs e)
        {
            Status = Status.Paused;
            PublishEvent($"Paused: {CurrentSong.Name}");
        }

        protected void HandleStoppedState(object sender, EventArgs e)
        {
            Status = Status.Stopped;
            PublishEvent("Stopped");
        }

        protected void HandlePlayingState(object sender, EventArgs e)
        {
            Status = Status.Playing;
            PublishEvent($"Playing: {CurrentSong.Name}");
        }

        protected void HandleEndReachedState(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ => Next());
        }

        protected void HandlePlayBackError(object sender, EventArgs e)
        {
            var er = "Error during playback...";
            PublishEvent(er);
        }

        protected void HandleTimeChanged(object sender, MediaPlayerTimeChangedEventArgs timeEvent)
        {
            if (timeEvent is null)
                return;

            CurrentElapsedTime = TimeSpan.FromMilliseconds(timeEvent.Time);
        }
    }
}