using MeTube.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public class HistoryRepository : Repository<History>, IHistoryRepository
    {
        public ApplicationDbContext? DbContext => Context as ApplicationDbContext;

        public HistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<History>> GetHistoriesByUserIdAsync(int userId)
        {
            return await DbContext.Histories
           .Include(h => h.Video)
           .Where(h => h.UserId == userId)
           .OrderByDescending(h => h.DateWatched)
           .ToListAsync();
        }
    }    
}
