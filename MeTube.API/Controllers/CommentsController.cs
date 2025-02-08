using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Mvc;

namespace MeTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // Inject IUnitOfWork and IMapper
        public CommentsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/comments/video/{videoId}
        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetCommentsByVideo(int videoId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByVideoIdAsync(videoId);
            if (!comments.Any())
                return NotFound(new { Message = "No comments found for this video." });

            // Map the list of comments to DTOs
            var commentDtos = _mapper.Map<IEnumerable<CommentDto>>(comments);
            return Ok(commentDtos);
        }

        // GET: api/comments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUser(int userId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByUserIdAsync(userId);
            if (!comments.Any())
                return NotFound(new { Message = "No comments found for this user." });

            // Map the list of comments to DTOs
            var commentDtos = _mapper.Map<IEnumerable<CommentDto>>(comments);
            return Ok(commentDtos);
        }

        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Fetch Video and User from the database using the IDs
            var video = await _unitOfWork.Videos.GetByIdAsync(commentDto.VideoId);
            var user = await _unitOfWork.Users.GetByIdAsync(commentDto.UserId);

            // Validate that the video and user exist
            if (video == null || user == null)
                return BadRequest(new { Message = "Invalid VideoId or UserId" });

            // Map DTO to entity
            var comment = new Comment
            {
                VideoId = commentDto.VideoId,
                UserId = commentDto.UserId,
                Content = commentDto.Content,
                DateAdded = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddCommentAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment added successfully." });
        }

        // DELETE: api/comments/{id}
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

        // PUT: api/comments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingComment = await _unitOfWork.Comments.GetCommentByIdAsync(id);
            if (existingComment == null)
                return NotFound(new { Message = "Comment not found." });

            // Use AutoMapper to update only the allowed properties
            _mapper.Map(updateCommentDto, existingComment);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment updated successfully." });
        }
    }
}
