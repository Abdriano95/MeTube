using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO.VideoDTOs
{
    public class DeleteVideoDto
    {
        [Required]
        public int Id { get; set; }
    }
}
