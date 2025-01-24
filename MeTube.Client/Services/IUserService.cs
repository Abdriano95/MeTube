using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(User user);
        //Task<User?> LoginAsync(string username, string password);
    }
}
