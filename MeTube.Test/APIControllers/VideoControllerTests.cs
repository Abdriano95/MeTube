using AutoMapper;
using MeTube.API.Controllers;
using MeTube.API.Services; // Här finns VideoService + IVideoService
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.VideoDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MeTube.Test.APIControllers
{
    public class VideoControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;

        
        private readonly Mock<IVideoService> _mockVideoService;

        private readonly VideoController _controller;

        // Exempel-lista med videor att återanvända
        private readonly List<Video> _videos;

        public VideoControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            // Mock av IVideoService (om VideoController tar in IVideoService).
            _mockVideoService = new Mock<IVideoService>();

            // Simulera en inloggad användare (UserID=1, Role=User som standard)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Skapa controller med mockade beroenden
            _controller = new VideoController(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                // Om din VideoController-konstruktor tar "VideoService" (klassen)
                // måste du ev. partial-mocka klassen i stället för IVideoService
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

            // Några exempelvideor
            _videos = new List<Video>
            {
                new Video {
                    Id = 1,
                    UserId = 1,
                    Title = "TestVideo #1",
                    Description="Desc #1",
                    Genre="Genre #1",
                    BlobName="blob1.mp4",
                    VideoUrl="http://blob1.mp4"
                },
                new Video {
                    Id = 2,
                    UserId = 2,
                    Title = "TestVideo #2",
                    Description="Desc #2",
                    Genre="Genre #2",
                    BlobName="blob2.mp4",
                    VideoUrl="http://blob2.mp4"
                }
            };
        }

        [Fact]
        public async Task GetAllVideos_ReturnsOk_WhenVideosExist()
        {
            // Arrange
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

            // Simulera att alla blobar existerar
            _mockVideoService
                .Setup(s => s.BlobExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.GetAllVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtoList = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, dtoList.Count());

        }
        [Fact]
        public async Task GetAllVideos_ReturnsOk_EmptyList_WhenNoVideosExist()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(u => u.Videos.GetAllVideosAsync())
                .ReturnsAsync(new List<Video>()); // tom

            // Act
            var result = await _controller.GetAllVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtoList = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Empty(dtoList);
        }
        [Fact]
        public async Task GetUploaderUsername_ReturnsOk_WhenVideoAndUserExist()
        {
            // Arrange
            var video = _videos.First();
            var user = new User
            {
                Id = video.UserId,
                Username = "UploaderUser",
                Email = "uploader@example.com",
                Password = "pwd123",
                Role = "User"
            };

            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(video.Id))
                           .ReturnsAsync(video);
            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(video.UserId))
                           .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUploaderUsername(video.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("UploaderUser", okResult.Value);
        }
        [Fact]
        public async Task GetUploaderUsername_ReturnsNotFound_WhenVideoNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((Video)null);

            // Act
            var result = await _controller.GetUploaderUsername(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetUploaderUsername_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var video = _videos.First();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(video.Id))
                           .ReturnsAsync(video);
            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(video.UserId))
                           .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUploaderUsername(video.Id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetVideoById_ReturnsOk_WhenVideoExists()
        {
            // Arrange
            var video = _videos.First();
            _mockUnitOfWork.Setup(u => u.Videos.GetVideoByIdAsync(video.Id))
                           .ReturnsAsync(video);

            _mockMapper
                .Setup(m => m.Map<VideoDto>(video))
                .Returns(new VideoDto
                {
                    Id = video.Id,
                    Title = video.Title
                });

            // BlobExists -> true
            _mockVideoService
                .Setup(s => s.BlobExistsAsync(video.BlobName))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.GetVideoById(video.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<VideoDto>(okResult.Value);
            Assert.Equal(video.Id, dto.Id);
        }

        [Fact]
        public async Task GetVideoById_ReturnsNotFound_WhenVideoDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(u => u.Videos.GetVideoByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Video)null);

            // Act
            var result = await _controller.GetVideoById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ------------------------------------------------------------------
        // GET: api/Video/user
        //  [Authorize]
        // ------------------------------------------------------------------
        [Fact]
        public async Task GetVideosByUserId_ReturnsOk_WhenUserHasVideos()
        {
            // Arrange
            // Standard user = ID=1 i claims
            var userVideos = _videos.Where(v => v.UserId == 1).ToList();
            _mockUnitOfWork
                .Setup(u => u.Videos.GetVideosByUserIdAsync(1))
                .ReturnsAsync(userVideos);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<VideoDto>>(userVideos))
                .Returns(userVideos.Select(v => new VideoDto { Id = v.Id, Title = v.Title }));

            // Act
            var result = await _controller.GetVideosByUserId();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Single(dtos); // end. because _videos had only 1 with UserId=1
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsUnauthorized_WhenUserIdIs0()
        {
            // Arrange
            // Sätt claimsprincipal med NameIdentifier=0
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "0")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = await _controller.GetVideosByUserId();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsOkEmptyList_WhenNoVideosFound()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(u => u.Videos.GetVideosByUserIdAsync(1))
                .ReturnsAsync(new List<Video>()); // tom

            // Act
            var result = await _controller.GetVideosByUserId();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<Video>>(okResult.Value);
            Assert.Empty(list);
        }


    }
}

        