using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Entity
{
    public class User
    {
        [Key]
        public required int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 20 characters.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Role is Required")]
        public required string Role { get; set; } = "User";

        public ICollection<Video> Videos { get; set; } = new List<Video>();
    }
}
