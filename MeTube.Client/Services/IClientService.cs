using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IClientService
    {
        Task<bool> RegisterUserAsync(User user);
        Task<LoginResponse?> LoginAsync(string username, string password);
    }
}
