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
        private readonly IClientService _clientService;
        private readonly IJSRuntimeWrapper _jsRuntime;
        private readonly HttpClient _httpClient;
        public UserService(IClientService clientservice, IJSRuntimeWrapper jsRuntime, HttpClient httpClient)
        {
            _clientService = clientservice;
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        // Registers a new user asynchronously.
        // user: The user to register.
        // Returns: A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
        public Task<bool> RegisterUserAsync(User user)
        {
            return _clientService.RegisterUserAsync(user);
        }

        // Retrieves all users asynchronously.
        // Returns: A task that represents the asynchronous operation. The task result contains an enumerable of users.
        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return _clientService.GetAllUsersAsync();
        }

        // Retrieves the user ID by email asynchronously.
        // email: The email of the user.
        // Returns: A task that represents the asynchronous operation. The task result contains the user ID or null if not found.
        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            return await _clientService.GetUserIdByEmailAsync(email);
        }

        // Logs in a user asynchronously.
        // username: The username of the user.
        // password: The password of the user.
        // Returns: A task that represents the asynchronous operation. The task result contains the logged-in user or null if login failed.
        public async Task<User?> LoginAsync(string username, string password)
        {
            var loginResponse = await _clientService.LoginAsync(username, password);
            return loginResponse?.User;
        }

        // Logs out the current user asynchronously.
        // Returns: A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
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

        // Retrieves the JWT token for a user asynchronously.
        // username: The username of the user.
        // password: The password of the user.
        // Returns: A task that represents the asynchronous operation. The task result contains the JWT token or an empty string if login failed.
        public async Task<string> GetTokenAsync(string username, string password)
        {
            var response = await _clientService.LoginAsync(username, password);
            return response?.Token ?? string.Empty;
        }

        // Checks if the user is authenticated by validating the JWT token stored in local storage.
        // Returns: A task that represents the asynchronous operation. The task result contains a dictionary with authentication status and role.
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
            { "Role", role }
        };
        }

        // Deletes a user by ID asynchronously.
        // id: The ID of the user to delete.
        // Returns: A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _clientService.DeleteUser(id);
        }

        // Updates a user by ID asynchronously.
        // id: The ID of the user to update.
        // updateUserDto: The user data to update.
        // Returns: A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            return await _clientService.UpdateUserAsync(id, updateUserDto);
        }

        // Checks if a user exists by username and email asynchronously.
        // username: The username of the user.
        // email: The email of the user.
        // Returns: A task that represents the asynchronous operation. The task result contains a dictionary with user existence status.
        public async Task<Dictionary<string, string>> DoesUserExistAsync(string username, string email)
        {
            var userData = new Dictionary<string, string>
                {
                    { "username", username },
                    { "email", email}
                };
            return await _clientService.DoesUserExistAsync(userData);
        }

        // Retrieves a user by ID asynchronously.
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _clientService.GetUserByIdAsync(id);
        }

        // Retrieves all user details asynchronously.
        public Task<IEnumerable<UserDetails>> GetAllUsersDetailsAsync()
        {
            return _clientService.GetAllUsersDetailsAsync();
        }
    }
}
