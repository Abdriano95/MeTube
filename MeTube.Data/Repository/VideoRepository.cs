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

        public void DeleteVideo(Video video)
        {
            Delete(video);
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
