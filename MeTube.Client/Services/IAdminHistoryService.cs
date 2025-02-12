using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IAdminHistoryService
    {
        Task<List<HistoryAdmin>> GetHistoryByUserAsync(int userId);
        Task<HistoryAdmin?> CreateHistoryAsync(HistoryAdmin newHistory);
        Task<bool> UpdateHistoryAsync(HistoryAdmin history);
        Task<bool> DeleteHistoryAsync(int historyId);
    }
}
