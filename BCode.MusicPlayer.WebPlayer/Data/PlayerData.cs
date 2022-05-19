namespace BCode.MusicPlayer.WebPlayer.Data
{
    public class PlayerData
    {
        public bool IsInitialized { get; set; }
        public CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();
        public string LastMessage { get; set; } = string.Empty;
    }
}
