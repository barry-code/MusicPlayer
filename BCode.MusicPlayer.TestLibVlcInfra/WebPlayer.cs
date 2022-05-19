﻿using BCode.MusicPlayer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Infrastructure
{
    public class WebPlayer : LibVlcPlayer
    {
        public override IList<ISong> PlayList { get; set; } = new List<ISong>();
    }
}