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
    }
}
