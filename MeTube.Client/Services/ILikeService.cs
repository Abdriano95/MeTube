namespace MeTube.Client.Services
{
    public interface ILikeService
    {
        Task<bool> AddLikeAsync(int videoId);
        Task<bool> RemoveLikeAsync(int videoId);
        Task<bool> HasUserLikedVideoAsync(int videoId);
        Task<int> GetLikeCountForVideoAsync(int videoId);
    }
}