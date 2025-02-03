using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Entity
{
    public class Video
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 30 characters.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 255 characters.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Genre must be between 3 and 30 characters.")]
        public required string Genre { get; set; }
        public string? VideoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? BlobName { get; set; }
        public DateTime DateUploaded { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<History> Histories { get; set; } = new List<History>();
    }
}
