﻿using BCode.MusicPlayer.Core;
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
using System.Windows.Documents;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public class FileExplorer : ReactiveObject
    {
        private ILogger<FileExplorer> _logger;

        public FileExplorer()
        {
            _logger = ApplicationLoggerFactory.CreateLogger<FileExplorer>();

            GetDrives();
        }

        private DirectoryInfo _currentPath;
        public DirectoryInfo CurrentPath { get => _currentPath; set => this.RaiseAndSetIfChanged(ref _currentPath, value); }

        private ObservableCollection<BrowseItem> _currentContent = new ObservableCollection<BrowseItem>();
        public ObservableCollection<BrowseItem> CurrentContent { get => _currentContent; set => this.RaiseAndSetIfChanged(ref _currentContent, value); } 

        public void GoToTopDirectoryLevel()
        {
            try
            {
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
                var newDir = CurrentPath.Parent;

                UpdateWorkingDirectory(newDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error going up folder level");
            }
        }

        public void GoToDirectory(DirectoryInfo newDirectory)
        {
            try
            {
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

                var dirs = directory.GetDirectories().Where(d => (d.Attributes & FileAttributes.Hidden) == 0).Select(d => new BrowseItem(d)).ToArray();
                CurrentContent.AddRange(dirs);

                var files = directory.GetFiles().Where(f => Constants.AudioFileExtensions.Contains(f.Extension)).Select(f => new BrowseItem(f)).ToArray();
                CurrentContent.AddRange(files);

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error getting drives");
            }
        }

        public class BrowseItem
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
        }
    }
}
