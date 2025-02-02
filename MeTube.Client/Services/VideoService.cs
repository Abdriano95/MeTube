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

        public Task<bool> DeleteVideoAsync(int videoId)
        {
            throw new NotImplementedException();
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

        public Task<List<Video>?> GetVideosByUserIdAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> GetVideoStreamAsync(int id)
        {
            return await _httpClient.GetStreamAsync($"{Constants.VideoStreamUrl}/{id}");
        }


        public Task<Video?> UpdateVideoAsync(Video video)
        {
            throw new NotImplementedException();
        }

        public Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream)
        {
            throw new NotImplementedException();
        }

        public Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream)
        {
            throw new NotImplementedException();
        }

        public Task<Video?> UploadVideoAsync(Video video, Stream videoFileStream, string userId)
        {
            throw new NotImplementedException();
        }
    }
}