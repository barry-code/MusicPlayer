using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BCode.MusicPlayer.WpfPlayer.Shared
{
    public class InvertedBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public InvertedBooleanToVisibilityConverter() : base(Visibility.Collapsed, Visibility.Visible)
        {
        }
    }
}
