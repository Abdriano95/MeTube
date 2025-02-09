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
    public class CommentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetCommentsByVideo(int videoId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByVideoIdAsync(videoId);
            if (!comments.Any())
                return NotFound(new { Message = "No comments found for this video." });

            return Ok(_mapper.Map<IEnumerable<CommentDto>>(comments));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUser(int userId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByUserIdAsync(userId);
            if (!comments.Any())
                return NotFound(new { Message = "No comments found for this user." });

            return Ok(_mapper.Map<IEnumerable<CommentDto>>(comments));
        }

        // GET: api/comments/username/{userId}
        [HttpGet("username/{userId}")]
        public async Task<IActionResult> GetPosterUsername(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found." });
            return Ok(user.Username );
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";

            var video = await _unitOfWork.Videos.GetByIdAsync(commentDto.VideoId);
            if (video == null)
                return BadRequest(new { Message = "Invalid VideoId" });

            var comment = new Comment
            {
                VideoId = commentDto.VideoId,
                UserId = userId,
                Content = commentDto.Content,
                DateAdded = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddCommentAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment added successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _unitOfWork.Comments.GetCommentByIdAsync(id);
            if (comment == null)
                return NotFound(new { Message = "Comment not found." });

            _unitOfWork.Comments.DeleteComment(comment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment deleted successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingComment = await _unitOfWork.Comments.GetCommentByIdAsync(id);
            if (existingComment == null)
                return NotFound(new { Message = "Comment not found." });

            _mapper.Map(updateCommentDto, existingComment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment updated successfully." });
        }
    }
}
