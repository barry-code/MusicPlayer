using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public static class ApplicationLoggerFactory
    {
        private static ILoggerFactory _loggerFactory;

        public static ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger(string name)
        {
            return _loggerFactory.CreateLogger(name);
        }

        public static void ConfigureLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
    }
}
