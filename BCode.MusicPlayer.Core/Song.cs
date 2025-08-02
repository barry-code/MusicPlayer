namespace BCode.MusicPlayer.Core
{
    public class Song
    {
        private readonly int MAX_CHAR_LENGTH_BEFORE_TRUNCATE = 50;

        public Song()
        {
            SongId = Guid.NewGuid();
        }

        public Guid SongId { get; }

        private string _name = string.Empty;
        public string Name
        {
            get { return Truncate(_name) ?? "Unknown"; }
            set { _name = value; }
        }

        public string DirectoryPath { get; set; } = string.Empty;

        public int AlbumId { get; set; }

        public string Path { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long Size { get; set; }

        private string _artistName = string.Empty;
        public string ArtistName
        {
            get { return Truncate(_artistName) ?? "Unknown"; }
            set { _artistName = value; }
        }

        private string _albumName = string.Empty;
        public string AlbumName
        {
            get { return Truncate(_albumName) ?? "Unknown"; }
            set { _albumName = value; }
        }

        public int Order { get; set; }

        private string _year = string.Empty;
        public string Year
        {
            get { return Truncate(_year) ?? "Unknown"; }
            set { _year = value; }
        }

        public uint TrackNumer { get; set; }

        public TimeSpan Duration { get; set; }

        public IList<Genre> Genres { get; set; } = new List<Genre>();

        private string Truncate(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            if (s.Length < MAX_CHAR_LENGTH_BEFORE_TRUNCATE)
            {
                return s;
            }

            return $"{s.Substring(0, MAX_CHAR_LENGTH_BEFORE_TRUNCATE - 3).TrimEnd()}...";
        }
    }
}
