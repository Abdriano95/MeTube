using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO
{
    public class HistoryDto
    {
        public int VideoId { get; set; }
        public string VideoTitle { get; set; } = string.Empty;
        public DateTime DateWatched { get; set; }
    }
}
