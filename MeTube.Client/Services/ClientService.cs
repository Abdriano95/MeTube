using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO;
using Microsoft.VisualBasic;
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

        public ClientService(IMapper mapper) 
        {
            _mapper = mapper;

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
                CreateUserDto dto = _mapper.Map<CreateUserDto>(user);
                Uri uri = new Uri(Constants.UserRegisterUrl);
                HttpResponseMessage response = await _client.PostAsJsonAsync(uri, dto);
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

        //public async Task<User?> LoginAsync(string username, string password)
        //{
        //    try
        //    {
        //        LoginDto loginDto = new LoginDto { Username = username, Password = password };
        //        Uri uri = new Uri(Constants.UserLoginUrl);
        //        HttpResponseMessage response = await _client.PostAsJsonAsync(uri, loginDto);

        //        if (!response.IsSuccessStatusCode) return null;

        //        UserDto userDto = await response.Content.ReadFromJsonAsync<UserDto>(_serializerOptions);

        //        if (userDto == null)
        //        {
        //            Debug.WriteLine("Failed to deserialize CustomerDto");
        //            return null;
        //        }
        //        return _mapper.Map<User>(userDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        //        return null;
        //    }
        //}
    }
}
