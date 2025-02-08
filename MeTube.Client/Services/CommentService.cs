using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MeTube.Client.Services
{
    public class CommentService : ICommentService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly IJSRuntime _jsRuntime;

        public CommentService(HttpClient httpClient, IMapper mapper, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _mapper = mapper;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Comment>?> GetCommentsByVideoIdAsync(int videoId)
        {
            try
            {
                var requestUri = new Uri($"https://localhost:5001/api/comments/video/{videoId}");
                var response = await _httpClient.GetAsync(requestUri);

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var commentDtos = JsonSerializer.Deserialize<List<CommentDto>>(json, _serializerOptions);
                return _mapper.Map<List<Comment>>(commentDtos);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching comments: {ex.Message}");
                return null;
            }
        }

        public async Task<Comment?> AddCommentAsync(CommentDto commentDto)
        {
            await AddAuthorizationHeader(); // Ensure token is added
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(commentDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var requestUri = new Uri("https://localhost:5001/api/comments");

                var response = await _httpClient.PostAsync(requestUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var createdDto = await response.Content.ReadFromJsonAsync<CommentDto>(_serializerOptions);
                    return _mapper.Map<Comment>(createdDto);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Error adding comment: {response.StatusCode}. Response: {errorResponse}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error adding comment: {ex.Message}");
                return null;
            }
        }

        public async Task<Comment?> UpdateCommentAsync(CommentDto commentDto)
        {
            await AddAuthorizationHeader();
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(commentDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var requestUri = new Uri($"https://localhost:5001/api/comments/{commentDto.Id}");

                var response = await _httpClient.PutAsync(requestUri, content);
                response.EnsureSuccessStatusCode();

                var updatedDto = await response.Content.ReadFromJsonAsync<CommentDto>(_serializerOptions);
                return _mapper.Map<Comment>(updatedDto);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating comment: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            await AddAuthorizationHeader();
            try
            {
                var requestUri = new Uri($"https://localhost:5001/api/comments/{commentId}");

                var response = await _httpClient.DeleteAsync(requestUri);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting comment: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> GetPosterUsernameAsync(int userId)
        {
            try
            {
                var requestUri = new Uri($"https://localhost:5001/api/comments/username/{userId}");
                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Failed to fetch username for userId {userId}. StatusCode: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Fetched username: {content}");

                return content;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching poster username: {ex.Message}");
                return null;
            }
        }

    }
}
