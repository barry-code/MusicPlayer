using System;
using System.Collections.Generic;
using System.Text;

namespace BCode.MusicPlayer.Core
{
    public class Genre
    {
        public Genre()
        {
            Name = String.Empty;
        }

        public string Name { get; set; }        

        public static Genre Rock { get; } = new Genre { Name = "Rock" };
        public static Genre Pop { get; } = new Genre { Name = "Pop" };
        public static Genre Jazz { get; } = new Genre { Name = "Jazz" };
        public static Genre Dance { get; } = new Genre { Name = "Dance" };
        public static Genre Classical { get; } = new Genre { Name = "Classical" };
        public static Genre Metal { get; } = new Genre { Name = "Metal" };
    }
}
