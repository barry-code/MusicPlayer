using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.Infrastructure;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public class FileExplorer : ReactiveObject
    {
        private ILogger<FileExplorer> _logger;
        private bool _isAtTopLevel;
        private DirectoryInfo _lastSelectedFolderPath;
        private readonly object _imgLock = new object();
        private BitmapImage _nextImage;
        private ILibraryManager _libraryManager;

        public FileExplorer()
        {
            _logger = ApplicationLoggerFactory.CreateLogger<FileExplorer>();

            _libraryManager = new LibraryManager();

            GetDrives();
        }

        private DirectoryInfo _currentPath;
        public DirectoryInfo CurrentPath { get => _currentPath; set => this.RaiseAndSetIfChanged(ref _currentPath, value); }

        private ObservableCollection<BrowseItem> _currentContent = new ObservableCollection<BrowseItem>();
        public ObservableCollection<BrowseItem> CurrentContent { get => _currentContent; set => this.RaiseAndSetIfChanged(ref _currentContent, value); }

        private BrowseItem _selectedItem;
        public BrowseItem SelectedItem { get => _selectedItem; set => this.RaiseAndSetIfChanged(ref _selectedItem, value); }

        private ImageSource _backgroundImage;
        public ImageSource BackgroundImage { get => _backgroundImage; set => this.RaiseAndSetIfChanged(ref _backgroundImage, value); }

        public void GoToTopDirectoryLevel()
        {
            try
            {
                _lastSelectedFolderPath = null;
                CurrentPath = null;

                if (_isAtTopLevel)
                    return;

                GetDrives();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error going to home level");
            }
        }

        public async Task GoBackUpDirectory()
        {
            try
            {
                if (CurrentPath is null)
                    return;

                var newDir = CurrentPath.Parent;

                if (newDir is null)
                {
                    GoToTopDirectoryLevel();
                    return;
                }

                await UpdateWorkingDirectory(newDir);

                if (_lastSelectedFolderPath is null)
                {
                    return;
                }

                var lastSelected = CurrentContent.FirstOrDefault(c => c.IsDirectory && c.DirectoryDetail.FullName == _lastSelectedFolderPath.FullName);

                if (lastSelected is not null)
                {
                    SelectedItem = lastSelected;
                }

                _lastSelectedFolderPath = SelectedItem.DirectoryDetail.Parent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error going up folder level");
            }
        }

        public void UseCurrentFolderImage()
        {
            //TODO: move all functionality relating to background image, into central location so can be used by both browse and also playlist screen.
            lock (_imgLock)
            {
                BackgroundImage = _nextImage;
            }
        }

        public async Task GoToDirectory(DirectoryInfo newDirectory)
        {
            try
            {
                _lastSelectedFolderPath = SelectedItem.DirectoryDetail;

                await UpdateWorkingDirectory(newDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error going to folder level");
            }
        }

        private async Task UpdateWorkingDirectory(DirectoryInfo directory)
        {
            if (directory is null)
                return;

            try
            {
                CurrentPath = directory;

                CurrentContent.Clear();
                List<BrowseItem> browseItems = new();

                var dirs = directory.GetDirectories().Where(d => (d.Attributes & FileAttributes.Hidden) == 0).Select(d => new BrowseItem(d)).OrderBy(d => d.Name).ToList();
                browseItems.AddRange(dirs);

                var allFiles = directory.GetFiles().ToArray();

                var songResults = await _libraryManager.GetAllSongs(directory.FullName, CancellationToken.None, SearchOption.TopDirectoryOnly);

                browseItems.AddRange(songResults.Songs.Select(s => new BrowseItem(s)));

                CurrentContent.AddRange(browseItems);

                SelectedItem = CurrentContent.FirstOrDefault();

                _isAtTopLevel = false;

                _ = Task.Run(() => CheckForFolderImage(allFiles));

                this.RaisePropertyChanged(nameof(CurrentContent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error updating directory");
            }
        }

        private void GetDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                CurrentContent.Clear();
                CurrentContent.AddRange(drives.Select(d => new BrowseItem(d.RootDirectory)).ToArray());
                _isAtTopLevel = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error getting drives");
            }
        }
    
        private void CheckForFolderImage(FileInfo[] files)
        {
            try
            {
                HashSet<string> imgExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".png", "bmp"
                };

                var allImages = files.Where(f => imgExtensions.Contains(f.Extension)).ToList();
                var frontCoverImage = allImages.FirstOrDefault(i => i.Name.Contains("front"));
                var chosenImage = frontCoverImage != null ? frontCoverImage : allImages.OrderByDescending(f => f.Length).FirstOrDefault();

                if (chosenImage is not null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(chosenImage.FullName, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    lock (_imgLock)
                    {
                        _nextImage = bitmap;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
