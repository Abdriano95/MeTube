using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Entity
{
    public class History
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Video")]
        public int VideoId { get; set; }
        public DateTime DateWatched { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Video? Video { get; set; }
    }
}
