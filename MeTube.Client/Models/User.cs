namespace MeTube.Client.Models
{
    public class User
    {
        public int UserId { get; set; } = 0!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
