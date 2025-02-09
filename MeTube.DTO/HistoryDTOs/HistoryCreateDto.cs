using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO.HistoryDTOs
{
    public class HistoryCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VideoId { get; set; }

        public DateTime DateWatched { get; set; }
    }
}
