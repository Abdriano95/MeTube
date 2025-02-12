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

        public async Task<string> GetVideoUploaderUsernameAsync(int videoId)
        {
            return await DbContext.Videos
                .Where(v => v.Id == videoId)
                .Select(v => v.User.Username)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a list of recommended videos for a user based on their liked videos.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to get recommended videos.</param>
        /// <param name="maxCount">The maximum number of recommended videos to return. Default is 5.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of recommended videos.</returns>
        /// <remarks>
        /// The method performs the following steps:
        /// 1. Retrieves all liked video IDs for the user.
        /// 2. If the user has not liked any videos, it returns the most recently uploaded videos.
        /// 3. Retrieves the genres of the liked videos.
        /// 4. Determines the top genre from the liked videos.
        /// 5. Retrieves other videos in the top genre, excluding the user's own videos and already liked videos.
        /// </remarks>
        public async Task<IEnumerable<Video>> GetRecommendedVideosForUserAsync(int userId, int maxCount = 5)
        {
            // 1) Get all liked video IDs for the user
            var likedVideoIds = await DbContext.Likes
                .Where(l => l.UserID == userId)
                .Select(l => l.VideoID)
                .ToListAsync();

            if (!likedVideoIds.Any())
            {
                // If user has not liked any videos, randomize 5 videos, or return empty list
                return await DbContext.Videos
                    .OrderByDescending(v => v.DateUploaded)
                    .Take(maxCount)
                    .ToListAsync();
            }

            // 2) Get all genres for the liked videos
            var likedGenres = await DbContext.Videos
                .Where(v => likedVideoIds.Contains(v.Id))
                .Select(v => v.Genre)
                .ToListAsync();


            // E.g. find "top genre" or take random
            var topGenre = likedGenres.GroupBy(g => g)
                                      .OrderByDescending(g => g.Count()).ToList();
                                      
            int genreIndex = 0;
            while (genreIndex < topGenre.Count())
            {
                var recommended = await DbContext.Videos
                                    .Where(v => v.Genre == topGenre[genreIndex].Key && !likedVideoIds.Contains(v.Id))
                                    .OrderByDescending(v => v.DateUploaded)
                                    .Take(maxCount)
                                    .ToListAsync();

                if (recommended.Count > 0)
                {
                    return recommended;
                    
                }

                genreIndex++;
            }
            // 3) Get other videos in that genre
            //    Exclude users own videos, or redan already liked videos

            return await DbContext.Videos
                    .OrderByDescending(v => v.DateUploaded)
                    .Take(maxCount)
                    .ToListAsync();
        }


    }
}
