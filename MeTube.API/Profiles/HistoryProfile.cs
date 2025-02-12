using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO.HistoryDTOs;

namespace MeTube.API.Profiles
{
    public class HistoryProfile : Profile
    {
        public HistoryProfile()
        {
            // Entity to DTO
            CreateMap<History, HistoryDto>()
            .ForMember(dest => dest.VideoTitle, opt => opt.MapFrom(src => src.Video.Title))
            .ForMember(dest => dest.DateWatched, opt => opt.MapFrom(src => src.DateWatched));

            // DTO to Entity
            CreateMap<HistoryDto, History>()
                .ForMember(dest => dest.Video, opt => opt.Ignore());

            // -- For AdminDto (READ) --
            // Entity to DTO
            CreateMap<History, HistoryAdminDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.VideoTitle, opt => opt.MapFrom(src => src.Video.Title));

            // DTO to Entity
            CreateMap<HistoryAdminDto, History>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Video, opt => opt.Ignore());

            // -- For AdminDto (CREATE) --
            CreateMap<HistoryCreateDto, History>().ReverseMap();

            // -- For AdminDto (UPDATE) --
            CreateMap<HistoryUpdateDto, History>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           // ID is set separately
           .ForMember(dest => dest.DateWatched, opt => opt.Condition(src => src.DateWatched != default));

            // ReverseMap()
            CreateMap<History, HistoryUpdateDto>();

        }
    }
}
