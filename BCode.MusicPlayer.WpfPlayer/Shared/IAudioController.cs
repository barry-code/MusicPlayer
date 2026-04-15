using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public interface IAudioController
    {
        public event Action<float> VolumeChanged;
        void Initialize();
        void SetVolume(float volumePercent);
        float GetVolume();
    }
}
