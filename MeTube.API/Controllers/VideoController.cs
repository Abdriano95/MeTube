using AutoMapper;
using MeTube.API.Services;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.VideoDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly VideoService _videoService;

        public VideoController(IUnitOfWork unitOfWork, IMapper mapper, VideoService videoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _videoService = videoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVideos()
        {
            var videos = await _unitOfWork.Videos.GetAllVideosAsync();

            // Validera att blobbar finns
            var videoDtos = new List<VideoDto>();
            foreach (var video in videos)
            {
                var dto = _mapper.Map<VideoDto>(video);
                dto.BlobExists = await _videoService.BlobExistsAsync(video.BlobName);
                videoDtos.Add(dto);
            }

            return Ok(videoDtos);
        }

        // GET: api/Video/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound();

            var videoDto = _mapper.Map<VideoDto>(video);
            videoDto.BlobExists = await _videoService.BlobExistsAsync(video.BlobName);

            return Ok(videoDto);
        }

        // GET: api/Video/user}
        [HttpGet("user")]
        public async Task<IActionResult> GetVideosByUserId()
        {
            var requestFromUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userIdRequestedFromuser = int.Parse(requestFromUser);
            var videos = await _unitOfWork.Videos.GetVideosByUserIdAsync(userIdRequestedFromuser);
            if (videos.Any())
            {
                var videoDtos = _mapper.Map<IEnumerable<VideoDto>>(videos);
                return Ok(videoDtos);
            }
            else
                return Ok(new List<Video>());
        }

        // POST: api/Video
        [HttpPost]
        public async Task<IActionResult> UploadVideo([FromForm] UploadVideoDto uploadVideoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate the video file
            if (!ValidateVideoFile(uploadVideoDto.VideoFile))
                return BadRequest(ModelState);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Upload video to Azure Blob Storage
                var blobResponse = await _videoService.UploadAsync(uploadVideoDto.VideoFile);
                if (blobResponse.Error)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(blobResponse.Status);
                }

                // 2. Upload thumbnail to Azure Blob Storage
                string? thumbnailUrl = null;
                if (uploadVideoDto.ThumbnailFile != null)
                {
                    var thumbnailResponse = await _videoService.UploadThumbnailAsync(uploadVideoDto.ThumbnailFile);
                    if (thumbnailResponse.Error)
                        throw new Exception(thumbnailResponse.Status);

                    thumbnailUrl = thumbnailResponse.Blob?.Uri;
                }
                else
                {
                    thumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/YouTube_Diamond_Play_Button.png/1200px-YouTube_Diamond_Play_Button.png"; // Default URL
                }

                // 3. Save the metadata without committing the transaction
                var video = _mapper.Map<Video>(uploadVideoDto);
                video.BlobName = blobResponse.Blob.Name;
                video.VideoUrl = blobResponse.Blob.Uri;
                video.ThumbnailUrl = thumbnailUrl;
                video.DateUploaded = DateTime.UtcNow;

                await _unitOfWork.Videos.AddVideoWithoutSaveAsync(video);
                await _unitOfWork.SaveChangesAsync(); // Saves the video to the database

                // 4. Validating the video entity
                if (video.Id == 0)
                {
                    await transaction.RollbackAsync();
                    await _videoService.DeleteAsync(video.BlobName);
                    return BadRequest("Database failed to generate video ID");
                }

                await transaction.CommitAsync();
                var videoDto = _mapper.Map<VideoDto>(video);
                return CreatedAtAction(nameof(GetVideoById), new { id = video.Id }, videoDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (uploadVideoDto?.VideoFile != null)
                {
                    await _videoService.DeleteAsync(uploadVideoDto.VideoFile.FileName);
                }
                return StatusCode(500, $"Transaction failed: {ex.Message}");
            }
        }

        // GET: api/Video/stream/{id}
        [HttpGet("stream/{id}")]
        public async Task<IActionResult> StreamVideo(int id)
        {
            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null || string.IsNullOrEmpty(video.BlobName)) return NotFound();

            var blob = await _videoService.DownloadAsync(video.BlobName);
            if (blob?.Content == null || blob.ContentType == null)
                return NotFound("Blob content or content type is null");

            Response.Headers.Append("Cache-Control", "public,max-age=31536000");
            return File(blob.Content, blob.ContentType, enableRangeProcessing: true);
        }

        // PUT: api/Video/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateVideo(int id, [FromBody] UpdateVideoDto updateVideoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
                if (video == null)
                    return NotFound();

                // Prevents the blob from being overwritten
                var originalBlobName = video.BlobName;
                _mapper.Map(updateVideoDto, video);
                video.BlobName = originalBlobName; // Restore the original blob name

                _unitOfWork.Videos.UpdateVideo(video);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
        }


        // PUT: api/Video/{id}/file
        [HttpPut("{id:int}/file")]
        [Consumes("multipart/form-data")] // Explicit ange content-type
        public async Task<IActionResult> UpdateVideoFile(int id,IFormFile file)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                //Validate the file
                if (!ValidateVideoFile(file))
                    return BadRequest(ModelState);

                var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
                if (video == null)
                    return NotFound();

                // 1. Delete old blob
                var deleteResponse = await _videoService.DeleteAsync(video.BlobName);
                if (deleteResponse.Error)
                    throw new Exception($"Failed to delete old blob: {deleteResponse.Status}");

                // 2. Upload new blob
                var uploadResponse = await _videoService.UploadAsync(file);
                if (uploadResponse.Error)
                    throw new Exception($"Failed to upload new blob: {uploadResponse.Status}");

                // 3. Update metadata
                video.BlobName = uploadResponse.Blob.Name;
                video.VideoUrl = uploadResponse.Blob.Uri;

                _unitOfWork.Videos.UpdateVideo(video);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"File update failed: {ex.Message}");
            }
        }

        // PUT: api/Video/{id}/thumbnail
        [HttpPut("{id:int}/thumbnail")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateThumbnail(int id, IFormFile thumbnailFile)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get video entity with tracking
                var video = await _unitOfWork.Videos.GetVideoByIdWithTrackingAsync(id);
                if (video == null) return NotFound();

                // Delete old thumbnail if it's not the default thumbnail
                if (!string.IsNullOrEmpty(video.ThumbnailUrl) &&
                    !video.ThumbnailUrl.Contains("YouTube_Diamond_Play_Button.png"))
                {
                    var oldThumbnailName = video.ThumbnailUrl.Split('/').Last();
                    await _videoService.DeleteThumbnailAsync(oldThumbnailName);
                }

                // Upload new thumbnail
                var response = await _videoService.UploadThumbnailAsync(thumbnailFile);
                if (response.Error) return BadRequest(response.Status);

                // Update and save metadata
                video.ThumbnailUrl = response.Blob?.Uri;
                _unitOfWork.Videos.UpdateVideo(video);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Thumbnail update failed: {ex.Message}");
            }
        }


        // DELETE: api/Video/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound();

            // Delete thumbnail if it's not the default thumbnail
            if (!string.IsNullOrEmpty(video.ThumbnailUrl) && !video.ThumbnailUrl.Contains("YouTube_Diamond_Play_Button.png"))
            {
                var thumbnailName = video.ThumbnailUrl.Split('/').Last();
                await _videoService.DeleteThumbnailAsync(thumbnailName);
            }


            // Delete blob from Azure Blob Storage
            var deleteResponse = await _videoService.DeleteAsync(video.BlobName);
            if (deleteResponse.Error)
                return BadRequest(deleteResponse.Status);

            // Delete video from database
            _unitOfWork.Videos.DeleteVideo(video);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Video/{id}/default-thumbnail
        [HttpPut("{id:int}/default-thumbnail")]
        public async Task<IActionResult> ResetToDefaultThumbnail(int id)
        {
            var video = await _unitOfWork.Videos.GetVideoByIdAsync(id);
            if (video == null) return NotFound();

            // Delete current thumbnail if it's not the default thumbnail
            if (!video.ThumbnailUrl.Contains("YouTube_Diamond_Play_Button.png"))
            {
                await _videoService.DeleteThumbnailAsync(video.ThumbnailUrl.Split('/').Last());
            }

            video.ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e8/YouTube_Diamond_Play_Button.png/1200px-YouTube_Diamond_Play_Button.png";
            _unitOfWork.Videos.UpdateVideo(video);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // method to validate the video file. Conditions: file size max 500MB, file type mp4
        private bool ValidateVideoFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("VideoFile", "File is empty");
                return false;
            }
            if (file.Length > 500 * 1024 * 1024)
            {
                ModelState.AddModelError("VideoFile", "File size exceeds 500MB");
                return false;
            }
            if (file.ContentType != "video/mp4")
            {
                ModelState.AddModelError("VideoFile", "File type must be mp4");
                return false;
            }
            return true;
        }

        

    }
}
