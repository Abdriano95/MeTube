using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IClientService
    {
        Task<bool> RegisterUserAsync(User user);
        Task<LoginResponse?> LoginAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<int?> GetUserIdByEmailAsync(string email);
        Task<bool> DeleteUser(int id);
    }
}
