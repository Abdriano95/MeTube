using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeTube.Client.Services
{
    public class ClientService : IClientService
    {
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        private IMapper _mapper;
        private readonly IJSRuntimeWrapper _jsRuntime;
        public ClientService(HttpClient client, IMapper mapper, IJSRuntimeWrapper jsruntime) 
        {
            _mapper = mapper;
            _jsRuntime = jsruntime;
            _client = client ?? throw new ArgumentNullException(nameof(client));

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                var dto = _mapper.Map<CreateUserDto>(user);
                Uri uri = new Uri(Constants.UserRegisterUrl);
                var response = await _client.PostAsJsonAsync(uri, dto);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Registration failed. StatusCode: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error Content: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
                return false;
            }
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            try
            {
                LoginDto loginDto = new LoginDto { Username = username, Password = password };
                Uri uri = new Uri(Constants.UserLoginUrl);
                HttpResponseMessage response = await _client.PostAsJsonAsync(uri, loginDto);

                if (!response.IsSuccessStatusCode) return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(jsonResponse, _serializerOptions);
                if (loginResponse == null)
                {
                    Debug.WriteLine("Failed to deserialize LoginResponse.");
                    return null;
                }



                return loginResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
                return null;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");

                if (string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("No token found, cannot log out.");
                    return false;
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "api/user/logout");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("User successfully logged out.");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Logout failed. StatusCode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during logout: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            await AddAuthorizationHeader();
            try
            {
                Uri uri = new Uri(Constants.GetAllUsers);
                var response = await _client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to fetch users. StatusCode: {response.StatusCode}");
                    return Enumerable.Empty<User>();
                }

                var users = await response.Content.ReadFromJsonAsync<IEnumerable<User>>(_serializerOptions);

                if (!users.Any())
                {
                    Debug.WriteLine("Failed to deserialize users.");
                    return Enumerable.Empty<User>();
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching users: {ex.Message}");
                return Enumerable.Empty<User>();
            }
        }
        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            try
            {
                Uri uri = new Uri($"{Constants.GetUserIdByEmail}?email={Uri.EscapeDataString(email)}");
                var response = await _client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to fetch user ID: {response.StatusCode}");
                    return null;
                }

                var userIdDto = await response.Content.ReadFromJsonAsync<UserIdDto>(_serializerOptions);
                return userIdDto?.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching users: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteUser(int id)
        {
            await AddAuthorizationHeader();
            try
            {
                Uri uri = new Uri($"{Constants.DeleteUser}/{id}");
                var response = await _client.DeleteAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to fetch user ID: {response.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching users: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            await AddAuthorizationHeader();
            Uri uri = new Uri($"{Constants.UpdateUser}/{id}");
            var response = await _client.PutAsJsonAsync(uri, updateUserDto);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to update user: {response.StatusCode}");
                return false;
            }

            Console.WriteLine("User updated successfully!");
            return true;
        }

        public async Task<Dictionary<string, string>> DoesUserExistAsync(Dictionary<string, string> userData)
        {
            try
            {
                if (!userData.TryGetValue("username", out var username) || !userData.TryGetValue("email", out var email))
                    return new Dictionary<string, string> { { "true", "Invalid input data." } };

                Uri uri = new Uri($"{Constants.CheckUserExistsUrl}?username={username}&email={email}");
                var response = await _client.GetAsync(uri);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = new Dictionary<string, string>();

                if (response.IsSuccessStatusCode)
                {
                    result["false"] = "";
                    return result;
                }
                var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

                if (!jsonResponse.TryGetValue("Exists", out var existsObj) || !jsonResponse.TryGetValue("message", out var messageObj))
                    return new Dictionary<string, string> { { "true", "API response is missing expected keys." } };
                string exists = existsObj.ToString();
                string message = messageObj.ToString();

                return new Dictionary<string, string> {{ exists, message }};
            }
            catch (JsonException jsonEx)
            {
                return new Dictionary<string, string> { { "true", "invalid jsonformat" } };
            }
        }
        public async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                Uri uri = new Uri(string.Format(Constants.GetUserUrl, id));
                var response = await _client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to fetch user ID: {response.StatusCode}");
                    return null;
                }

                var userDto = await response.Content.ReadFromJsonAsync<UserDto>(_serializerOptions);
                return _mapper.Map<User>(userDto);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching users: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<UserDetails>> GetAllUsersDetailsAsync()
        {
            await AddAuthorizationHeader();
            try
            {
                Uri uri = new Uri(Constants.GetAllUsersDetails);
                var response = await _client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to fetch users. StatusCode: {response.StatusCode}");
                    return Enumerable.Empty<UserDetails>();
                }

                var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDetails>>(_serializerOptions);

                if (!users.Any())
                {
                    Debug.WriteLine("Failed to deserialize users.");
                    return Enumerable.Empty<UserDetails>();
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching users: {ex.Message}");
                return Enumerable.Empty<UserDetails>();
            }
        }
    }
}
