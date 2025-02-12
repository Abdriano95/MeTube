using MeTube.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Data.Repository
{
    public interface IHistoryRepository : IRepository<History>
    {
        Task<IEnumerable<History>> GetHistoriesByUserIdAsync(int userId);
        Task<History?> GetHistoryWithRelationsAsync(int historyId);
    }
}
