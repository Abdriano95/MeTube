using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Entity
{
    public class Comment
    {
        [Key]
        public required int Id { get; set; }
        [ForeignKey("Video")]
        public required int VideoId { get; set; }
        [ForeignKey("User")]
        public required int UserId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Content must be between 3 and 255 characters.")]
        public required string Content { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Video? Video { get; set; }
        public User? User { get; set; }
    }
}
