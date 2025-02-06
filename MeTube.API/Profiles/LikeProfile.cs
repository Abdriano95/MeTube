using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO;

namespace MeTube.API.Profiles
{
    public class LikeProfile : Profile
    {
        public LikeProfile()
        {
            // Map Like to LikeDto
            CreateMap<Like, LikeDto>()
                .ForMember(dest => dest.VideoID, opt => opt.MapFrom(src => src.VideoID))
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.VideoTitle, opt => opt.MapFrom(src => src.Video.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));
            // Map LikeDto to Like
            CreateMap<LikeDto, Like>()
                .ForMember(dest => dest.VideoID, opt => opt.MapFrom(src => src.VideoID))
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID));
        }
    }
}
