using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO
{
    public class UserDetailsDto
    {
        public int Id { get; set; } = 0!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
