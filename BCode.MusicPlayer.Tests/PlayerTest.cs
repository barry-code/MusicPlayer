using BCode.MusicPlayer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace BCode.MusicPlayer.WpfPlayerTests
{
    public class PlayerTest// : IDisposable
    {
        private readonly IPlayer _player;
        private readonly string Good_Quality_Test_Files_Path = @"C:\temp\unit testing\music player\good quality songs";
        private readonly string Corrupted_Test_Files_Path = @"C:\temp\unit testing\music player\corrupted songs";
        private readonly string[] musicFileExtensions = { ".mp3", ".mp4", ".wav", ".wma" };

        public PlayerTest(IPlayer player) => _player = player;
        

        [Fact]
        public void VolumeZero_IsMutedShouldBeTrue()
        {
            //arrange
            _player.CurrentVolume = 0.5f;

            //act
            _player.CurrentVolume = 0f;

            //assert
            Assert.True(_player.IsMuted);
        }

        [Fact]
        public void VolumeNotZero_IsMutedShouldBeFalse()
        {

            //arrange
            _player.CurrentVolume = 0.0f;

            //act
            _player.CurrentVolume = 0.5f;

            //assert
            Assert.False(_player.IsMuted);
        }

        [Fact]
        public void VolumeUp_VolumeShouldIncrease()
        {
            //arrange
            float initVolume = 0.0f;
            _player.CurrentVolume = initVolume;

            //act
            _player.VolumeUp();

            //assert
            Assert.True(_player.CurrentVolume > initVolume); 
        }

        [Fact]
        public void VolumeDown_VolumeShouldDecrease()
        {
            //arrange
            float initVolume = 1.0f;
            _player.CurrentVolume = initVolume;

            //act
            _player.VolumeDown();

            //assert
            Assert.True(_player.CurrentVolume < initVolume);
        }

        [Fact]
        public void AddSongsToPlaylist_SongsShouldBeAddedToPlayList()
        {
            //arrange
            var songs = GetSampleSongFilesForTesting();

            //act
            SetupNewSamplePlayList();

            //assert
            Assert.True(_player.PlayList.Count == songs.Count);           
        }

        [Fact]
        public void RemoveSongFromPlaylist_SongShouldBeRemovedFromPlayList()
        {
            //arrange
            SetupNewSamplePlayList();
            var song = _player.PlayList.FirstOrDefault();

            if (song is null)
            {
                throw new System.Exception("no songs in playlist");
            }

            //act
            _player.RemoveSongFromPlayList(song);

            //assert
            Assert.DoesNotContain<ISong>(song, _player.PlayList);
        }

        [Fact]
        public void ClearPlaylist_PlayListShouldBeEmpty()
        {
            //arrange
            SetupNewSamplePlayList();

            //act
            _player.PlayList.Clear();

            //assert
            Assert.Empty(_player.PlayList);
            Assert.True(_player.CurrentSong is null);
            Assert.False(_player.IsPlaying);
            Assert.True(_player.Status == Status.Stopped);
        }

        [Fact]
        public void PlayFromStoppedState_SongShouldPlay()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();

            //Act
            _player.Play();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.True(_player.IsPlaying);
            Assert.True(_player.Status == Status.Playing);
        }

        [Fact]
        public void Pause_SongShouldPause()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();
            _player.SkipAhead();

            //Act
            _player.Pause();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.False(_player.IsPlaying);
            Assert.True(_player.Status == Status.Paused);
            Assert.True(_player.CurrentElapsedTime > TimeSpan.Zero);
        }

        [Fact]
        public void PlayFromPausedState_SongShouldContinuePlaying()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();
            _player.SkipAhead();
            _player.Pause();
            var currentSongTime = _player.CurrentElapsedTime;

            //Act
            _player.Play();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.True(_player.IsPlaying);
            Assert.True(_player.Status == Status.Playing);
            Assert.True(currentSongTime >= _player.CurrentElapsedTime);
        }

        [Fact]
        public void Stop_SongShouldStop()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();

            //Act
            _player.Stop();

            //Assert
            Assert.True(_player.CurrentSong is null);
            Assert.False(_player.IsPlaying);
            Assert.True(_player.Status == Status.Stopped);
            Assert.True(_player.CurrentElapsedTime == TimeSpan.Zero);
        }

        [Fact]
        public void SkipAhead_SongElapsedTimeShouldMoveForward()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();
            var currentSongTime = _player.CurrentElapsedTime;

            //Act
            _player.SkipAhead();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.True(_player.IsPlaying);
            Assert.True(_player.Status == Status.Playing);
            Assert.True(_player.CurrentElapsedTime > currentSongTime);
        }

        [Fact]
        public void SkipBack_SongElapsedTimeShouldMoveBackward()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();
            _player.SkipAhead();
            var currentSongTime = _player.CurrentElapsedTime;

            //Act
            _player.SkipBack();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.True(_player.IsPlaying);
            Assert.True(_player.Status == Status.Playing);
            Assert.True(_player.CurrentElapsedTime < currentSongTime);
        }

        [Fact]
        public void Next_NextSongShouldPlay()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();
            var intialSong = _player.CurrentSong;
            var currentIndex = _player.PlayList.IndexOf(intialSong);
            var nextIndex = currentIndex + 1;

            if (nextIndex > _player.PlayList.Count - 1)
                throw new Exception("not enough songs in playlist for testing");

            var nextSong = _player.PlayList[nextIndex];

            //Act
            _player.Next();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.True(_player.IsPlaying);
            Assert.True(_player.Status == Status.Playing);            
            Assert.True(_player.CurrentSong == nextSong);
        }

        [Fact]
        public void Previous_PreviousSongShouldPlay()
        {
            //Arrange
            _player.ClearPlayList();
            AddSampleSongsToPlayList();
            _player.Play();
            _player.Next();
            var intialSong = _player.CurrentSong;
            var currentIndex = _player.PlayList.IndexOf(intialSong);
            var prevIndex = currentIndex - 1;

            if (prevIndex < 0)
                throw new Exception("not enough songs in playlist for testing");

            var prevSong = _player.PlayList[prevIndex];

            //Act
            _player.Previous();

            //Assert
            Assert.True(_player.CurrentSong is not null);
            Assert.True(_player.IsPlaying);
            Assert.True(_player.Status == Status.Playing);
            Assert.True(_player.CurrentSong == prevSong);
        }

        private void SetupNewSamplePlayList()
        {
            _player.ClearPlayList();

            AddSampleSongsToPlayList();
        }

        private void AddSampleSongsToPlayList()
        {
            var songs = GetSampleSongFilesForTesting();

            foreach (var item in songs)
            {
                _player.AddSongToPlayList(item);
            }
        }

        private List<string> GetSampleSongFilesForTesting(FileQuality quality = FileQuality.Good)
        {
            List<string> songFiles = new();

            var path = quality == FileQuality.Corrupted ? Corrupted_Test_Files_Path : Good_Quality_Test_Files_Path;

            Directory.CreateDirectory(path);

            var foundSongs = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(f => musicFileExtensions.Contains(Path.GetExtension(f))).ToList();

            if (foundSongs.Count == 0)
            {
                throw new FileNotFoundException($"no sample music files found for testing in {path}");
            }

            if (foundSongs.Count < 3 && quality == FileQuality.Good)
            {
                throw new FileNotFoundException($"minimum of 3 sample music files required for testing in {path}");
            }

            songFiles.AddRange(foundSongs);

            return songFiles;
        }

        public void Dispose()
        {
            //if (_player is not null)
            //{
            //    _player.Dispose();
            //}
        }

        public enum FileQuality
        {
            Good,
            Corrupted            
        }
    }
}