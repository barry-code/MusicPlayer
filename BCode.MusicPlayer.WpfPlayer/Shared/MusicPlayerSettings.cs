using System;
using System.Collections.Generic;

namespace BCode.MusicPlayer.WpfPlayer.Shared;
public class MusicPlayerSettings : IEquatable<MusicPlayerSettings>
{
    public float LastVolume { get; set; } = 30;
    public bool UseCustomBackgroundImage { get; set; } = false;
    public string CustomBackgroundImagePath { get; set; } = string.Empty;

    public bool Equals(MusicPlayerSettings other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return LastVolume.Equals(other.LastVolume)
            && UseCustomBackgroundImage == other.UseCustomBackgroundImage
            && string.Equals(
                CustomBackgroundImagePath,
                other.CustomBackgroundImagePath,
                StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
        => Equals(obj as MusicPlayerSettings);

    public override int GetHashCode()
        => HashCode.Combine(
            LastVolume,
            UseCustomBackgroundImage,
            CustomBackgroundImagePath);
}
