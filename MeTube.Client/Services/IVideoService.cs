using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IVideoService
    {
        Task<List<Video>?> GetAllVideosAsync();
        Task<Video?> GetVideoByIdAsync(int id);
        Task<List<Video>?> GetVideosByUserIdAsync();
        Task<Video?> UploadVideoAsync(Video video, Stream videoFileStream, string userId);

        Task<Video?> UpdateVideoAsync(Video video);

        Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream, string fileName);
        Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream, string fileName);

        Task<bool> DeleteVideoAsync(int videoId);
    }
}
