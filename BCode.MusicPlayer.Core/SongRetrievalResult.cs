using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Core
{
    public class SongRetrievalResult
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public IList<ISong> Songs { get; set; }
    }
}
