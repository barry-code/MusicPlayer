using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public interface IPlayer : IDisposable
    {
        IList<ISong> PlayList { get; set; }
        ISong CurrentSong { get; set; }
        Status Status { get; set; }
        float CurrentVolume { get; set; }
        TimeSpan CurrentElapsedTime { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsMuted { get; set; }

        event EventHandler<PlayerEvent> PlayerEvent;


        void Play();
        void Play(int playListIndex);
        void Pause();
        void Stop();
        void Next();
        void Previous();
        void AddSongToPlayList(ISong song);
        void AddSongToPlayList(string filePath);
        void AddSongsToPlayList(ICollection<ISong> songs);
        Task AddSongsToPlayList(ICollection<string> files, CancellationToken addSongscancelToken);
        Task AddSongsToPlayList(string folderPath, CancellationToken addSongscancelToken);
        void RemoveSongFromPlayList(ISong song);
        void ClearPlayList();
        void OnLoad();
        void VolumeUp();
        void VolumeDown();
        void SkipAhead();
        void SkipBack();
        void SkipTo(int seconds);
    }
}
