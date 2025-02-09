using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO.HistoryDTOs
{
    public class HistoryUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VideoId { get; set; }

        public DateTime DateWatched { get; set; } = DateTime.UtcNow;
    }
}
