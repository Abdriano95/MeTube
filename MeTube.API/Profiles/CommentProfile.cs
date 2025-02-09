using AutoMapper;
using MeTube.Data.Entity;
using MeTube.DTO;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        // Mapping for getting comments
        CreateMap<Comment, CommentDto>();

        // Mapping for adding a comment
        CreateMap<CommentDto, Comment>();

        // Mapping for updating a comment (only Content)
        CreateMap<UpdateCommentDto, Comment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Prevent ID from being updated
            .ForMember(dest => dest.VideoId, opt => opt.Ignore()) // Prevent VideoId from being updated
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Prevent UserId from being updated
            .ForMember(dest => dest.DateAdded, opt => opt.Ignore()); // Preserve the original DateAdded
    }
}
