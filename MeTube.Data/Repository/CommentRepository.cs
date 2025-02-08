using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace MeTube.Data.Repository
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public ApplicationDbContext DbContext => Context as ApplicationDbContext;

        public CommentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Comment>> GetCommentsByVideoIdAsync(int videoId)
        {
            return await DbContext.Comments
                .Where(c => c.VideoId == videoId)
                .Include(c => c.Video)
                .Include(c => c.User)
                .OrderByDescending(c => c.DateAdded)
                .ToListAsync();
        }
        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(int userId)
        {
            return await DbContext.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Video)
                .Include(c => c.User)
                .OrderByDescending(c => c.DateAdded)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await DbContext.Comments
                .Include(c => c.User)
                .Include(c => c.Video)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await AddAsync(comment);
        }

        public void DeleteComment(Comment comment)
        {
            Delete(comment);
        }

        public void UpdateComment(Comment comment)
        {
            Update(comment);
        }

        public async Task<string> GetPosterUsernameAsync(int userId)
        {
            return await DbContext.Comments
                .Where(u => u.Id == userId)
                .Select(u => u.User.Username)
                .FirstOrDefaultAsync();
        }
    }
}
