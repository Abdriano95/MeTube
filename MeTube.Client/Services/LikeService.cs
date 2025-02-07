using AutoMapper;
using MeTube.DTO;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Components;
using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public class LikeService : ILikeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IJSRuntime _jsRuntime;

        public LikeService(HttpClient httpClient, IMapper mapper, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<bool> AddLikeAsync(int videoId)
        {
            await AddAuthorizationHeader();
            try
            {
                var likeDto = new LikeDto { VideoID = videoId };
                var content = new StringContent(
                    JsonSerializer.Serialize(likeDto),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await _httpClient.PostAsync(Constants.LikeAddUrl, content);
                Console.WriteLine($"Response: {await response.Content.ReadAsStringAsync()}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveLikeAsync(int videoId)
        {
            await AddAuthorizationHeader();
            try
            {
                var likeDto = new LikeDto { VideoID = videoId };
                var jsonContent = JsonSerializer.Serialize(likeDto);
                var request = new HttpRequestMessage(HttpMethod.Delete, Constants.LikeRemoveUrl)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasUserLikedVideoAsync(int videoId)
        {
            await AddAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync(Constants.LikeGetByVideoIdUrl(videoId));
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(jsonResponse).RootElement;
                    return result.GetProperty("hasLiked").GetBoolean();
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetLikeCountForVideoAsync(int videoId)
        {
            try
            {
                var response = await _httpClient.GetAsync(Constants.LikeGetForVideoUrl(videoId));
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync(); // Logga detta
                    var result = JsonSerializer.Deserialize<dynamic>(jsonResponse);
                    return result.GetProperty("count").GetInt32();
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<IEnumerable<Like>> GetLikesForVideoManagementAsync(int videoId)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync(Constants.LikeBaseUrl + $"/manage/{videoId}");
            response.EnsureSuccessStatusCode();
            var likeDtos = await response.Content.ReadFromJsonAsync<IEnumerable<LikeDto>>();
            return _mapper.Map<IEnumerable<Like>>(likeDtos);
        }

        // Removing likes for a video as an admin
        public async Task RemoveLikesForVideoAsync(int videoId)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(Constants.LikeBaseUrl + $"/video/{videoId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
