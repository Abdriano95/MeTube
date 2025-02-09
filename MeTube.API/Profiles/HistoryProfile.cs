using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO;

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
        }
    }
}
