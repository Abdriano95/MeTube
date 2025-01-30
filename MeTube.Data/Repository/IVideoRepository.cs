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
        Task<IEnumerable<Video>> GetVideosByUserIdAsync(int userId);
        Task AddVideoAsync(Video video);
        void UpdateVideo(Video video);
        void DeleteVideo(Video video);
    }
}
