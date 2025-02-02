using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.VideoDTOs;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MeTube.Client.Services
{
    public class VideoService : IVideoService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly IJSRuntime _jsRuntime;

        public VideoService(HttpClient httpClient, IMapper mapper, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
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
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<bool> DeleteVideoAsync(int videoId)
        {
            await AddAuthorizationHeader();
            try
            {
                var response = await _httpClient.DeleteAsync(Constants.VideoDeleteUrl(videoId));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Video>?> GetAllVideosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(Constants.VideoGetAllUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var videoDtos = JsonSerializer.Deserialize<List<VideoDto>>(json, _serializerOptions);
                return _mapper.Map<List<Video>>(videoDtos);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Video?> GetVideoByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Constants.VideoGetByIdUrl(id));
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var videoDto = JsonSerializer.Deserialize<VideoDto>(json, _serializerOptions);
                return _mapper.Map<Video>(videoDto);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Video>?> GetVideosByUserIdAsync()
        {
            await AddAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync(Constants.VideoGetByUserUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var videoDtos = JsonSerializer.Deserialize<List<VideoDto>>(json, _serializerOptions);
                return _mapper.Map<List<Video>>(videoDtos);
            }
            catch
            {
                return null;
            }
        }


        public async Task<Video?> UpdateVideoAsync(Video video)
        {
            await AddAuthorizationHeader();
            try
            {
                var videoDto = _mapper.Map<VideoDto>(video);
                var content = new StringContent(
                    JsonSerializer.Serialize(videoDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync(Constants.VideoUpdateUrl(video.Id), content);
                response.EnsureSuccessStatusCode();

                var updatedDto = await response.Content.ReadFromJsonAsync<VideoDto>(_serializerOptions);
                return _mapper.Map<Video>(updatedDto);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream, string fileName)
        {
            await AddAuthorizationHeader();
            try
            {
                var content = new MultipartFormDataContent();
                var videoContent = new StreamContent(videoFileStream);
                videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                content.Add(videoContent, "file", fileName);

                var response = await _httpClient.PutAsync(Constants.VideoUpdateFileUrl(videoId), content);
                if (!response.IsSuccessStatusCode) return null;

                var updatedDto = await response.Content.ReadFromJsonAsync<VideoDto>(_serializerOptions);
                return _mapper.Map<Video>(updatedDto);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream, string fileName)
        {
            await AddAuthorizationHeader();
            try
            {
                var content = new MultipartFormDataContent();
                var thumbnailContent = new StreamContent(thumbnailFileStream);
                thumbnailContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(thumbnailContent, "thumbnail", fileName);

                var response = await _httpClient.PutAsync(Constants.VideoUpdateThumbnailUrl(videoId), content);
                if (!response.IsSuccessStatusCode) return null;

                var updatedDto = await response.Content.ReadFromJsonAsync<VideoDto>(_serializerOptions);
                return _mapper.Map<Video>(updatedDto);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Video?> UploadVideoAsync(Video video, Stream videoFileStream, string fileName)
        {
            await AddAuthorizationHeader();
            try
            {
                var content = new MultipartFormDataContent();
                var videoDto = _mapper.Map<VideoDto>(video);

                // Metadata
                content.Add(new StringContent(JsonSerializer.Serialize(videoDto)), "metadata");

                // Video file
                var videoContent = new StreamContent(videoFileStream);
                videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                content.Add(videoContent, "file", fileName);

                var response = await _httpClient.PostAsync(Constants.VideoUploadUrl, content);
                response.EnsureSuccessStatusCode();

                var createdDto = await response.Content.ReadFromJsonAsync<VideoDto>(_serializerOptions);
                return _mapper.Map<Video>(createdDto);
            }
            catch
            {
                return null;
            }
        }
    }
}