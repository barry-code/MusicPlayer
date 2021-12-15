using BCode.MusicPlayer.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace BCode.MusicPlayer.WpfPlayerTests
{
    public class PlayerTest
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

            var path = quality == Quality.Corrupted ? Corrupted_Test_Files_Path : Good_Quality_Test_Files_Path;

            Directory.CreateDirectory(path);

            var foundSongs = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(f => musicFileExtensions.Contains(Path.GetExtension(f))).ToList();

            if (foundSongs.Count == 0)
            {
                throw new FileNotFoundException($"no sample music files found for testing in {path}");
            }

            songFiles.AddRange(foundSongs);

            return songFiles;
        }

        public enum FileQuality
        {
            Good,
            Corrupted            
        }
    }
}