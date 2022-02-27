using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public static class Log4NetLoggerFactory
    {
        public static void ConfigureLog4NetLogging(this ContainerBuilder expr)
        {
            LoggerFactory factory = new LoggerFactory();
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            factory.AddLog4Net(Path.Combine(dir, "log4net.config"));

            ApplicationLoggerFactory.ConfigureLoggerFactory(factory);
        }
    }
}
