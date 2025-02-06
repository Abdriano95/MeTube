namespace MeTube.Client.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int VideoId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
    }
}
