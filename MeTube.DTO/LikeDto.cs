using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO
{
    public class LikeDto
    {
        public int VideoID { get; set; }
        public int UserID { get; set; }

        public string? VideoTitle { get; set; }
        public string? UserName { get; set; }
    }
}
