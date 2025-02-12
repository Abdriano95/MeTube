using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO;

namespace MeTube.API.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Map from CreateUserDto to User
            CreateMap<CreateUserDto, User>();

            // Map from UpdateUserDto to User
            CreateMap<UpdateUserDto, User>();

            // Map from User to ManageUserDto
            CreateMap<User, ManageUserDto>();

            // Optionally, map from User to a DTO if needed
            CreateMap<User, UserDto>();

            CreateMap<User, UserIdDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));

            // Map from User to UserDetailsDto
            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // Map from UserDetailsDto to User
            CreateMap<UserDetailsDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        }
    }
}