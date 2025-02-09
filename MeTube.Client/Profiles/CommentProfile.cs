using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO;

namespace MeTube.Client.Profiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            // Map CommentDto to Comment
            CreateMap<CommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VideoId, opt => opt.MapFrom(src => src.VideoId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.DateAdded));

            // Map Comment to CommentDto
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VideoId, opt => opt.MapFrom(src => src.VideoId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.DateAdded));
        }
    }
}
