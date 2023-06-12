using BCode.MusicPlayer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

namespace BCode.MusicPlayer.WpfPlayerTests
{
    public class PlayerTest : IClassFixture<PlayerFixture>
    {
        PlayerFixture _fixture;                
        private readonly string Good_Quality_Test_Files_Path = @"samples\GoodQuality";
        private readonly string Corrupted_Test_Files_Path = @"samples\BadQuality";

        public PlayerTest(PlayerFixture fixture)
        {
            _fixture = fixture;
        }               

        [Fact]
        public void Mute_IsMutedShouldBeTrueAndVolumeZero()
        {
            //arrange
            _fixture.Player.CurrentVolume = 0.5f;

            //act
            _fixture.Player.Mute();

            //assert
            Assert.True(_fixture.Player.IsMuted);
            Assert.True(_fixture.Player.CurrentVolume <= 0);
        }

        [Fact]
        public void UnMute_IsMutedShouldBeFalseAndVolumeReturnsToPreviousValue()
        {

            //arrange            
            _fixture.Player.CurrentVolume = 0.5f;
            var previousVolume = _fixture.Player.CurrentVolume;
            _fixture.Player.Mute();

            //act
            _fixture.Player.UnMute();            

            //assert
            Assert.False(_fixture.Player.IsMuted);
            Assert.True(_fixture.Player.CurrentVolume == previousVolume);
        }

        [Fact]
        public void AddSongsToPlaylist_SongsShouldBeAddedToPlayList()
        {
            //arrange
            var songs = GetSampleSongFilesForTesting();            

            //act
            SetupNewSamplePlayList();

            //assert
            Assert.True(_fixture.Player.PlayList.Count == songs.Count);           
        }

        [Fact]
        public void RemoveSongFromPlaylist_SongShouldBeRemovedFromPlayList()
        {
            //arrange
            SetupNewSamplePlayList();
            var song = _fixture.Player.PlayList.FirstOrDefault();

            if (song is null)
            {
                throw new System.Exception("no songs in playlist");
            }

            //act
            _fixture.Player.RemoveSongFromPlayList(song);

            //assert
            Assert.DoesNotContain<Song>(song, _fixture.Player.PlayList);
        }

        [Fact]
        public void ClearPlaylist_PlayListShouldBeEmpty()
        {
            //arrange
            SetupNewSamplePlayList();

            //act
            _fixture.Player.PlayList.Clear();

            //assert
            Assert.Empty(_fixture.Player.PlayList);
            Assert.True(_fixture.Player.CurrentSong is null);
            Assert.False(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Stopped);
        }

        [Fact]
        public void PlayFromStoppedState_SongShouldPlay()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            Thread.Sleep(1000);

            //Act
            _fixture.Player.Play();
            Thread.Sleep(1000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.True(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Playing);
        }

        [Fact]
        public void Pause_SongShouldPause()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            _fixture.Player.Play();
            _fixture.Player.SkipAhead();
            Thread.Sleep(1000);

            //Act
            _fixture.Player.Pause();
            Thread.Sleep(1000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.False(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Paused);
            Assert.True(_fixture.Player.CurrentSongElapsedTime > TimeSpan.Zero);
        }

        [Fact]
        public void PlayFromPausedState_SongShouldContinuePlaying()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            Thread.Sleep(1000);
            _fixture.Player.Play();
            _fixture.Player.SkipAhead();
            _fixture.Player.Pause();
            Thread.Sleep(1000);
            var pausedSongTime = _fixture.Player.CurrentSongElapsedTime.TotalMilliseconds;

            //Act
            _fixture.Player.Play();
            Thread.Sleep(1000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.True(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Playing);
            Assert.True(_fixture.Player.CurrentSongElapsedTime.TotalMilliseconds >= pausedSongTime);
        }

        [Fact]
        public void Stop_SongShouldStop()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            _fixture.Player.Play();
            Thread.Sleep(1000);

            //Act
            _fixture.Player.Stop();
            Thread.Sleep(1000);

            //Assert
            Assert.False(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Stopped);
        }

        [Fact]
        public void SkipAhead_SongElapsedTimeShouldMoveForward()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            _fixture.Player.Play();
            Thread.Sleep(1000);
            var currentSongTime = _fixture.Player.CurrentSongElapsedTime;

            //Act
            _fixture.Player.SkipAhead();
            Thread.Sleep(1000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.True(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Playing);
            Assert.True(_fixture.Player.CurrentSongElapsedTime > currentSongTime);
        }

        [Fact]
        public void SkipBack_SongElapsedTimeShouldMoveBackward()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            _fixture.Player.Play();
            Thread.Sleep(1000);
            _fixture.Player.SkipAhead();
            Thread.Sleep(1000);
            var currentSongTime = _fixture.Player.CurrentSongElapsedTime;

            //Act
            _fixture.Player.SkipBack();
            Thread.Sleep(1000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.True(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Playing);
            Assert.True(_fixture.Player.CurrentSongElapsedTime < currentSongTime);
        }

        [Fact]
        public void Next_NextSongShouldPlay()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            _fixture.Player.Play();
            var intialSong = _fixture.Player.CurrentSong;
            var currentIndex = _fixture.Player.PlayList.IndexOf(intialSong);
            var nextIndex = currentIndex + 1;

            if (nextIndex > _fixture.Player.PlayList.Count - 1)
                throw new Exception("not enough songs in playlist for testing");

            var nextSong = _fixture.Player.PlayList[nextIndex];

            //Act
            _fixture.Player.Next();
            Thread.Sleep(2000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.True(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Playing);            
            Assert.True(_fixture.Player.CurrentSong == nextSong);
        }

        [Fact]
        public void Previous_PreviousSongShouldPlay()
        {
            //Arrange
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            var lastSongIndex = _fixture.Player.PlayList.Count - 1;
            _fixture.Player.Play(lastSongIndex);
            var intialSong = _fixture.Player.CurrentSong;
            var currentIndex = _fixture.Player.PlayList.IndexOf(intialSong);
            var prevIndex = currentIndex - 1;

            if (prevIndex < 0)
                throw new Exception("not enough songs in playlist for testing");

            var prevSong = _fixture.Player.PlayList[prevIndex];

            //Act
            _fixture.Player.Previous();
            Thread.Sleep(2000);

            //Assert
            Assert.True(_fixture.Player.CurrentSong is not null);
            Assert.True(_fixture.Player.IsPlaying);
            Assert.True(_fixture.Player.Status == Status.Playing);
            Assert.True(_fixture.Player.CurrentSong == prevSong);
        }

        [Fact]
        public void SkipTo_SongTimeShouldMoveToSpecifiedTime()
        {
            //Arrange
            int secondsToSkipTo = 28;
            _fixture.Player.ClearPlayList();
            AddSampleSongsToPlayList();
            _fixture.Player.Play();
            Thread.Sleep(1000);

            //Act
            Thread.Sleep(1000);            
            _fixture.Player.SkipTo(secondsToSkipTo);
            _fixture.Player.Pause();

           //Assert
           Assert.True(_fixture.Player.CurrentSongElapsedTime.Seconds == secondsToSkipTo);
        }

        [Fact]
        public void SetVolume_VolumeShouldMoveToSpecifiedVolume()
        {
            //Arrange
            _fixture.Player.CurrentVolume = 0.9f;
            var newVolume = 0.2f;

            //Act
            _fixture.Player.SetVolume(newVolume);

            //Assert
            Assert.True(_fixture.Player.CurrentVolume == newVolume);
        }

        private void SetupNewSamplePlayList(FileQuality quality = FileQuality.Good)
        {
            _fixture.Player.ClearPlayList();

            AddSampleSongsToPlayList(quality);
        }

        private void AddSampleSongsToPlayList(FileQuality quality = FileQuality.Good)
        {
            MutePlayer();
            
            var songs = GetSampleSongFilesForTesting(quality);

            foreach (var item in songs)
            {
                _fixture.Player.AddSongToPlayList(item);
            }
        }

        private List<string> GetSampleSongFilesForTesting(FileQuality quality = FileQuality.Good)
        {
            List<string> songFiles = new();

            var songDirectory = quality == FileQuality.Corrupted ? Corrupted_Test_Files_Path : Good_Quality_Test_Files_Path;
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(assemblyLocation))
                throw new FileNotFoundException();

            var path = Path.Combine(assemblyLocation, songDirectory);

            var foundSongs = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(f => Constants.AudioFileExtensions.Contains(Path.GetExtension(f))).ToList();

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

        private void MutePlayer()
        {
            _fixture.Player.Mute();
        }

        public enum FileQuality
        {
            Good,
            Corrupted            
        }
    }
}