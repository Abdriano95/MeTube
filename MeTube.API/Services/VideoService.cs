using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MeTube.DTO.VideoDTOs;
using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics;

namespace MeTube.API.Services
{
    public class VideoService : IVideoService
    {
        private readonly BlobContainerClient _videoContainerClient;
        private readonly BlobContainerClient _thumbnailContainerClient;
        private readonly IConfiguration _configuration;

        public VideoService(IConfiguration configuration)
        {
            _configuration = configuration;

            //Get sensitive data from from configuration
            var storageAccount = _configuration["AzureStorage:AccountName"];
            var accessKey = _configuration["AzureStorage:AccountKey"];

            if (string.IsNullOrEmpty(storageAccount) || string.IsNullOrEmpty(accessKey))
            {
                throw new Exception("Azure Storage-configuration is missing or not initiated");
            }

            var credential = new StorageSharedKeyCredential(storageAccount, accessKey);
            var blobUri = $"https://{storageAccount}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            _videoContainerClient = blobServiceClient.GetBlobContainerClient("videos");
            _thumbnailContainerClient = blobServiceClient.GetBlobContainerClient("thumbnails");

            _videoContainerClient.CreateIfNotExists();
            _thumbnailContainerClient.CreateIfNotExists();
        }

        public async Task<bool> BlobExistsAsync(string blobName)
        {
            var blobClient = _videoContainerClient.GetBlobClient(blobName);
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }

        // Thumbnail upload
        public async Task<BlobResponseDto> UploadThumbnailAsync(IFormFile blob)
        {
            BlobResponseDto response = new();
            BlobClient client = _thumbnailContainerClient.GetBlobClient(blob.FileName);
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = blob.ContentType
            };
            var contentType = blob.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(blob.FileName, out contentType))
                {
                    contentType = "application/octet-stream";
                }
            }
            var allowedTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(contentType))
            {
                return new BlobResponseDto
                {
                    Error = true,
                    Status = "Only PNG and JPEG are allowed"
                };
            }
            await using (Stream? stream = blob.OpenReadStream())
            {
                await client.UploadAsync(
                    stream,
                    new BlobUploadOptions { HttpHeaders = blobHttpHeaders },
                    cancellationToken: default
                );
            }
            var properties = await client.GetPropertiesAsync();
            response.Status = $"File {blob.FileName} Uploaded Successfully";
            response.Error = false;
            response.Blob = new BlobDto
            {
                Uri = client.Uri.AbsoluteUri,
                Name = client.Name,
                ContentType = properties.Value.ContentType
            };
            return response;
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
        {
            BlobResponseDto response = new();

            var contentType = blob.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(blob.FileName, out contentType))
                {
                    contentType = "application/octet-stream";
                }
            }
            var allowedTypes = new[] { "video/mp4", "video/webm" };
            if (!allowedTypes.Contains(contentType))
            {
                return new BlobResponseDto
                {
                    Error = true,
                    Status = "Only MP4 & WEBM are allowed"
                };
            }

            var tempIn = Path.GetTempFileName(); 
            var extension = Path.GetExtension(blob.FileName);
            var tempInFinal = Path.ChangeExtension(tempIn, extension);

            File.Move(tempIn, tempInFinal);
            await using (var fs = new FileStream(tempInFinal, FileMode.Create))
            {
                await blob.CopyToAsync(fs);
            }

            var isMp4 = contentType == "video/mp4";
            string finalPathForUpload;

            if (isMp4)
            {
                var tempOut = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_faststart.mp4");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{tempInFinal}\" -c copy -movflags +faststart \"{tempOut}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string stderr = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        return new BlobResponseDto
                        {
                            Error = true,
                            Status = $"ffmpeg failed: {stderr}"
                        };
                    }
                }

                finalPathForUpload = tempOut; 
            }
            else
            {
                finalPathForUpload = tempInFinal;
            }

            try
            {
                var client = _videoContainerClient.GetBlobClient(Path.GetFileName(finalPathForUpload));

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                await using (var uploadFs = new FileStream(finalPathForUpload, FileMode.Open, FileAccess.Read))
                {
                    await client.UploadAsync(uploadFs, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                }

                var properties = await client.GetPropertiesAsync();
                response.Status = $"File {Path.GetFileName(finalPathForUpload)} Uploaded Successfully";
                response.Error = false;
                response.Blob = new BlobDto
                {
                    Uri = client.Uri.AbsoluteUri,
                    Name = client.Name,
                    ContentType = properties.Value.ContentType
                };
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Status = "Upload failed: " + ex.Message;
            }
            finally
            {
                if (File.Exists(tempInFinal)) File.Delete(tempInFinal);
            }

            return response;
        }


        // Delete thumbnail
        public async Task<BlobResponseDto> DeleteThumbnailAsync(string blobFilename)
        {
            BlobResponseDto response = new();
            BlobClient file = _thumbnailContainerClient.GetBlobClient(blobFilename);
            if (await file.ExistsAsync())
            {
                await file.DeleteAsync();
                response.Status = $"File {blobFilename} Deleted Successfully";
                response.Error = false;
            }
            else
            {
                response.Status = $"File {blobFilename} Not Found";
                response.Error = true;
            }
            return response;
        }

        public async Task<BlobResponseDto> DeleteAsync(string blobFilename)
        {
            BlobResponseDto response = new();
            BlobClient file = _videoContainerClient.GetBlobClient(blobFilename);
            if (await file.ExistsAsync())
            {
                await file.DeleteAsync();
                response.Status = $"File {blobFilename} Deleted Successfully";
                response.Error = false;
            }
            else
            {
                response.Status = $"File {blobFilename} Not Found";
                response.Error = true;
            }
            return response;
        }
    }
}
