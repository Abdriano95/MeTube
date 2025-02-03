using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.DTO.VideoDTOs
{
    public class BlobResponseDto
    {
        public BlobResponseDto()
        {
            Blob = new BlobDto();
        }

        public string? Status { get; set; }

        public bool Error { get; set; }

        public BlobDto? Blob { get; set; }
    }
}
