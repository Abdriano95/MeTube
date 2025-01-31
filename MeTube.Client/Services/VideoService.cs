using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.VideoDTOs;
using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;

namespace MeTube.Client.Services
{
    public class VideoService : ClientService
    {
        private readonly IMapper _mapper;
        public VideoService(HttpClient client, IMapper mapper, IJSRuntime jsruntime) : base(client, mapper, jsruntime)
        {
            _mapper = mapper;
        }

        public async Task<List<Video>?> GetAllVideosAsync()
        {
            var response = await GetAsync<List<VideoDto>>(Constants.VideoGetAllUrl);
            return _mapper.Map<List<Video>>(response);
        }

        public async Task<Video?> GetVideoByIdAsync(int id)
        {
            var response = await GetAsync<VideoDto>($"{Constants.VideoGetByIdUrl}/{id}");
            return _mapper.Map<Video>(response);
        }

        //Stream video
        public async Task<Stream> GetVideoStreamAsync(int id)
        {
            var response = await GetAsync<Stream>($"{Constants.VideoStreamUrl}/{id}");
            return response;
        }

    }
}
