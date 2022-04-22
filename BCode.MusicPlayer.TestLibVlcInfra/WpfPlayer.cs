using BCode.MusicPlayer.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.TestLibVlcInfra
{
    public class WpfPlayer : LibVlcPlayer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override IList<ISong> PlayList { get; set; } = new ObservableCollection<ISong>();

        public override ISong CurrentSong
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
                if (value < 0)
                {
                    value = 0;                    
                }

                if (value > 100)
                {
                    value = 100;
                }

                if (_currentVolume != value)
                {
                    _currentVolume = value;
                    NotifyPropertyChanged();
                    AdjustPlayerVolume();
                }
            }
        }

        public override TimeSpan CurrentElapsedTime
        {
            get { return _currentElapsedTime; }

            set
            {
                if (_currentElapsedTime != value)
                {
                    _currentElapsedTime = value;
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
                    if (_isPlaying)
                    {
                        PublishEvent($"Now playing [{CurrentSong.Name}]");
                    }
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

        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
