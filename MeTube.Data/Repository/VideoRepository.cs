using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public class VideoRepository : Repository<Video>, IVideoRepository
    {
        public ApplicationDbContext DbContext => Context as ApplicationDbContext;

        public VideoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task AddVideoAsync(Video video)
        {
            await AddAsync(video);
        }

        public async Task AddVideoWithoutSaveAsync(Video video)
        {
            // Uses Entity Framworks Change Tracker without saving changes to the database
            await DbContext.Set<Video>().AddAsync(video);
        }

        // Ta bort kommentarer, history och likes som tillhör videon
        // Ta bort videon
        public async Task DeleteVideo(Video video)
        {
            await using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                await DbContext.Comments.Where(a => a.VideoId == video.Id).ExecuteDeleteAsync();
                await DbContext.Histories.Where(b => b.VideoId == video.Id).ExecuteDeleteAsync();
                await DbContext.Likes.Where(c => c.VideoID == video.Id).ExecuteDeleteAsync();
                DbContext.Videos.Remove(video);
                DbContext.SaveChanges();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Kunde inte radera videon.", ex);
            }
        }


        public async Task<IEnumerable<Video>> GetAllVideosAsync()
        {
            return await GetAllAsync();
        }

        public async Task<Video?> GetVideoByIdAsync(int id)
        {
            return await DbContext.Videos
                        .AsNoTracking() // Prevents Entity Framework from tracking changes to the entity
                        .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Video?> GetVideoByIdWithTrackingAsync(int id)
        {
            return await DbContext.Videos
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Video>> GetVideosByUserIdAsync(int userId)
        {
            return await DbContext.Videos.Where(v => v.UserId == userId).ToListAsync();
        }

        public void UpdateVideo(Video video)
        {
            DbContext.Videos.Update(video);
        }
    }
}
