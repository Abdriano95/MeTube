using MeTube.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsByVideoIdAsync(int videoId);
        Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task AddCommentAsync(Comment comment);
        void UpdateComment(Comment comment);
        void DeleteComment(Comment comment);
        Task<string> GetPosterUsernameAsync(int userId);
    }
}
