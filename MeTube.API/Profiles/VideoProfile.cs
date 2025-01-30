using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO;

namespace MeTube.API.Profiles
{
    public class VideoProfile : Profile
    {
        public VideoProfile()
        {
            // Map Video to VideoDto
            CreateMap<Video, VideoDto>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.DateUploaded));

            // Map VideoDto to Video
            CreateMap<VideoDto, Video>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.DateUploaded));

            // Map Video to UploadVideoDto (one-way, for creation)
            CreateMap<UploadVideoDto, Video>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // Map Video to UpdateVideoDto (partial update)
            CreateMap<UpdateVideoDto, Video>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // Ignore null values
        }
    }
}
