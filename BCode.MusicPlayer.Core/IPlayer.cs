using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public interface IPlayer : IDisposable
    {
        IList<Song> PlayList { get; set; }
        Song CurrentSong { get; set; }
        Status Status { get; set; }
        float CurrentVolume { get; set; }
        TimeSpan CurrentSongElapsedTime { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsMuted { get; set; }

        event EventHandler<PlayerEvent> PlayerEvent;
        public bool IsBrowseMode { get; set; }
        IList<Song> BrowseModePlayList { get; set; }

        void Initialize();
        void Play();
        void Play(int playListIndex);
        void Pause();
        void Stop();
        void Next();
        void Previous();
        void SkipAhead();
        void SkipBack();
        void SkipTo(int seconds);
        void AddSongToPlayList(Song song);
        void AddSongToPlayList(string filePath);
        void AddSongsToPlayList(ICollection<Song> songs);
        Task AddSongsToPlayList(ICollection<string> files, CancellationToken addSongsCancelToken);
        Task AddSongsToPlayList(string folderPath, CancellationToken addSongsCancelToken);
        void RemoveSongFromPlayList(Song song);
        void ClearPlayList();
        void SetVolume(float volume);
        void Mute();
        void UnMute();
        Task StartBrowseMode(string fileFullPath, bool startPlaying = false);
        void StopBrowseMode();
        void Exit();
    }
}
