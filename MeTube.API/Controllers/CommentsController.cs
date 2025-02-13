using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.CommentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeTube.API.Controllers
{
    // This controller manages all comment-related API endpoints.
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // Constructor injection for unit of work and mapper dependencies.
        public CommentsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all comments for a given video.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>Returns a list of CommentDto objects.</returns>
        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetCommentsByVideo(int videoId)
        {
            // Retrieve comments from the repository.
            var comments = await _unitOfWork.Comments.GetCommentsByVideoIdAsync(videoId);
            // Map the comments to DTOs and return them.
            return Ok(_mapper.Map<IEnumerable<CommentDto>>(comments));
        }

        /// <summary>
        /// Retrieves all comments made by a specific user.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <returns>Returns a list of CommentDto objects or a not found message if no comments exist.</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUser(int userId)
        {
            // Retrieve comments for the user.
            var comments = await _unitOfWork.Comments.GetCommentsByUserIdAsync(userId);
            // If no comments are found, return a 404 with a message.
            if (!comments.Any())
                return NotFound(new { Message = "No comments found for this user." });

            // Map the comments to DTOs and return them.
            return Ok(_mapper.Map<IEnumerable<CommentDto>>(comments));
        }

        /// <summary>
        /// Retrieves the username of the user who posted the comment.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <returns>Returns the username as a string.</returns>
        [HttpGet("username/{userId}")]
        public async Task<IActionResult> GetPosterUsername(int userId)
        {
            // Retrieve the user entity by ID.
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            // If user is not found, return a 404 with a message.
            if (user == null)
                return NotFound(new { Message = "User not found." });
            // Return the user's username.
            return Ok(user.Username);
        }

        /// <summary>
        /// Adds a new comment. This endpoint requires authentication and roles "Admin" or "User".
        /// </summary>
        /// <param name="commentDto">The comment data transfer object containing comment details.</param>
        /// <returns>Returns a success message if the comment is added successfully.</returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
        {
            // Validate the model state.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Retrieve the user ID and username from the token claims.
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";

            // Check if the video exists.
            var video = await _unitOfWork.Videos.GetByIdAsync(commentDto.VideoId);
            if (video == null)
                return BadRequest(new { Message = "Invalid VideoId" });

            // Create a new Comment entity from the DTO.
            var comment = new Comment
            {
                Id = commentDto.Id,
                VideoId = commentDto.VideoId,
                UserId = userId,
                Content = commentDto.Content,
                DateAdded = DateTime.UtcNow
            };

            // Add the comment to the repository and save changes.
            await _unitOfWork.Comments.AddCommentAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment added successfully." });
        }

        /// <summary>
        /// Deletes a comment. Only users with the "Admin" role can delete comments.
        /// </summary>
        /// <param name="id">The ID of the comment to delete.</param>
        /// <returns>Returns a success message if the comment is deleted.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            // Retrieve the comment by ID.
            var comment = await _unitOfWork.Comments.GetCommentByIdAsync(id);
            // If the comment does not exist, return a 404.
            if (comment == null)
                return NotFound(new { Message = "Comment not found." });

            // Delete the comment and save changes.
            _unitOfWork.Comments.DeleteComment(comment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment deleted successfully." });
        }

        /// <summary>
        /// Updates an existing comment. Only users with the "Admin" role can update comments.
        /// </summary>
        /// <param name="id">The ID of the comment to update.</param>
        /// <param name="updateCommentDto">The DTO containing updated comment information.</param>
        /// <returns>Returns a success message if the comment is updated.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto updateCommentDto)
        {
            // Validate the model state.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Retrieve the existing comment.
            var existingComment = await _unitOfWork.Comments.GetCommentByIdAsync(id);
            if (existingComment == null)
                return NotFound(new { Message = "Comment not found." });

            // Map the updated DTO values into the existing comment entity.
            _mapper.Map(updateCommentDto, existingComment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Comment updated successfully." });
        }
    }
}
