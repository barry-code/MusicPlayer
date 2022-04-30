using BCode.MusicPlayer.Core;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace BCode.MusicPlayer.WpfPlayer.ViewModel
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private ILogger _logger;
        private CancellationTokenSource _cancelTokenSource;
        private const int NOTIFICATION_POP_UP_DURATION_MILLISECONDS = 2000;        
        private SnackbarMessageQueue _notificationMessageQueue;
        private Timer _notificationsFinishedTimer;

        public MainWindowViewModel(IPlayer player, ILogger logger)
        {
            Player = player;
            _logger = logger;
            SetupNotifications();

            Player.PlayerEvent += HandlePlayerEvent;

            _logger.LogDebug("Starting Music Player");

            AddFilesCmd = ReactiveCommand.CreateFromTask(AddFiles);
            AddFolderCmd = ReactiveCommand.CreateFromTask(AddSongsToPlaylist);
            ClearPlayListCmd = ReactiveCommand.Create(ClearPlaylist);
            PlayPauseCmd = ReactiveCommand.Create(PlayPause);
            PlaySongFromPlayListCmd = ReactiveCommand.Create<int>(PlaySongFromPlaylist);
            NextCmd = ReactiveCommand.Create(Next);
            PrevCmd = ReactiveCommand.Create(Previous);
            StopCmd = ReactiveCommand.Create(Stop);
            SkipAheadCmd = ReactiveCommand.Create(SkipAhead);
            SkipBackCmd = ReactiveCommand.Create(SkipBack);
            CancelLoadCmd = ReactiveCommand.Create(CancelLoad);
            MuteCmd = ReactiveCommand.Create(Mute);
            UnMuteCmd = ReactiveCommand.Create(UnMute);
        }

        public ReactiveCommand<Unit, Unit> AddFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> AddFolderCmd { get; }
        public ReactiveCommand<Unit, Unit> ClearPlayListCmd { get; }
        public ReactiveCommand<Unit, Unit> PlayPauseCmd { get; }
        public ReactiveCommand<int, Unit> PlaySongFromPlayListCmd { get; }
        public ReactiveCommand<Unit, Unit> NextCmd { get; }
        public ReactiveCommand<Unit, Unit> PrevCmd { get; }
        public ReactiveCommand<Unit, Unit> StopCmd { get; }
        public ReactiveCommand<Unit, Unit> SkipAheadCmd { get; }
        public ReactiveCommand<Unit, Unit> SkipBackCmd { get; }
        public ReactiveCommand<Unit, Unit> CancelLoadCmd { get; }
        public ReactiveCommand<Unit, Unit> MuteCmd { get; }
        public ReactiveCommand<Unit, Unit> UnMuteCmd { get; }

        public IPlayer Player { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private int _currentSongTimeSeconds;
        public int CurrentSongTimeSeconds
        {
            get => _currentSongTimeSeconds;
            set => SetSeekTime(value);
        }

        private string _currentStatusMessage;
        public string CurrentStatusMessage
        {
            get => _currentStatusMessage;
            set => this.RaiseAndSetIfChanged(ref _currentStatusMessage, value);
        }

        private bool _showNotificationAlert;
        public bool ShowNotificationAlert
        {
            get => _showNotificationAlert;
            set => this.RaiseAndSetIfChanged(ref _showNotificationAlert, value);
        }

        public double CurrentSongMaxTime => Player?.CurrentSong?.Duration.TotalSeconds ?? 0;

        public SnackbarMessageQueue NotificationMessageQueue => _notificationMessageQueue;

        public int NotificationsRemainingCount => _notificationMessageQueue.QueuedMessages.Count;

        public void Dispose()
        {
            try
            {
                _notificationsFinishedTimer?.Dispose();
                _notificationMessageQueue?.Clear();
                _notificationMessageQueue?.Dispose();

                _cancelTokenSource?.Dispose();

                if (Player is not null)
                {
                    Player.PlayerEvent -= HandlePlayerEvent;
                    Player.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

        private void PlayPause()
        {
            try
            {
                if (Player.IsPlaying)
                {
                    Player.Pause();
                    return;
                }

                Player.Play();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }            
        }

        private void Previous()
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

        private void Stop()
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

        private void Next()
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

        private void SkipAhead()
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

        private void SkipBack()
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

        private void ClearPlaylist()
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

        private async Task AddFiles() 
        {
            try
            {                
                var dlg = new OpenFileDialog();
                dlg.Multiselect = true;
                dlg.Filter = GetAudioFilesFilterString();

                var result = dlg.ShowDialog();

                if (result != DialogResult.OK)
                    return;

                _cancelTokenSource = new CancellationTokenSource();

                IsLoading = true;

                var files = dlg.FileNames;

                await Player.AddSongsToPlayList(files, _cancelTokenSource.Token);
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

        private async Task AddSongsToPlaylist()
        {
            try
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();

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

        private void PlaySongFromPlaylist(int playlistIndex)
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

        private void Mute()
        {
            try
            {
                Player.Mute();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }            
        }

        private void UnMute()
        {            
            try
            {
                Player.UnMute();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void HandlePlayerEvent(object sender, EventArgs e)
        {
            var ev = e as PlayerEvent;

            if (ev is not null)
            {
                if (ev.EventType == PlayerEvent.Type.Information && ev.EventCategory == PlayerEvent.Category.TrackTimeChanged)
                {
                    var newTime = (int)(Player?.CurrentSongElapsedTime.TotalSeconds ?? 0);
                    
                    if (_currentSongTimeSeconds != newTime)
                    {
                        _currentSongTimeSeconds = newTime;
                        this.RaisePropertyChanged(nameof(CurrentSongTimeSeconds));
                    }
                                      
                    return;
                }

                if (ev.Message == "Stopped")
                {
                    return;
                }

                if (ev.Message.StartsWith("Added "))
                {
                    ShowNotificationPopUp(ev.Message);
                    return;
                }

                if (ev.EventType == PlayerEvent.Type.Error)
                {
                    _logger.LogError(ev.Message);
                    ShowNotificationPopUp(ev.Message);
                    return;
                }

                this.RaisePropertyChanged(nameof(CurrentSongMaxTime));
                _logger.LogInformation(ev.Message);
                CurrentStatusMessage = ev.Message;
            }
        }

        private void CancelLoad()
        {
            _cancelTokenSource.Cancel();
            IsLoading = false;
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

        private void SetupNotifications()
        {
            _notificationMessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(NOTIFICATION_POP_UP_DURATION_MILLISECONDS));
            _notificationMessageQueue.DiscardDuplicates = true;
            _notificationsFinishedTimer = new Timer(NotificationsFinished, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void HideNotificationsPopup()
        {
            _notificationsFinishedTimer.Change(500, Timeout.Infinite);
        }

        private void ShowNotificationPopUp(string msg) 
        {                  
            _notificationMessageQueue.Enqueue(msg);
            ShowNotificationAlert = true;
        }

        private void NotificationsFinished(object state)
        {            
            ShowNotificationAlert = false;
        }

        private string GetAudioFilesFilterString()
        {
            var filterString = "";
            if (Core.Constants.AudioFileExtensions.Count() == 0)
            {
                filterString = "*.*";
            }

            filterString = "Audio Files|" + string.Join(";", Core.Constants.AudioFileExtensions.Select(f => $"*{f}"));

            return filterString;
        }
    }    
}
