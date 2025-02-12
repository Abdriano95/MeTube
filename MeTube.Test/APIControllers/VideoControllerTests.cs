using AutoMapper;
using MeTube.API.Controllers;
using MeTube.API.Services;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.VideoDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Security.Claims;
using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MeTube.Test.APIControllers
{
    public class VideoControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly VideoController _controller;
        private readonly List<Video> _videos;

        public VideoControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockVideoService = new Mock<IVideoService>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new VideoController(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockVideoService.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            _videos = new List<Video>
            {
                new Video {
                    Id = 1, UserId = 1, Title = "TestVideo #1",
                    Description="Desc #1", Genre="Genre #1",
                    BlobName="blob1.mp4", VideoUrl="http://blob1.mp4"
                },
                new Video {
                    Id = 2, UserId = 2, Title = "TestVideo #2",
                    Description="Desc #2", Genre="Genre #2",
                    BlobName="blob2.mp4", VideoUrl="http://blob2.mp4"
                }
            };
        }

        [Fact]
        public async Task GetAllVideos_ReturnsOk_WhenVideosExist()
        {
            _mockUnitOfWork
                .Setup(u => u.Videos.GetAllVideosAsync())
                .ReturnsAsync(_videos);

            _mockMapper
                .Setup(m => m.Map<VideoDto>(It.IsAny<Video>()))
                .Returns((Video v) => new VideoDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Description = v.Description
                });

            _mockVideoService
                .Setup(s => s.BlobExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _controller.GetAllVideos();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtoList = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, dtoList.Count());
        }

        [Fact]
        public async Task GetAllVideos_ReturnsOk_EmptyList_WhenNoVideosExist()
        {
            _mockUnitOfWork
                .Setup(u => u.Videos.GetAllVideosAsync())
                .ReturnsAsync(new List<Video>());

            var result = await _controller.GetAllVideos();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtoList = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Empty(dtoList);
        }

        [Fact]
        public async Task GetUploaderUsername_ReturnsOk_WhenVideoAndUserExist()
        {
            var video = _videos.First();
            var user = new User
            {
                Id = video.UserId,
                Username = "UploaderUser",
                Email = "test@example.com",
                Password = "pwd123",
                Role = "User"
            };
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(video.Id))
                           .ReturnsAsync(video);
            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(video.UserId))
                           .ReturnsAsync(user);

            var result = await _controller.GetUploaderUsername(video.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("UploaderUser", okResult.Value);
        }

        [Fact]
        public async Task GetUploaderUsername_ReturnsNotFound_WhenVideoNotFound()
        {
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((Video)null);

            var result = await _controller.GetUploaderUsername(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUploaderUsername_ReturnsNotFound_WhenUserNotFound()
        {
            var video = _videos.First();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(video.Id))
                           .ReturnsAsync(video);
            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(video.UserId))
                           .ReturnsAsync((User)null);

            var result = await _controller.GetUploaderUsername(video.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetVideoById_ReturnsOk_WhenVideoExists()
        {
            var video = _videos.First();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(video.Id))
                           .ReturnsAsync(video);
            _mockMapper.Setup(m => m.Map<VideoDto>(video))
                       .Returns(new VideoDto { Id = video.Id, Title = video.Title });
            _mockVideoService.Setup(s => s.BlobExistsAsync(video.BlobName)).ReturnsAsync(true);

            var result = await _controller.GetVideoById(video.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<VideoDto>(okResult.Value);
            Assert.Equal(video.Id, dto.Id);
        }

        [Fact]
        public async Task GetVideoById_ReturnsNotFound_WhenVideoDoesNotExist()
        {
            _mockUnitOfWork
                .Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Video)null);

            var result = await _controller.GetVideoById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsOk_WhenUserHasVideos()
        {
            var userVideos = _videos.Where(v => v.UserId == 1).ToList();
            _mockUnitOfWork
                .Setup(u => u.Videos.GetVideosByUserIdAsync(1))
                .ReturnsAsync(userVideos);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<VideoDto>>(userVideos))
                .Returns(userVideos.Select(v => new VideoDto { Id = v.Id, Title = v.Title }));

            var result = await _controller.GetVideosByUserId();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Single(dtos);
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsUnauthorized_WhenUserIdIs0()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "0") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var result = await _controller.GetVideosByUserId();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsOkEmptyList_WhenNoVideosFound()
        {
            _mockUnitOfWork
                .Setup(u => u.Videos.GetVideosByUserIdAsync(1))
                .ReturnsAsync(new List<Video>());

            var result = await _controller.GetVideosByUserId();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<Video>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task UploadVideo_ReturnsCreated_WhenValid()
        {
            var uploadDto = new UploadVideoDto
            {
                Title = "NewVideo",
                Description = "TestDesc",
                Genre = "TestGenre"
            };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.ContentType).Returns("video/mp4");
            uploadDto.VideoFile = mockFile.Object;

            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                           .ReturnsAsync(mockTransaction.Object);

            _mockVideoService
                .Setup(vs => vs.UploadAsync(mockFile.Object))
                .ReturnsAsync(new BlobResponseDto
                {
                    Error = false,
                    Blob = new BlobDto { Name = "blobname.mp4", Uri = "http://somewhere/blob.mp4" }
                });

            _mockUnitOfWork
                .Setup(u => u.Videos.AddVideoWithoutSaveAsync(It.IsAny<Video>()))
                .Callback<Video>(v => { v.Id = 999; })
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<Video>(uploadDto))
                .Returns(new Video
                {
                    Id = 0,
                    Title = uploadDto.Title,
                    Description = uploadDto.Description!,
                    Genre = uploadDto.Genre!
                });

            _mockMapper
                .Setup(m => m.Map<VideoDto>(It.IsAny<Video>()))
                .Returns(new VideoDto { Id = 999, Title = "NewVideo" });

            var result = await _controller.UploadVideo(uploadDto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(VideoController.GetVideoById), created.ActionName);
            var returnedDto = Assert.IsType<VideoDto>(created.Value);
            Assert.Equal(999, returnedDto.Id);
        }

        [Fact]
        public async Task UploadVideo_ReturnsBadRequest_WhenFileIsInvalid()
        {
            var uploadDto = new UploadVideoDto
            {
                Title = "BadVideo",
                Description = "Desc",
                Genre = "Genre"
            };
            uploadDto.VideoFile = null;

            var result = await _controller.UploadVideo(uploadDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadVideo_ReturnsUnauthorized_WhenUserIdIs0()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "0") };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var uploadDto = new UploadVideoDto
            {
                Title = "T",
                Description = "D",
                Genre = "G"
            };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.ContentType).Returns("video/mp4");
            uploadDto.VideoFile = mockFile.Object;

            var result = await _controller.UploadVideo(uploadDto);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DeleteVideo_ReturnsOk_WhenSuccessful()
        {
            var vid = _videos.First();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(vid.Id))
                           .ReturnsAsync(vid);

            _mockVideoService
                .Setup(s => s.DeleteAsync(vid.BlobName))
                .ReturnsAsync(new BlobResponseDto { Error = false });

            _mockUnitOfWork
                .Setup(u => u.Videos.DeleteVideo(vid))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteVideo(vid.Id);

            var okResult = Assert.IsType<OkResult>(result);
            _mockVideoService.Verify(s => s.DeleteAsync(vid.BlobName), Times.Once);
            _mockUnitOfWork.Verify(u => u.Videos.DeleteVideo(vid), Times.Once);
        }

        [Fact]
        public async Task DeleteVideo_ReturnsNotFound_WhenVideoIsNull()
        {
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((Video)null);

            var result = await _controller.DeleteVideo(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteVideo_ReturnsUnauthorized_WhenUserIdIs0()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "0") };
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(claims));

            var result = await _controller.DeleteVideo(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DeleteVideo_ReturnsBadRequest_WhenDeleteBlobFails()
        {
            var vid = _videos.First();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(vid.Id))
                           .ReturnsAsync(vid);

            _mockVideoService.Setup(s => s.DeleteAsync(vid.BlobName))
                             .ReturnsAsync(new BlobResponseDto { Error = true, Status = "Delete fail" });

            var result = await _controller.DeleteVideo(vid.Id);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Delete fail", badRequest.Value);
        }

        [Fact]
        public async Task UpdateVideo_ReturnsOk_WhenVideoExistsAndAdmin()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(claims));

            var existingVideo = _videos.First();
            var updateDto = new UpdateVideoDto
            {
                Title = "Updated Title",
                Description = "Updated Desc",
                Genre = "Updated Genre"
            };

            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync())
                           .ReturnsAsync(mockTransaction.Object);

            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(existingVideo.Id))
                           .ReturnsAsync(existingVideo);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<VideoDto>(existingVideo))
                       .Returns(new VideoDto { Id = existingVideo.Id, Title = updateDto.Title });

            var result = await _controller.UpdateVideo(existingVideo.Id, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<VideoDto>(okResult.Value);
            Assert.Equal("Updated Title", returnedDto.Title);
        }

        [Fact]
        public async Task UpdateVideo_ReturnsNotFound_WhenVideoIsNull()
        {
            var updateDto = new UpdateVideoDto();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((Video)null);

            var result = await _controller.UpdateVideo(999, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateVideo_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, "1"),   // en inloggad användare
                    new Claim(ClaimTypes.Role, "User")          // men inte "Admin"
            };
            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Mocka transaktionen så den inte kastar exception i onödan
            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction
                .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockTransaction
                .Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            // Om koden inte hittar en video innan den "avbryts", mocka ev. bort det:
            // _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
            //                .ReturnsAsync(new Video { ... });

            var updateDto = new UpdateVideoDto
            {
                Title = "Title",
                Description = "Desc",
                Genre = "Genre"
            };

            // Act
            var result = await _controller.UpdateVideo(1, updateDto);

            // Assert
            // [Authorize(Roles="Admin")] ger i enhetstester ofta en ObjectResult(403) snarare än ForbidResult.
            var objectRes = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectRes.StatusCode);
        }

    }
}
