namespace MeTube.Client.Models
{
    public class Like
    {
        public int VideoID { get; set; }
        public int UserID { get; set; }
        public string? VideoTitle { get; set; }
        public string? UserName { get; set; }
    }
}
