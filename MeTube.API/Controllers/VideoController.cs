using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Mvc;

namespace MeTube.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VideoController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/Video
        [HttpGet]
        public async Task<IActionResult> GetAllVideos()
        {
            var videos = await _unitOfWork.Videos.GetAllVideosAsync();
            var videoDtos = _mapper.Map<IEnumerable<VideoDto>>(videos);
            return Ok(videoDtos);
        }

        // GET: api/Video/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound();

            var videoDto = _mapper.Map<VideoDto>(video);
            return Ok(videoDto);
        }

        // POST: api/Video
        [HttpPost]
        public async Task<IActionResult> UploadVideo([FromBody] UploadVideoDto uploadVideoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var video = _mapper.Map<Video>(uploadVideoDto);
            video.DateUploaded = DateTime.UtcNow;

            await _unitOfWork.Videos.AddVideoAsync(video);
            await _unitOfWork.SaveChangesAsync();

            var videoDto = _mapper.Map<VideoDto>(video);
            return CreatedAtAction(nameof(GetVideoById), new { id = video.Id }, videoDto);
        }

        // PUT: api/Video/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateVideo(int id, [FromBody] UpdateVideoDto updateVideoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound();

            _mapper.Map(updateVideoDto, video);

            _unitOfWork.Videos.UpdateVideo(video);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Video/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound();

            _unitOfWork.Videos.DeleteVideo(video);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
