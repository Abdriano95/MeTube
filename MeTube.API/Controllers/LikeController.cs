using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeTube.API.Controllers
{
    /// <summary>
    /// Controller for managing likes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikeController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for data access.</param>
        /// <param name="mapper">The AutoMapper instance for mapping between entities and DTOs.</param>
        public LikeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all likes.
        /// </summary>
        /// <returns>An IActionResult containing a collection of <see cref="LikeDto"/> objects.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllLikes()
        {
            var likes = await _unitOfWork.Likes.GetAllLikesAsync();
            if (!likes.Any())
                return NotFound(new { Message = "Likes not found" });
            var likeDtos = _mapper.Map<IEnumerable<LikeDto>>(likes);
            return Ok(likeDtos);
        }

        /// <summary>
        /// Retrieves likes for a specific video for management purposes (Admin only).
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>An IActionResult containing a collection of <see cref="LikeDto"/> objects.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("manage/{videoId}")]
        public async Task<IActionResult> GetLikesForVideoManagement(int videoId)
        {
            var likes = await _unitOfWork.Likes.GetLikesForVideoAsync(videoId);
            var likeDtos = _mapper.Map<IEnumerable<LikeDto>>(likes);
            return Ok(likeDtos);
        }

        /// <summary>
        /// Retrieves the like status for a specific video for the authenticated user.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>
        /// An IActionResult containing an anonymous object indicating whether the user has liked the video, and if so, the like details.
        /// </returns>
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

        /// <summary>
        /// Retrieves likes for a specific video.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>
        /// An IActionResult containing a <see cref="LikesForVideoResponseDto"/> with the like count and like details.
        /// </returns>
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

        /// <summary>
        /// Adds a like for a video.
        /// </summary>
        /// <param name="likeDto">The like data transfer object.</param>
        /// <returns>An IActionResult with the created like details.</returns>
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

        /// <summary>
        /// Removes a like for a video by the authenticated user.
        /// </summary>
        /// <param name="likeDto">The like data transfer object.</param>
        /// <returns>An IActionResult indicating the result of the remove operation.</returns>
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

        /// <summary>
        /// Removes a like for a video as an admin.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <param name="userId">The ID of the user whose like is to be removed.</param>
        /// <returns>An IActionResult indicating the result of the remove operation.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{videoId}/{userId}")]
        public async Task<IActionResult> RemoveLikeAsAdmin(int videoId, int userId)
        {
            var like = await _unitOfWork.Likes.GetLikeAsync(videoId, userId);
            if (like == null)
                return NotFound("Like not found");
            await _unitOfWork.Likes.RemoveLikeAsync(like);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}
