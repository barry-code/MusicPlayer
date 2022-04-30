using BCode.MusicPlayer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.WpfPlayerTests
{
    public class PlayerFixture : IDisposable
    {
        public PlayerFixture(IPlayer player)
        {
            Player = player;
        }

        public IPlayer Player { get; private set; }

        public void Dispose()
        {
            Player?.Dispose();
        }
    }
}
