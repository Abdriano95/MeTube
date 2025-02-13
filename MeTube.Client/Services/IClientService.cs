using MeTube.Client.Models;
using MeTube.DTO;

namespace MeTube.Client.Services
{
    public interface IClientService
    {
        Task<bool> RegisterUserAsync(User user);
        Task<LoginResponse?> LoginAsync(string username, string password);
        Task<bool> LogoutAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<UserDetails>> GetAllUsersDetailsAsync();
        Task<int?> GetUserIdByEmailAsync(string email);
        Task<User> GetUserByIdAsync(int id);
        Task<bool> DeleteUser(int id);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task AddAuthorizationHeader();
        Task<Dictionary<string, string>> DoesUserExistAsync(Dictionary<string, string> userData);
    }
}
