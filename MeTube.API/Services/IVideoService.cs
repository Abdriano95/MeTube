using MeTube.DTO.VideoDTOs;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs.Models;

namespace MeTube.API.Services
{
    public interface IVideoService
    {
        Task<bool> BlobExistsAsync(string blobName);
        Task<BlobResponseDto> UploadThumbnailAsync(IFormFile blob);
        Task<BlobResponseDto> UploadAsync(IFormFile blob);
        Task<BlobResponseDto> DeleteThumbnailAsync(string blobFilename);
        Task<BlobResponseDto> DeleteAsync(string blobFilename);
        Task<Stream> DownloadRangeAsync(string blobName, long start, long end);
        Task<BlobProperties> GetBlobPropertiesAsync(string blobName);
    }
}
