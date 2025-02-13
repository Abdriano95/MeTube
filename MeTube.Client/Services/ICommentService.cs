using MeTube.Client.Models;
using MeTube.DTO.CommentDTOs;

namespace MeTube.Client.Services
{
    public interface ICommentService
    {
        Task<List<Comment>?> GetCommentsByVideoIdAsync(int videoId);
        Task<Comment?> AddCommentAsync(CommentDto commentDto);
        Task<Comment?> UpdateCommentAsync(CommentDto commentDto);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<string?> GetPosterUsernameAsync(int userId);
    }
}