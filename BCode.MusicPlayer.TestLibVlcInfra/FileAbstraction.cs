namespace BCode.MusicPlayer.TestLibVlcInfra
{
    public class FileAbstraction : TagLib.File.IFileAbstraction
    {
        public FileAbstraction(string file)
        {
            Name = file;
        }

        public string Name { get; }

        public Stream ReadStream => new FileStream(Name, FileMode.Open);

        public Stream WriteStream => new FileStream(Name, FileMode.Open);

        public void CloseStream(Stream stream)
        {
            stream.Close();
        }
    }
}
