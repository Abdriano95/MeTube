﻿using AutoMapper;
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

            CreateMap<int?, UserIdDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.HasValue ? src.Value : 0));
        }
    }
}