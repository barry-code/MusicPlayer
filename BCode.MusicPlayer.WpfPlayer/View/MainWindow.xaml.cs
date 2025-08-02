using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.WpfPlayer.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using BCode.MusicPlayer.WpfPlayer.Shared;
using static BCode.MusicPlayer.WpfPlayer.Shared.FileExplorer;
using System.Windows.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.WpfPlayer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private MainWindowViewModel _viewModel;
        private ILogger _logger;

        public MainWindow(IPlayer player)
        {
            InitializeComponent();

            _logger = ApplicationLoggerFactory.CreateLogger<MainWindow>();

            _viewModel = new MainWindowViewModel(player, _logger);

            DataContext = _viewModel;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                _viewModel.ExpandedMode();
                this.WindowState = WindowState.Maximized;                
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("Close button clicked");
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing app");
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMinimalPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }

            _viewModel.MinimalMode();
        }

        private void btnExpandedPlayer_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExpandedMode();
        }

        private void snackBarMessages_IsActiveChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            try
            {
                bool isCurrentNotificationEnded = !e.NewValue;
                bool isLastNotification = _viewModel.NotificationsRemainingCount == 1;

                if (isCurrentNotificationEnded && isLastNotification)
                {
                    _viewModel.HideNotificationsPopup();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error working with notifications");
            }
            
        }

        private async void browseItemGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;

            if (grid is null)
                return;

            var item = grid.SelectedItem as BrowseItem;

            if (item is null)
                return;

            var dir = item.DirectoryDetail;

            if (dir is not null)
            {
                await _viewModel.FileExplorer.GoToDirectory(item.DirectoryDetail);

                if (_viewModel.FileExplorer?.SelectedItem is not null)
                {
                    browseItemGrid.ScrollIntoView(_viewModel.FileExplorer.SelectedItem);
                }

                return;
            }

            var file = item.Song;

            if (file is not null)
            {
                await _viewModel.Player.StartBrowseMode(file.Path, true);
                _viewModel.FileExplorer.UseCurrentFolderImage();
            }         
        }

        private void btnFolderBrowseGoHome(object sender, RoutedEventArgs e)
        {
            _viewModel.FileExplorer.GoToTopDirectoryLevel();
        }

        private async void btnFolderBrowseGoUpLevel(object sender, RoutedEventArgs e)
        {
            await _viewModel.FileExplorer.GoBackUpDirectory();

            if (_viewModel.FileExplorer?.SelectedItem is null)
                return;

            browseItemGrid.UpdateLayout();
            browseItemGrid.ScrollIntoView(_viewModel.FileExplorer.SelectedItem);
        }

        private async void AddBrowseItemToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = browseItemGrid;

            if (dataGrid.SelectedItem is BrowseItem browseItem)
            {
                await _viewModel.AddItemFromBrowseScreenToPlaylist(browseItem);
            }
        }
    }
}
   