using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public LikeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/Like
        [HttpGet]
        public async Task<IActionResult> GetAllLikes()
        {
            var likes = await _unitOfWork.Likes.GetAllLikesAsync();
            if (!likes.Any())
                return NotFound(new { Message = "Likes not found" });
            var likeDtos = _mapper.Map<IEnumerable<LikeDto>>(likes);
            return Ok(likeDtos);
        }

        // GET: api/Like/manage/{videoId}
        [Authorize(Roles = "Admin")]
        [HttpGet("manage/{videoId}")]
        public async Task<IActionResult> GetLikesForVideoManagement(int videoId)
        {
            var likes = await _unitOfWork.Likes.GetLikesForVideoAsync(videoId);
            var likeDtos = _mapper.Map<IEnumerable<LikeDto>>(likes);
            return Ok(likeDtos);
        }

        // POST: api/Like/{videoId}
        [Authorize]
        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetLike(int videoId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var likes = await _unitOfWork.Likes.GetAllLikesAsync();
            var like = likes.FirstOrDefault(l => l.VideoID == videoId && l.UserID == userId);

            if (like == null)
                return Ok(new { hasLiked = false });

            return Ok(new { hasLiked = true, like = _mapper.Map<LikeDto>(like) });
        }

        // GET: api/Like/video/{videoId}
        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetLikesForVideo(int videoId)
        {
            var likes = await _unitOfWork.Likes.GetAllLikesAsync();
            var videoLikes = likes.Where(l => l.VideoID == videoId).ToList();

            var response = new LikesForVideoResponseDto
            {
                Count = videoLikes.Count,
                Likes = _mapper.Map<IEnumerable<LikeDto>>(videoLikes)
            };

            return Ok(response);
        }


        // POST: api/Like
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddLike([FromBody] LikeDto likeDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var video = await _unitOfWork.Videos.GetVideoByIdAsync(likeDto.VideoID);
            if (video == null)
                return NotFound("Video not found");

            try
            {
                var like = _mapper.Map<Like>(likeDto);
                like.UserID = userId;

                await _unitOfWork.Likes.AddLikeAsync(like);
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLike), new { videoId = like.VideoID }, _mapper.Map<LikeDto>(like));

            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }           
        }

        // DELETE: api/Like/
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> RemoveLike([FromBody] LikeDto likeDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            try
            {
                var like = _mapper.Map<Like>(likeDto);
                like.UserID = userId;

                await _unitOfWork.Likes.RemoveLikeAsync(like);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE: api/Like/video/{videoId}
        [Authorize(Roles = "Admin")]
        [HttpDelete("video/{videoId}")]
        public async Task<IActionResult> RemoveLikesForVideo(int videoId)
        {
            try
            {
                await _unitOfWork.Likes.RemoveLikesForVideoAsync(videoId);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
