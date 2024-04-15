using BCode.MusicPlayer.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BCode.MusicPlayer.Infrastructure
{
    public class WpfPlayer : LibVlcPlayer, INotifyPropertyChanged
    {
        SettingsManager _settingsManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public override IList<Song> PlayList { get; set; } = new ObservableCollection<Song>();
        public override IList<Song> BrowseModePlayList { get; set; } = new ObservableCollection<Song>();

        public override Song CurrentSong
        {
            get { return _currentSong; }

            set
            {
                _currentSong = value;
                NotifyPropertyChanged();
            }
        }

        public override Status Status
        {
            get { return _status; }

            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged();
                    IsPlaying = _status == Status.Playing;
                }
            }
        }

        public override float CurrentVolume
        {
            get { return _currentVolume; }

            set
            {
                if (value < MIN_VOLUME_PERCENT)
                {
                    value = MIN_VOLUME_PERCENT;                    
                }
                else if (value > MAX_VOLUME_PERCENT)
                {
                    value = MAX_VOLUME_PERCENT;
                }

                if (_currentVolume != value)
                {
                    _currentVolume = value;
                    NotifyPropertyChanged();
                    AdjustPlayerVolume();
                }
            }
        }

        public override TimeSpan CurrentSongElapsedTime
        {
            get { return _currentSongElapsedTime; }

            set
            {
                if (_currentSongElapsedTime != value)
                {
                    _currentSongElapsedTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override bool IsPlaying
        {
            get { return _isPlaying; }

            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override bool IsMuted
        {
            get { return _isMuted; }

            set
            {
                _isMuted = value;
                NotifyPropertyChanged();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _settingsManager = new SettingsManager();
            
            var currentSettings = _settingsManager.GetSettings();

            CurrentVolume = currentSettings.LastVolume;
        }

        public override void Exit()
        {
            base.Exit();

            _settingsManager.UpdateSettings(new MusicPlayerSettings()
            {
                LastVolume = CurrentVolume
            });
        }

        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
