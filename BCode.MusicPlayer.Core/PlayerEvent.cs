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
        public Category EventCategory { get; }
        public DateTimeOffset TimeStampUtc { get; }
        public DateTime TimeStampLocal { get; }

        public PlayerEvent(string message, Type type = Type.Information, Category category = Category.PlayerState)
        {
            Message = message;
            EventType = type;
            EventCategory = category;
            TimeStampUtc = DateTimeOffset.UtcNow;
            TimeStampLocal = TimeStampUtc.LocalDateTime;
        }

        public enum Category
        {
            PlayerState,
            TrackTimeUpdate,
            TrackUpdate,
            PlayListUpdate
        }

        public enum Type
        {
            Information,
            Error
        }
    }
}
