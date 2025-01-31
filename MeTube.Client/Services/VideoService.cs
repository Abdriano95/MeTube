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
            var response = await GetAsync<IEnumerable<VideoDto>>($"{Constants.VideoGetAllUrl}");
            var dtos = response?.ToList();
            return _mapper.Map<List<Video>>(dtos);
        }

        public async Task<Video?> GetVideoByIdAsync(int id)
        {
            var response = await GetAsync<VideoDto>($"{Constants.VideoGetByIdUrl}/{id}");
            return _mapper.Map<Video>(response);
        }

    }
}
