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
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;

namespace BCode.MusicPlayer.WpfPlayer.ViewModel
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private ILogger _logger;
        private CancellationTokenSource _cancelTokenSource;
        private SnackbarMessageQueue _messageQueue = new SnackbarMessageQueue();

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

            this.WhenAnyValue(x => x.Player.CurrentSong)
                .Where(x => x != null)
                .DistinctUntilChanged()
                .Subscribe((x) => {
                    UpdateSongTimes(true);
                });

            this.WhenAnyValue(x => x.Player.CurrentElapsedTime)
                .WhereNotNull()
                .DistinctUntilChanged()
                .Subscribe((x) => { UpdateSongTimes(false); });
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

        public SnackbarMessageQueue MessageQueue => _messageQueue;

        public IPlayer Player { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private int _currentSongElapsedTime;
        public int CurrentSongElapsedTime 
        {
            get 
            { 
                return (int)(Player?.CurrentElapsedTime.TotalSeconds ?? 0); 
            }
            
            set
            {
                _currentSongElapsedTime = value;
                SetSeekTime(_currentSongElapsedTime);                
            } 
        }
        
        public int CurrentSongMaxTime => (int)(Player?.CurrentSong?.Duration.TotalSeconds ?? 0);


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
                    _messageQueue.Enqueue(ev.Message,null,null,null,true,false,TimeSpan.FromSeconds(2));
                    return;
                }

                if (ev.Message == "Stopped")
                {
                    return;
                }

                _logger.LogInformation(ev.Message);
                _messageQueue.Enqueue(ev.Message, null, null, null, false, false, TimeSpan.FromSeconds(1));
            }
        }

        private void CancelLoad()
        {
            _cancelTokenSource.Cancel();
            IsLoading = false;
        }

        private void UpdateSongTimes(bool isNewSong)
        {
            if (isNewSong)
            {
                this.RaisePropertyChanged(nameof(CurrentSongMaxTime));
            }

            this.RaisePropertyChanged(nameof(CurrentSongElapsedTime));
        }

        private void SetSeekTime(int songTimeInSeconds)
        {            
            try
            {
                Player.SkipTo(songTimeInSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void Dispose()
        {
            _cancelTokenSource?.Dispose();
            _messageQueue?.Clear();

            if (Player is not null)
            {
                Player.PlayerEvent -= HandlePlayerEvent;
                Player.Dispose();
            }
            
        }
    }    
}
