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
        //public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            /* FAILING DUE TO NOT RECOG ILOGGER IN MAINWINDOW */
            //IHost host = new HostBuilder()
            //    .ConfigureServices((hostContext, services) =>
            //    {
            //        services.AddSingleton<MainWindow>();
            //        services.AddSingleton<IPlayer, NAudioWpfPlayer>();
            //        services.AddScoped<ISong, Song>();
            //    })
            //    .ConfigureLogging(logBuilder =>
            //    {
            //        logBuilder.SetMinimumLevel(LogLevel.Trace);
            //        logBuilder.AddLog4Net("log4net.config");
            //    })
            //    .Build();

            //using (var scope = host.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;

            //    try
            //    {
            //        var mainWindow = services.GetRequiredService<MainWindow>();
            //        mainWindow.Show();

            //        Console.WriteLine("WORKING");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("ERROR IN STARTUP: " + ex.Message);
            //        throw ex;
            //    }
            //}

            /* AUTOFAC VERSION */
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
