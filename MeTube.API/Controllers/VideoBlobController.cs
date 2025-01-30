using MeTube.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoBlobController : ControllerBase
    {
        private readonly VideoService _videoService;

        public VideoBlobController(VideoService videoService)
        {
            _videoService = videoService;
        }

        // POST: api/Blob/Upload
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadBlob(IFormFile file)
        {
            try
            {
                var response = await _videoService.UploadAsync(file);
                return response.Error
                    ? BadRequest(response.Status)
                    : Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Blob/Download/{blobName}
        [HttpGet("Download/{blobName}")]
        public async Task<IActionResult> DownloadBlob(string blobName)
        {
            try
            {
                var blob = await _videoService.DownloadAsync(blobName);
                return blob == null
                    ? NotFound()
                    : File(blob.Content, blob.ContentType, blob.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Blob/List
        [HttpGet("List")]
        public async Task<IActionResult> ListBlobs()
        {
            try
            {
                var blobs = await _videoService.ListAsync();
                return Ok(blobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Blob/Delete/{blobName}
        [HttpDelete("Delete/{blobName}")]
        public async Task<IActionResult> DeleteBlob(string blobName)
        {
            try
            {
                var response = await _videoService.DeleteAsync(blobName);
                return response.Error
                    ? BadRequest(response.Status)
                    : Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
