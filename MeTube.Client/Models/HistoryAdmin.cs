namespace MeTube.Client.Models
{
    public class HistoryAdmin
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VideoId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string VideoTitle { get; set; } = string.Empty;
        public DateTime DateWatched { get; set; }

        public User? User { get; set; }
        public Video? Video { get; set; }
    }
}
