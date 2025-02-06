using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO;

namespace MeTube.Client.Profiles
{
    public class LikeProfile : Profile
    {
        public LikeProfile()
        {
            CreateMap<LikeDto, Like>();
            CreateMap<Like, LikeDto>();
        }
    }
}
