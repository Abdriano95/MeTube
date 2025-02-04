using MeTube.Client.Models;
using MeTube.DTO;

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
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<Dictionary<string, string>> IsUserAuthenticated();
        Task<Dictionary<string, string>> DoesUserExistAsync(string username, string email);
    }
}
