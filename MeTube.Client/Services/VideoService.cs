using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.VideoDTOs;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
            var response = await _httpClient.DeleteAsync($"{Constants.VideoBaseUrl}/{videoId}");
            return response.IsSuccessStatusCode;
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


        public async Task<Video?> UpdateVideoAsync(Video video)
        {
            var videoDto = _mapper.Map<VideoDto>(video);
            var content = new StringContent(
                JsonSerializer.Serialize(videoDto, _serializerOptions),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync($"{Constants.VideoBaseUrl}/{video.Id}", content);

            if (!response.IsSuccessStatusCode) return null;

            var updatedDto = await response.Content.ReadFromJsonAsync<VideoDto>(_serializerOptions);
            return _mapper.Map<Video>(updatedDto);
        }

        public Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream)
        {
            throw new NotImplementedException();
        }

        public Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream)
        {
            throw new NotImplementedException();
        }

        public async Task<Video?> UploadVideoAsync(Video video, Stream videoFileStream, string fileName)
        {
            var content = new MultipartFormDataContent();

            // Metadata
            var videoDto = _mapper.Map<VideoDto>(video);
            content.Add(new StringContent(JsonSerializer.Serialize(videoDto)), "metadata");

            // Video file
            var videoContent = new StreamContent(videoFileStream);
            videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            content.Add(videoContent, "file", fileName);

            var response = await _httpClient.PostAsync(Constants.VideoBaseUrl, content);

            if (!response.IsSuccessStatusCode) return null;

            var createdDto = await response.Content.ReadFromJsonAsync<VideoDto>(_serializerOptions);
            return _mapper.Map<Video>(createdDto);
        }
    }
}