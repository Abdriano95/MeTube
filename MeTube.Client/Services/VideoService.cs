using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.VideoDTOs;
using Microsoft.JSInterop;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace MeTube.Client.Services
{
    public class VideoService : IVideoService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly JsonSerializerOptions _serializerOptions;

        public VideoService(HttpClient httpClient, IMapper mapper)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<bool> DeleteVideoAsync(int videoId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{Constants.VideoBaseUrl}/{videoId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Video>?> GetAllVideosAsync()
        {
            var response = await _httpClient.GetAsync(Constants.VideoBaseUrl);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var videoDtos = JsonSerializer.Deserialize<List<VideoDto>>(json, _serializerOptions);
            return _mapper.Map<List<Video>>(videoDtos);
        }

        public async Task<Video?> GetVideoByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{Constants.VideoGetByIdUrl}/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var videoDto = JsonSerializer.Deserialize<VideoDto>(json, _serializerOptions);
            return _mapper.Map<Video>(videoDto);
        }

        public async Task<List<Video>?> GetVideosByUserIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"{Constants.VideoGetUsersVideos}/{userId}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var videoDtos = JsonSerializer.Deserialize<List<VideoDto>>(json, _serializerOptions);
            return _mapper.Map<List<Video>>(videoDtos);
        }

        public async Task<Stream> GetVideoStreamAsync(int id)
        {
            return await _httpClient.GetStreamAsync($"{Constants.VideoStreamUrl}/{id}");
        }


        public async Task<Video?> UpdateVideoAsync(Video video)
        {
            var videoDto = _mapper.Map<VideoDto>(video);
            var json = JsonSerializer.Serialize(videoDto, _serializerOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{Constants.VideoUpdateUrl}/{video.Id}", content);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedVideoDto = JsonSerializer.Deserialize<VideoDto>(responseJson, _serializerOptions);
            return _mapper.Map<Video>(updatedVideoDto);
        }

        public async Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(videoFileStream), "file", "video.mp4");

            var response = await _httpClient.PutAsync($"{Constants.VideoUpdateFileUrl}/file/{videoId}", content);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedVideoDto = JsonSerializer.Deserialize<VideoDto>(responseJson, _serializerOptions);
            return _mapper.Map<Video>(updatedVideoDto);

        }

        public async Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(thumbnailFileStream), "thumbnailFile", "thumbnail.jpg");

            var response = await _httpClient.PutAsync($"{Constants.VideoUpdateThumbnailUrl}/thumbnail/{videoId}", content);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var videoDto = JsonSerializer.Deserialize<VideoDto>(json, _serializerOptions);
            return _mapper.Map<Video>(videoDto);
        }

        public async Task<Video?> UploadVideoAsync(Video video, Stream videoFileStream, string userId)
        {
            var content = new MultipartFormDataContent();

            // Add video metadata
            var videoDto = _mapper.Map<VideoDto>(video);
            var videoJson = JsonSerializer.Serialize(videoDto, _serializerOptions);
            content.Add(new StringContent(videoJson), "videoMetadata");

            // Add video file
            content.Add(new StreamContent(videoFileStream), "videoFile", "video.mp4");

            // Add user ID
            content.Add(new StringContent(userId), "userId");

            var response = await _httpClient.PostAsync($"{Constants.VideoUploadUrl}/{userId}", content);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            var uploadedVideoDto = JsonSerializer.Deserialize<VideoDto>(responseJson, _serializerOptions);
            return _mapper.Map<Video>(uploadedVideoDto);
        }
    }
}
