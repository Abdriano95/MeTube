using MeTube.Client.Models;
using MeTube.DTO;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

namespace MeTube.Client.Services
{
    public class UserService : IUserService
    {
        private readonly ClientService _clientService;
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        public UserService(ClientService clientservice, IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _clientService = clientservice;
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public Task<bool> RegisterUserAsync(User user)
        {
            return _clientService.RegisterUserAsync(user);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return _clientService.GetAllUsersAsync();
        }

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            return await _clientService.GetUserIdByEmailAsync(email);
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

        public async Task<Dictionary<string, string>> IsUserAuthenticated()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return new Dictionary<string, string>
                {
                    { "IsAuthenticated", "false" },
                    { "Role", "Customer" }
                };
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "User";

            return new Dictionary<string, string>
            {
                { "IsAuthenticated", "true" },
                { "Role", role }};
            }
        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _clientService.DeleteUser(id);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            return await _clientService.UpdateUserAsync(id, updateUserDto);
        }

        public async Task<Dictionary<string, string>> DoesUserExistAsync(string username, string email)
        {
            var userData = new Dictionary<string, string>
            {
                { "username", username },
                { "email", email}
            };
            return await _clientService.DoesUserExistAsync(userData);
        }

    }
}
