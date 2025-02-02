using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IVideoService
    {
        Task<List<Video>?> GetAllVideosAsync();
        Task<Video?> GetVideoByIdAsync(int id);
        Task<Stream> GetVideoStreamAsync(int id);

        Task<Video?> UploadVideoAsync(Video video, Stream videoFileStream, string userId);

        Task<Video?> UpdateVideoAsync(Video video);

        Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream);

        Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream);

        Task<bool> DeleteVideoAsync(int videoId);
    }
}
