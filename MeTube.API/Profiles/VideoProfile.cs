using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO.VideoDTOs;

namespace MeTube.API.Profiles
{
    public class VideoProfile : Profile
    {
        public VideoProfile()
        {
            // Map Video to VideoDto
            CreateMap<Video, VideoDto>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.DateUploaded))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl));

            // Map VideoDto to Video
            CreateMap<VideoDto, Video>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.DateUploaded))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl));

            // Map Video to UploadVideoDto (one-way, for creation)
            CreateMap<UploadVideoDto, Video>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.VideoUrl, opt => opt.Ignore());

            // Map Video to UpdateVideoDto (partial update)
            CreateMap<UpdateVideoDto, Video>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // Ignore null values
        }
    }
}
