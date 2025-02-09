using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.HistoryDTOs;

namespace MeTube.Client.Profiles
{
    public class HistoryProfile : Profile
    {
        public HistoryProfile()
        {
            CreateMap<HistoryDto, History>();
            CreateMap<History, HistoryDto>();

            //
            // 1) SERVER → CLIENT
            //    "HistoryAdminDto" (server) -> "HistoryAdmin" (client model)
            //
            CreateMap<HistoryAdminDto, HistoryAdmin>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.VideoTitle, opt => opt.MapFrom(src => src.VideoTitle))
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Video, opt => opt.Ignore());

            //
            // 2) CLIENT → SERVER (CREATE)
            //    "HistoryAdmin" -> "HistoryCreateDto"
            //
            CreateMap<HistoryAdmin, HistoryCreateDto>();

            //
            // 3) CLIENT → SERVER (UPDATE)
            //    "HistoryAdmin" -> "HistoryUpdateDto"
            //
            CreateMap<HistoryAdmin, HistoryUpdateDto>();

        }
    }
}
