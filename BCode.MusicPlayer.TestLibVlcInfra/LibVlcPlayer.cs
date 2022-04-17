using BCode.MusicPlayer.Core;
using LibVLCSharp.Shared;

namespace BCode.MusicPlayer.TestLibVlcInfra
{
    public class LibVlcPlayer : IPlayer
    {
        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;

        public LibVlcPlayer()
        {
            _libVlc = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new MediaPlayer(_libVlc);
        }

        public IList<ISong> PlayList { get; set; }
        public ISong CurrentSong { get; set; }
        public Status Status { get; set; }
        public float CurrentVolume { get; set; }
        public TimeSpan CurrentElapsedTime { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsMuted { get; set; }

        public event EventHandler<PlayerEvent> PlayerEvent;

        public void AddSongsToPlayList(ICollection<ISong> songs)
        {
            throw new NotImplementedException();
        }

        public Task AddSongsToPlayList(ICollection<string> files, CancellationToken addSongscancelToken)
        {
            throw new NotImplementedException();
        }

        public Task AddSongsToPlayList(string folderPath, CancellationToken addSongscancelToken)
        {
            throw new NotImplementedException();
        }

        public void AddSongToPlayList(ISong song)
        {
            throw new NotImplementedException();
        }

        public void AddSongToPlayList(string filePath)
        {
            throw new NotImplementedException();
        }

        public void ClearPlayList()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Play(int playListIndex)
        {
            throw new NotImplementedException();
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public void RemoveSongFromPlayList(ISong song)
        {
            throw new NotImplementedException();
        }

        public void SkipAhead()
        {
            throw new NotImplementedException();
        }

        public void SkipBack()
        {
            throw new NotImplementedException();
        }

        public void SkipTo(int seconds)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void VolumeDown()
        {
            throw new NotImplementedException();
        }

        public void VolumeUp()
        {
            throw new NotImplementedException();
        }
    }
}