using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Entity
{
    public class Like
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Video")]
        public int VideoID { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("User")]
        public int UserID { get; set; }

        // Navigation properties
        public Video? Video { get; set; }
        public User? User { get; set; }
    }
}
