using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.WpfPlayer.View;
using DynamicData;
using log4net.Core;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
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

        public FileExplorer()
        {
            _logger = ApplicationLoggerFactory.CreateLogger<FileExplorer>();

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

        public void GoBackUpDirectory()
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

                UpdateWorkingDirectory(newDir);

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
            lock (_imgLock)
            {
                BackgroundImage = _nextImage;
            }
        }

        public void GoToDirectory(DirectoryInfo newDirectory)
        {
            try
            {
                _lastSelectedFolderPath = SelectedItem.DirectoryDetail;

                UpdateWorkingDirectory(newDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error going to folder level");
            }
        }

        private void UpdateWorkingDirectory(DirectoryInfo directory)
        {
            if (directory is null)
                return;

            try
            {
                CurrentPath = directory;

                CurrentContent.Clear();

                var dirs = directory.GetDirectories().Where(d => (d.Attributes & FileAttributes.Hidden) == 0).Select(d => new BrowseItem(d)).OrderBy(d => d.Name).ToArray();
                CurrentContent.AddRange(dirs);

                var allFiles = directory.GetFiles().ToArray();

                var audioFiles = allFiles
                    .Where(f => Constants.AudioFileExtensions.Contains(f.Extension, StringComparer.CurrentCultureIgnoreCase))
                    .Select(f => new BrowseItem(f))
                    .OrderBy(f => f.Name);

                CurrentContent.AddRange(audioFiles);

                SelectedItem = CurrentContent.FirstOrDefault();

                _isAtTopLevel = false;

                Task.Run(() => CheckForFolderImage(allFiles));

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
