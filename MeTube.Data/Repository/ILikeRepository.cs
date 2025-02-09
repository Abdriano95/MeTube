using MeTube.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public interface ILikeRepository : IRepository<Like>
    {
        Task AddLikeAsync(Like like);
        Task RemoveLikeAsync(Like like);
        Task<IEnumerable<Like>> GetAllLikesAsync();
        Task<List<Like>> GetLikesForVideoAsync(int videoId);

        // Removing likes for a video as an admin
        Task RemoveLikesForVideoAsync(int videoId);
        Task<Like> GetLikeAsync(int videoId, int userId);
    }
}
