using BCode.MusicPlayer.Core;
using BCode.MusicPlayer.TestLibVlcInfra;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.WpfPlayerTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPlayer, BCode.MusicPlayer.TestLibVlcInfra.WpfPlayer>();
        }
    }
}
