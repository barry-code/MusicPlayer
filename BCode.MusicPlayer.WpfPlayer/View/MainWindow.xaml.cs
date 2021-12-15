using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.WpfPlayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BCode.MusicPlayer.Infrastructure;
using Microsoft.Extensions.Logging;
using BCode.MusicPlayer.WpfPlayer.Shared;

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
    }
}
