using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(User user);
        Task<User?> LoginAsync(string username, string password);
        Task<bool> LogoutAsync();
        Task<string> GetTokenAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<int?> GetUserIdByEmailAsync(string email);
    }
}
