using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public class UserService : IUserService
    {
        private readonly ClientService _clientService;
        public UserService(ClientService clientservice)
        {
            _clientService = clientservice;
        }

        public Task<bool> RegisterUserAsync(User user)
        {
            return _clientService.RegisterUserAsync(user);
        }

        //public Task<User?> LoginAsync(string username, string password)
        //{
        //    return _clientService.LoginAsync(username, password);
        //}
    }
}
