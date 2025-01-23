using AutoMapper;
using MeTube.Data;
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

            // Optionally, map from User to a DTO if needed
            CreateMap<User, UserDto>();
        }
    }
}