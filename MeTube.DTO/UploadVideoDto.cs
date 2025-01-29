using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO
{
    public class UploadVideoDto
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Genre { get; set; }

        [Required]
        public string VideoUrl { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
