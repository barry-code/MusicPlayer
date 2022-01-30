using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.Infrastructure;
using BCode.MusicPlayer.WpfPlayer.View;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using log4net;
using System.Text;
using BCode.MusicPlayer.WpfPlayer.Shared;

namespace BCode.MusicPlayer.WpfPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IContainer _container { get; set; }
        private ILogger _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ContainerBuilder();

            builder.ConfigureLog4NetLogging();
            builder.RegisterType<MainWindow>().As<IMainWindow>().SingleInstance();
            builder.RegisterType<NAudioWpfPlayer>().As<IPlayer>().SingleInstance();

            _container = builder.Build();

            _logger = ApplicationLoggerFactory.CreateLogger("App");

            _logger.LogInformation("App starting");

            this.Exit += OnExit;

            MainWindow = (Window)_container.Resolve<IMainWindow>();
            MainWindow.Show();
            _logger.LogInformation("App opened main window");
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _logger.LogInformation("App exiting");
            _container.Dispose();
        }
    }
}
