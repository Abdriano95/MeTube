using MeTube.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public interface IVideoRepository : IRepository<Video>
    {
        Task<IEnumerable<Video>> GetAllVideosAsync();
        Task<Video?> GetVideoByIdAsync(int id);
        Task<Video?> GetVideoByIdWithTrackingAsync(int id);
        Task<IEnumerable<Video>> GetVideosByUserIdAsync(int userId);
        Task AddVideoAsync(Video video);
        Task AddVideoWithoutSaveAsync(Video video);
        void UpdateVideo(Video video);
        Task DeleteVideo(Video video);
    }
}
