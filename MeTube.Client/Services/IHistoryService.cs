using MeTube.Client.Models;
using MeTube.DTO;

namespace MeTube.Client.Services
{
    public interface IHistoryService
    {
        Task<IEnumerable<History>> GetUserHistoryAsync();
        Task AddHistoryAsync(History history);
    }
}
