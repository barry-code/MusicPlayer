using NAudio.CoreAudioApi;
using NAudio.Wasapi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public class NAudioController : IAudioController, IDisposable
    {
        private SimpleAudioVolume _volume;
        private AudioSessionControl _session;

        public event Action<float> VolumeChanged;

        public NAudioController()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            var sessionManager = device.AudioSessionManager;
            var sessions = sessionManager.Sessions;

            // pick the first active session (your app must have audio playing)
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                if (session.State == AudioSessionState.AudioSessionStateActive)
                {
                    _session = session;
                    _volume = session.SimpleAudioVolume;

                    // Subscribe to volume change events
                    //_session.
                    break;
                }
            }
        }

        public void Initialize()
        {
            
        }

        public float GetVolume()
        {
            
        }

        public void SetVolume(float volumePercent)
        {
            
        }

        private void EndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
