using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCode.MusicPlayer.Core
{
    public class PlayerEvent : EventArgs
    {
        public string Message { get; }
        public Type EventType { get; }
        public DateTimeOffset TimeStamp { get; }
        public TimeOnly LocalTime { get; }

        public PlayerEvent(string message, Type type = Type.Information)
        {
            Message = message;
            EventType = type;
            TimeStamp = DateTimeOffset.UtcNow;
            LocalTime = TimeOnly.FromDateTime(TimeStamp.LocalDateTime);
        }

        public enum Type
        {
            Information,
            Error
        }
    }
}
