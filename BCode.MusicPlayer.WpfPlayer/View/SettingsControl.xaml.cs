using BCode.MusicPlayer.WpfPlayer.Shared;
using BCode.MusicPlayer.WpfPlayer.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BCode.MusicPlayer.WpfPlayer.View
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private SettingsControlViewModel _viewModel;
        private bool _firstLoad;

        public event EventHandler SettingsClosedEvent;

        public SettingsControl()
        {
            InitializeComponent();

            this.DataContextChanged += SettingsControl_DataContextChanged;
        }

        private void SettingsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as SettingsControlViewModel;

            if (_viewModel == null)
            {
                throw new InvalidOperationException(
                    "SettingsControl must have a SettingsControlViewModel as its DataContext.");
            }
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Close();
            SettingsClosedEvent?.Invoke(this, EventArgs.Empty);
            _firstLoad = false;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
        }

        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;

            var dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*";
            dlg.Title = "Select background image";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                _viewModel.CustomBackgroundImagePath = dlg.FileName;
            }
        }
    }
}
