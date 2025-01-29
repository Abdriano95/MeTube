using Azure.Storage;
using Azure.Storage.Blobs;
using MeTube.DTO;
using Microsoft.AspNetCore.Http;

namespace MeTube.API.Services
{
    public class VideoService
    {
        private readonly BlobContainerClient _blobServiceClient;
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
            _blobServiceClient = blobServiceClient.GetBlobContainerClient("videos");
            _blobServiceClient.CreateIfNotExists();
        }

        public async Task<List<BlobDto>> ListAsync()
        {
            List<BlobDto> blobs = new List<BlobDto>();

            await foreach (var blobItem in _blobServiceClient.GetBlobsAsync())
            {
                string uri = _blobServiceClient.Uri.ToString();
                string name = blobItem.Name;
                string fullUri = $"{uri}/{name}";

                blobs.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = blobItem.Properties.ContentType
                });
            }

            return blobs;
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
        {
            BlobResponseDto response = new();
            BlobClient client = _blobServiceClient.GetBlobClient(blob.FileName);

            await using (Stream? stream = blob.OpenReadStream())
            {
                await client.UploadAsync(stream, true);
            }

            response.Status = $"File {blob.FileName} Uploaded Successfully";
            response.Error = false;
            response.Blob = new BlobDto
            {
                Uri = client.Uri.AbsoluteUri,
                Name = client.Name
            };

            return response;
        }

        public async Task<BlobDto?> DownloadAsync(string blobFilename)
        {
            BlobClient file = _blobServiceClient.GetBlobClient(blobFilename);

            if (await file.ExistsAsync())
            {
                var data = await file.OpenReadAsync();

                Stream blobContent = data;

                var content = await file.DownloadContentAsync();

                string name = blobFilename;

                string contentType = content.Value.Details.ContentType;

                return new BlobDto
                {
                    Name = name,
                    ContentType = contentType,
                    Content = blobContent
                };
            }

            return null;
        }
        

        public async Task<BlobResponseDto> DeleteAsync(string blobFilename)
        {
            BlobResponseDto response = new();
            BlobClient file = _blobServiceClient.GetBlobClient(blobFilename);
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
