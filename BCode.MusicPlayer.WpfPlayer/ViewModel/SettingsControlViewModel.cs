using BCode.MusicPlayer.WpfPlayer.Shared;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.WpfPlayer.ViewModel
{
    public class SettingsControlViewModel : ReactiveObject
    {
        public ISettingsManager SettingsManager { get; set; }

        public SettingsControlViewModel(ISettingsManager settingsManager)
        {
            SettingsManager = settingsManager;
            Init();
        }

        private bool _useCustomBackgroundImage;
        public bool UseCustomBackgroundImage { get => _useCustomBackgroundImage; set => this.RaiseAndSetIfChanged(ref _useCustomBackgroundImage, value);  }

        private string _custombackgroundImagePath;
        public string CustomBackgroundImagePath { get => _custombackgroundImagePath; set => this.RaiseAndSetIfChanged(ref _custombackgroundImagePath, value); }

        private bool _isSettingsOpen;
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set => this.RaiseAndSetIfChanged(ref _isSettingsOpen, value);
        }

        public void Save()
        {
            if (SettingsManager is not null)
            {
                var current = SettingsManager.CurrentSettings;
                current.UseCustomBackgroundImage = UseCustomBackgroundImage;
                current.CustomBackgroundImagePath = CustomBackgroundImagePath;
                SettingsManager.UpdateSettings(current);
            }
        }

        public void Close()
        {
            Save();
        }

        private void Init()
        {
            if (SettingsManager is not null)
            {
                var current = SettingsManager.CurrentSettings;
                UseCustomBackgroundImage = current.UseCustomBackgroundImage;
                CustomBackgroundImagePath = current.CustomBackgroundImagePath;
            }
        }
    }
}
