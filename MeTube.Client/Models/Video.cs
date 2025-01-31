namespace MeTube.Client.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime DateUploaded { get; set; }
        public string UserId { get; set; }
    }
}
