using System.ComponentModel.DataAnnotations;

namespace MeTube.DTO
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; }

        [StringLength(20, MinimumLength = 3)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Role { get; set; }
    }
}