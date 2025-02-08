using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO;

namespace MeTube.Client.Profiles
{
    public class HistoryProfile : Profile
    {
        public HistoryProfile()
        {
            CreateMap<HistoryDto, History>();
            CreateMap<History, HistoryDto>();
        }
    }
}
