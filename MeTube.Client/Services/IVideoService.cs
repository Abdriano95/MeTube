using MeTube.Client.Models;

namespace MeTube.Client.Services
{
    public interface IVideoService
    {
        Task<List<Video>?> GetAllVideosAsync();
        Task<Video?> GetVideoByIdAsync(int id);
        Task<List<Video>?> GetVideosByUserIdAsync();
        Task<List<Video>?> GetRecommendedVideosAsync();
        Task<Video?> UploadVideoAsync(Video video, MemoryStream videoStream, string videoFileName,
                                     MemoryStream? thumbnailStream = null, string? thumbnailFileName = null);
        Task<Video?> UpdateVideoAsync(Video video);

        Task<Video?> UpdateVideoFileAsync(int videoId, Stream videoFileStream, string fileName);
        Task<Video?> UpdateVideoThumbnailAsync(int videoId, Stream thumbnailFileStream, string fileName);
        Task<bool> ResetThumbnail(int videoId);
        Task<bool> DeleteVideoAsync(int videoId);
        Task<string?> GetUploaderUsernameAsync(int videoId);
    }
}
