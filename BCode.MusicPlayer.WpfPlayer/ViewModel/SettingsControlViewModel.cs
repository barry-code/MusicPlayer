using BCode.MusicPlayer.WpfPlayer.Shared;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace BCode.MusicPlayer.WpfPlayer.ViewModel
{
    public class SettingsControlViewModel : ReactiveObject
    {
        public ISettingsManager SettingsManager { get; set; }

        private MusicPlayerSettings _originalSettings;

        private bool _hasChanges;
        public bool HasChanges { get => _hasChanges; set => this.RaiseAndSetIfChanged(ref _hasChanges, value); }

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
                // update original snapshot and clear dirty flag
                _originalSettings = new MusicPlayerSettings
                {
                    LastVolume = current.LastVolume,
                    UseCustomBackgroundImage = current.UseCustomBackgroundImage,
                    CustomBackgroundImagePath = current.CustomBackgroundImagePath
                };
                HasChanges = false;
            }
        }

        public void Close()
        {
            // Revert any unsaved changes by restoring values from the original snapshot
            if (_originalSettings is not null)
            {
                UseCustomBackgroundImage = _originalSettings.UseCustomBackgroundImage;
                CustomBackgroundImagePath = _originalSettings.CustomBackgroundImagePath;
            }
            HasChanges = false;
        }

        private void Init()
        {
            if (SettingsManager is not null)
            {
                var current = SettingsManager.CurrentSettings;

                // keep an original snapshot to detect changes and to allow revert
                _originalSettings = new MusicPlayerSettings
                {
                    LastVolume = current.LastVolume,
                    UseCustomBackgroundImage = current.UseCustomBackgroundImage,
                    CustomBackgroundImagePath = current.CustomBackgroundImagePath
                };

                UseCustomBackgroundImage = _originalSettings.UseCustomBackgroundImage;
                CustomBackgroundImagePath = _originalSettings.CustomBackgroundImagePath;

                // subscribe to property changes to determine if we have unsaved changes
                this.WhenAnyValue(x => x.UseCustomBackgroundImage, x => x.CustomBackgroundImagePath)
                    .Subscribe(tuple =>
                    {
                        var (useCustom, path) = tuple;
                        var temp = new MusicPlayerSettings
                        {
                            UseCustomBackgroundImage = useCustom,
                            CustomBackgroundImagePath = path
                        };

                        HasChanges = !_originalSettings.Equals(temp);
                    });

                // listen for external settings changes and refresh snapshot
                SettingsManager.SettingsChanged += (s, e) =>
                {
                    var updated = SettingsManager.CurrentSettings;
                    _originalSettings = new MusicPlayerSettings
                    {
                        LastVolume = updated.LastVolume,
                        UseCustomBackgroundImage = updated.UseCustomBackgroundImage,
                        CustomBackgroundImagePath = updated.CustomBackgroundImagePath
                    };

                    UseCustomBackgroundImage = _originalSettings.UseCustomBackgroundImage;
                    CustomBackgroundImagePath = _originalSettings.CustomBackgroundImagePath;
                    HasChanges = false;
                };
            }
        }
    }
}
