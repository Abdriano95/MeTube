using AutoMapper;
using MeTube.DTO;
using MeTube.Client.Models;

namespace MeTube.Client.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Map Entity to DTO
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));


            // Map DTO to Entity
            CreateMap<User, CreateUserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
        }
    }
}
