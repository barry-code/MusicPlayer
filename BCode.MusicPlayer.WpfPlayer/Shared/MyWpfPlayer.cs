using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BCode.MusicPlayer.WpfPlayer.Shared;

public class MyWpfPlayer : LibVlcPlayer, INotifyPropertyChanged
{
    private readonly IAudioController _audioController;

    public MyWpfPlayer(IAudioController audioController)
    {
        _audioController = audioController;

        //TODO: need to check what i do here about listending to the audiocontroller volumne changes. Do i then set CurrentVolume, and maybe supress calling audiocontroller again (to avoid a loop) ???
    }

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
                //AdjustPlayerVolume();
                _audioController?.Initialize();
                _audioController?.SetVolume(_currentVolume);
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
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void NotifyPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
