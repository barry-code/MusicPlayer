using System;

namespace BCode.MusicPlayer.WpfPlayer.Shared;

public interface ISettingsManager
{
    event EventHandler SettingsChanged;

    public MusicPlayerSettings CurrentSettings { get; }

    void UpdateSettings(MusicPlayerSettings settings);
}
