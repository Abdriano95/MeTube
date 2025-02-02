using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO.VideoDTOs
{
    public class UpdateVideoDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(30, MinimumLength = 3)]
        public string? Title { get; set; }

        [StringLength(255, MinimumLength = 3)]
        public string? Description { get; set; }

        [StringLength(30, MinimumLength = 3)]
        public string? Genre { get; set; }

        public string? VideoUrl { get; set; }

        [Required]
        public int UserId { get; set; }
    }

}
