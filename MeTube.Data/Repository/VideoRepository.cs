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
        /// Retrieves a list of recommended videos for a given user, prioritizing genres of liked videos
        /// and then supplementing with random videos if necessary. 
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve recommendations.</param>
        /// <param name="maxCount">The maximum number of recommended videos to return (default is 5).</param>
        /// <returns>A task that represents the asynchronous operation, returning a collection of recommended videos.</returns>
        /// <remarks>
        /// Algorithm steps:
        /// 1. Retrieves all liked video IDs for the specified user.
        /// 2. If the user has no likes, returns a random set of <paramref name="maxCount"/> videos, excluding the user's own.
        /// 3. Otherwise, groups all liked videos by genre and sorts them by descending frequency.
        /// 4. Iterates over each genre chunk and picks up to a fixed number of videos (genreChunkSize) from that genre,
        ///    excluding the user's own uploads and already liked videos.
        /// 5. If the total count is still below <paramref name="maxCount"/>, fills the remaining slots with random videos
        ///    (again excluding liked and user-owned videos).
        /// </remarks>
        public async Task<IEnumerable<Video>> GetRecommendedVideosForUserAsync(int userId, int maxCount = 5)
        {
            // 1) Collect all video IDs liked by this user
            var likedVideoIds = await DbContext.Likes
                .Where(l => l.UserID == userId)
                .Select(l => l.VideoID)
                .ToListAsync();

            // 2) If user has no likes, return random videos (excluding user's own)
            if (!likedVideoIds.Any())
            {
                return await DbContext.Videos
                    .Where(v => v.UserId != userId)      // Exclude user's own videos
                    .OrderBy(x => Guid.NewGuid())        // Randomize order
                    .Take(maxCount)
                    .ToListAsync();
            }

            // 3) Group all liked videos by genre
            var likedGenres = await DbContext.Videos
                .Where(v => likedVideoIds.Contains(v.Id))
                .Select(v => v.Genre)
                .ToListAsync();

            var groupedGenres = likedGenres
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .ToList();

            var recommended = new List<Video>();
            const int genreChunkSize = 2;  // Example: pick up to 2 from each genre

            // 4) Iterate through genres in priority order
            foreach (var genreGroup in groupedGenres)
            {
                if (recommended.Count >= maxCount) break;

                var genreName = genreGroup.Key;
                int needed = maxCount - recommended.Count;
                int chunk = Math.Min(genreChunkSize, needed);

                // Fetch up to 'chunk' videos in this genre, excluding liked and user-owned
                var genreVideos = await DbContext.Videos
                    .Where(v => v.Genre == genreName
                             && !likedVideoIds.Contains(v.Id)
                             && v.UserId != userId)   // Exclude user's own
                    .OrderByDescending(v => v.DateUploaded)
                    .Take(chunk)
                    .ToListAsync();

                recommended.AddRange(genreVideos);
            }

            // 5) If still under maxCount, fill the remainder with random videos 
            if (recommended.Count < maxCount)
            {
                var excludedIds = new HashSet<int>(likedVideoIds);
                foreach (var r in recommended)
                    excludedIds.Add(r.Id);

                int needed = maxCount - recommended.Count;

                var randomFill = await DbContext.Videos
                    .Where(v => !excludedIds.Contains(v.Id)
                             && v.UserId != userId)   // Exclude user's own again
                    .OrderBy(x => Guid.NewGuid())
                    .Take(needed)
                    .ToListAsync();

                recommended.AddRange(randomFill);
            }

            return recommended;
        }

    }
}
