using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public class LikeRepository : Repository<Like>, ILikeRepository
    {
        public ApplicationDbContext? DbContext => Context as ApplicationDbContext;

        public LikeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task AddLikeAsync(Like like)
        {
            var existingLike = await DbContext.Likes.FindAsync(like.VideoID, like.UserID);
            if (existingLike != null)
                throw new ArgumentException("User has already liked this video.");

            await DbContext.Likes.AddAsync(like); 
        }

        public async Task RemoveLikeAsync(Like like)
        {
            var existingLike = await DbContext.Likes.FindAsync(like.VideoID, like.UserID);
            if (existingLike == null)
                throw new ArgumentException("Like does not exist.");

            DbContext.Likes.Remove(existingLike);
        }

        public async Task<IEnumerable<Like>> GetAllLikesAsync()
        {
            return await DbContext.Likes
                .Include(l => l.User)
                .Include(l => l.Video)
                .ToListAsync();
        }

        public async Task<List<Like>> GetLikesForVideoAsync(int videoId)
        {
            return await DbContext.Likes
                .Include(l => l.User)
                .Include(l => l.Video)
                .Where(l => l.VideoID == videoId)
                .ToListAsync();
        }

        public async Task RemoveLikesForVideoAsync(int videoId)
        {
            var likes = await DbContext.Likes
                .Where(l => l.VideoID == videoId)
                .ToListAsync();
            DbContext.Likes.RemoveRange(likes);
        }
    }
}
