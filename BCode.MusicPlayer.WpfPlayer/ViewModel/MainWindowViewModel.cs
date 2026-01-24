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
using System.Windows;
using System.Reflection;
using BCode.MusicPlayer.WpfPlayer.Shared;

namespace BCode.MusicPlayer.WpfPlayer.ViewModel
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private ILogger _logger;
        private CancellationTokenSource _cancelTokenSource;
        private const int PopUpDurationMilliseconds = 2000;        
        private SnackbarMessageQueue _notificationMessageQueue;
        private Timer _notificationsFinishedTimer;
        private string _appName = string.Empty;
        private const int DefaultHeight = 700;
        private const int DefaultWidth = 900;
        private const int MinimizedModeMinHeight = 295;
        private const int ExpandedModeMinHeight = 540;
        private const int MinimizedModeMinWidth = 600;
        private const int ExpandedModeMinWidth = 600;

        public MainWindowViewModel(IPlayer player, ILogger logger)
        {
            Player = player;
            _logger = logger;
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                          ?? assembly.GetName().Version?.ToString();

            _appName = $"{assembly.GetName().Name} {version}";

            SetupNotifications();

            Player.PlayerEvent += HandlePlayerEvent;

            _logger.LogDebug("Starting Music Player");

            FileExplorer = new FileExplorer();

            AddFilesCmd = ReactiveCommand.CreateFromTask(AddFiles,
                outputScheduler: RxApp.MainThreadScheduler);
            AddFolderCmd = ReactiveCommand.CreateFromTask(AddSongsToPlaylist,
                outputScheduler: RxApp.MainThreadScheduler);
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
            BrowseCmd = ReactiveCommand.Create(BrowseMode);
            PlaylistCmd = ReactiveCommand.Create(NonBrowseMode);
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
        public ReactiveCommand<Unit, Unit> BrowseCmd { get; }
        public ReactiveCommand<Unit, Unit> PlaylistCmd { get; }

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

        private bool _isInMinimalMode;
        public bool IsInMinimalMode
        {
            get => _isInMinimalMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _isInMinimalMode, value);
                ExpandedVisibility = _isInMinimalMode ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Visibility _expandedVisibility = Visibility.Visible;
        public Visibility ExpandedVisibility
        {
            get => _expandedVisibility;
            set => this.RaiseAndSetIfChanged(ref _expandedVisibility, value);
        }

        private int _minHeight = ExpandedModeMinHeight;
        public int MinHeight
        {
            get => _minHeight;
            set => this.RaiseAndSetIfChanged(ref _minHeight, value);
        }

        private int _windowHeight = DefaultHeight;
        public int WindowHeight
        {
            get => _windowHeight;
            set => this.RaiseAndSetIfChanged(ref _windowHeight, value);
        }

        private int _minWidth = ExpandedModeMinWidth;
        public int MinWidth
        {
            get => _minWidth;
            set => this.RaiseAndSetIfChanged(ref _minWidth, value);
        }

        private int _windowWidth = DefaultWidth;
        public int WindowWidth
        {
            get => _windowWidth;
            set => this.RaiseAndSetIfChanged(ref _windowWidth, value);
        }

        private ResizeMode _resizeMode = ResizeMode.CanResizeWithGrip;
        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set => this.RaiseAndSetIfChanged(ref _resizeMode, value);
        }

        private FileExplorer _fileExplorer;
        public FileExplorer FileExplorer 
        { 
            get => _fileExplorer;
            set => this.RaiseAndSetIfChanged(ref _fileExplorer, value);
        }

        private bool _isBrowseScreen;
        public bool IsBrowseScreen
        {
            get => _isBrowseScreen;
            set => this.RaiseAndSetIfChanged(ref _isBrowseScreen, value);
        }

        public string AppName => _appName;        

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

        public void MinimalMode()
        {
            IsInMinimalMode = true;
            WindowHeight = MinHeight = MinimizedModeMinHeight;
            WindowWidth = MinWidth = MinimizedModeMinWidth;
            ResizeMode = ResizeMode.NoResize;
        }

        public void ExpandedMode()
        {
            IsInMinimalMode = false;            
            MinHeight = ExpandedModeMinHeight;
            WindowHeight = DefaultHeight;
            MinWidth = ExpandedModeMinWidth;
            WindowWidth = DefaultWidth;
            ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        private void PlayPause()
        {
            try
            {
                CheckBrowseMode();

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
                CheckBrowseMode();
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
                CheckBrowseMode();
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
                CheckBrowseMode();
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
                CheckBrowseMode();
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
                CheckBrowseMode();
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
                CurrentStatusMessage = "";
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
                CheckBrowseMode();

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

            if (ev is null)
                return;

            if (ev.EventCategory == PlayerEvent.Category.TrackTimeUpdate)
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

            if (ev.EventCategory == PlayerEvent.Category.PlayListUpdate)
            {
                ShowNotificationPopUp(ev.Message);
                return;
            }

            if (ev.EventCategory == PlayerEvent.Category.BrowseModePlayListUpdate)
            {
                GetBrowseModeSongDetails();
                return;
            }

            if (ev.EventType == PlayerEvent.Type.Error)
            {
                _logger.LogError(ev.Message);
                ShowNotificationPopUp(ev.Message);
                return;
            }

            if (ev.EventCategory == PlayerEvent.Category.TrackUpdate)
            {
                if (Player.IsBrowseMode)
                {
                    UpdateBrowseModeSong();
                }

                this.RaisePropertyChanged(nameof(CurrentSongMaxTime));
                _currentSongTimeSeconds = 0;
                this.RaisePropertyChanged(nameof(CurrentSongTimeSeconds));
            }

            _logger.LogInformation(ev.Message);
            CurrentStatusMessage = ev.Message;
        }

        private void UpdateBrowseModeSong()
        {
            try
            {

                //TODO is this method still needed, now that have SongDetail in the BrowseItem ???

                var currentSongPath = Player.CurrentSong.Path;

                if (FileExplorer.SelectedItem is null || FileExplorer.SelectedItem.Song is null)
                    return;

                var currentBrowseModeSelectedFile = FileExplorer.SelectedItem.Song.Path;

                if (currentSongPath == currentBrowseModeSelectedFile)
                    return;

                var newSongInBrowserScreen = FileExplorer.CurrentContent.Where(f => !f.IsDirectory).FirstOrDefault(f => f.Song.Path == currentSongPath);

                if (newSongInBrowserScreen is null)
                    return;

                FileExplorer.SelectedItem = newSongInBrowserScreen;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error updating browse mode song");
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
                if (songTimeInSeconds >= (int)CurrentSongMaxTime)
                {
                    return;
                }

                Player.SkipTo(songTimeInSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private void SetupNotifications()
        {
            _notificationMessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(PopUpDurationMilliseconds));
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

        private void BrowseMode()
        {
            IsBrowseScreen = true;
        }

        private void NonBrowseMode()
        {
            IsBrowseScreen = false;
        }

        private void CheckBrowseMode()
        {
            if (!IsBrowseScreen && Player.IsBrowseMode)
            {
                Player.StopBrowseMode();
            }
        }

        private void GetBrowseModeSongDetails()
        {
            //TODO: Refactor so that dont have to do this. Already have song details. Need to incorporate that into fileexplorer or replace file explorer.
            // DONT THINK THIS IS NEEDED ANYMORE

            //foreach (var item in FileExplorer.CurrentContent.Where(i => !i.IsDirectory))
            //{
            //    var songDetail = Player.BrowseModePlayList.FirstOrDefault(s => s.Path == item.FileDetail.FullName);

            //    if (songDetail is null)
            //        continue;

            //    item.Duration = songDetail.Duration.ToString(@"mm\:ss");
            //    item.Artist = songDetail.ArtistName;
            //}
        }

        public async Task AddItemFromBrowseScreenToPlaylist(object item)
        {
            try
            {
                var itemToAdd = item as BrowseItem;

                if (itemToAdd is null)
                    return;

                IsLoading = true;

                _cancelTokenSource = new CancellationTokenSource();

                if (itemToAdd.IsDirectory)
                {
                    await Player.AddSongsToPlayList(itemToAdd.DirectoryDetail.FullName, _cancelTokenSource.Token);

                    return;
                }

                await Player.AddSongToPlayList(itemToAdd.Song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error adding item from browse to playlist");
            }
            finally
            {
                IsLoading = false;
                _cancelTokenSource?.Dispose();
            }
            
        }
    }    
}

