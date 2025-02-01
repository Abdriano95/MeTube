using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.VideoDTOs;

namespace MeTube.Client.Profiles
{
    public class VideoProfile : Profile
    {
        public VideoProfile()
        {
            //Map VideoDto to Video
            CreateMap<VideoDto, Video>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl))
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.DateUploaded))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            //Map Video to VideoDto
            CreateMap<Video, VideoDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl))
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.DateUploaded))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            // Map Video to UploadVideoDto (one-way, for creation)
            CreateMap<UploadVideoDto, Video>()
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.VideoUrl, opt => opt.Ignore());

            // Map Video to UpdateVideoDto (partial update)
            CreateMap<UpdateVideoDto, Video>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
