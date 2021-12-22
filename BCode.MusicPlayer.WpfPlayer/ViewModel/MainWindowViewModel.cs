using BCode.MusicPlayer.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace BCode.MusicPlayer.WpfPlayer.ViewModel
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private ILogger _logger;
        private CancellationTokenSource _cancelTokenSource;

        public MainWindowViewModel(IPlayer player, ILogger logger)
        {
            Player = player;
            _logger = logger;            

            Player.PlayerEvent += HandlePlayerEvent;

            _logger.LogDebug("Starting");

            AddFilesCmd = ReactiveCommand.CreateFromTask(AddSongsToPlaylist);
            ClearPlayListCmd = ReactiveCommand.Create(ClearPlaylist);
            PlayCmd = ReactiveCommand.Create(Play);
            PlaySongFromPlayListCmd = ReactiveCommand.Create<int>(PlaySongFromPlaylist);
            PauseCmd = ReactiveCommand.Create(Pause);
            NextCmd = ReactiveCommand.Create(Next);
            PrevCmd = ReactiveCommand.Create(Previous);
            StopCmd = ReactiveCommand.Create(Stop);
            SkipAheadCmd = ReactiveCommand.Create(SkipAhead);
            SkipBackCmd = ReactiveCommand.Create(SkipBack);
            CancelLoadCmd = ReactiveCommand.Create(CancelLoad);
        }

        public ReactiveCommand<Unit, Unit> AddFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> ClearPlayListCmd { get; }
        public ReactiveCommand<Unit, Unit> PlayCmd { get; }
        public ReactiveCommand<int, Unit> PlaySongFromPlayListCmd { get; }
        public ReactiveCommand<Unit, Unit> PauseCmd { get; }
        public ReactiveCommand<Unit, Unit> NextCmd { get; }
        public ReactiveCommand<Unit, Unit> PrevCmd { get; }
        public ReactiveCommand<Unit, Unit> StopCmd { get; }
        public ReactiveCommand<Unit, Unit> SkipAheadCmd { get; }
        public ReactiveCommand<Unit, Unit> SkipBackCmd { get; }
        public ReactiveCommand<Unit, Unit> CancelLoadCmd { get; }


        public IPlayer Player { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public void Play()
        {
            try
            {
                Player.Play();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }            
        }

        public void Pause()
        {            
            try
            {
                Player.Pause();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Previous()
        {
            try
            {
                Player.Previous();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Stop()
        {
            try
            {
                Player.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Next()
        {
            try
            {
                Player.Next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void SkipAhead()
        {
            try
            {
                Player.SkipAhead();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void SkipBack()
        {
            try
            {
                Player.SkipBack();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void ClearPlaylist()
        {
            try
            {
                Player.ClearPlayList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task AddSongsToPlaylist()
        {
            try
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.Description = "Select folder containing songs to load...";

                var result = dlg.ShowDialog();

                if (result != DialogResult.OK)
                    return;

                _cancelTokenSource = new CancellationTokenSource();
                IsLoading = true;

                var path = dlg.SelectedPath;

                await Player.AddSongsToPlayList(path, _cancelTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                IsLoading = false;
                _cancelTokenSource?.Dispose();
            }
        }

        public void PlaySongFromPlaylist(int playlistIndex)
        {
            try
            {
                Player.Play(playlistIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private void HandlePlayerEvent(object sender, EventArgs e)
        {
            var ev = e as PlayerEvent;

            if (ev is not null)
            {
                if (ev.EventType == PlayerEvent.Type.Error)
                {
                    _logger.LogError(ev.Message);
                    MessageBox.Show("ERROR:" + ev.Message);
                    return;
                }

                _logger.LogInformation(ev.Message);
            }
        }

        private void CancelLoad()
        {
            _cancelTokenSource.Cancel();
            IsLoading = false;
        }

        public void Dispose()
        {
            _cancelTokenSource?.Dispose();

            if (Player is not null)
            {
                Player.PlayerEvent -= HandlePlayerEvent;
                Player.Dispose();
            }
            
        }
    }
}
