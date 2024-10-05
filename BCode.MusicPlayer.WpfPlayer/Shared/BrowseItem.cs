using ReactiveUI;
using System.IO;

namespace BCode.MusicPlayer.WpfPlayer.Shared;
public class BrowseItem : ReactiveObject
{
    public BrowseItem(DirectoryInfo di)
    {
        Name = di.Name;
        DirectoryDetail = di;
        IsDirectory = true;
        IconType = "FolderOutline";
    }

    public BrowseItem(FileInfo fi)
    {
        Name = fi.Name;
        FileDetail = fi;
        IconType = "File";
    }

    public string Name { get; private set; }
    public DirectoryInfo DirectoryDetail { get; private set; }
    public FileInfo FileDetail { get; private set; }
    public bool IsDirectory { get; private set; }
    public string IconType { get; private set; }

    private string _duration = string.Empty;
    public string Duration { get => _duration; set => this.RaiseAndSetIfChanged(ref _duration, value); }

    private string _artist = string.Empty;
    public string Artist { get => _artist; set => this.RaiseAndSetIfChanged(ref _artist, value); }
}
