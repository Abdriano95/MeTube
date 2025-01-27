using MeTube.Client.Models;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace MeTube.Client.Services
{
    public class UserService : IUserService
    {
        private readonly ClientService _clientService;
        private readonly IJSRuntime _jsRuntime;
        public UserService(ClientService clientservice, IJSRuntime jsRuntime)
        {
            _clientService = clientservice;
            _jsRuntime = jsRuntime;
        }

        public Task<bool> RegisterUserAsync(User user)
        {
            return _clientService.RegisterUserAsync(user);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return _clientService.GetAllUsersAsync();
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var loginResponse = await _clientService.LoginAsync(username, password);
            return loginResponse?.User;
        }

        public async Task<bool> LogoutAsync()
        {
            var response = await _clientService.LogoutAsync();
            if (response)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
                return true;
            }
            return false;
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {
            var response = await _clientService.LoginAsync(username, password);
            return response?.Token ?? string.Empty;
        }
    }
}
