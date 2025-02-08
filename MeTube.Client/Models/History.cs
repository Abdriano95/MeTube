namespace MeTube.Client.Models
{
    public class History
    {
        public int VideoId { get; set; }
        public string VideoTitle { get; set; } = string.Empty;
        public DateTime DateWatched { get; set; }
    }
}
