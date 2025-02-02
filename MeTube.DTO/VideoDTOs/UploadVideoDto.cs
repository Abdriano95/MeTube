using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MeTube.DTO.VideoDTOs
{
    public class UploadVideoDto
    {
        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Genre { get; set; }

        public string VideoUrl { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile VideoFile { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? ThumbnailFile { get; set; }

    }
}
