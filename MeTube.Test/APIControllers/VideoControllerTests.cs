using AutoMapper;
using MeTube.API.Controllers;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.VideoDTOs;
using MeTube.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeTube.Test.APIControllers
{
    public class VideoControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly VideoController _controller;

        public VideoControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockVideoService = new Mock<IVideoService>();
            _controller = new VideoController(_mockUnitOfWork.Object, _mockMapper.Object, _mockVideoService.Object);

            // Setup ClaimsPrincipal for authenticated requests
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        [Fact]
        public async Task GetAllVideos_ReturnsOkWithVideos()
        {
            // Arrange
            var videos = new List<Video>
            {
                new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Genre 1", BlobName = "blob1" },
                new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = "Genre 2", BlobName = "blob2" }
            };
            _mockUnitOfWork.Setup(uow => uow.Videos.GetAllVideosAsync()).ReturnsAsync(videos);
            _mockVideoService.Setup(vs => vs.BlobExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<VideoDto>(It.IsAny<Video>())).Returns((Video source) => new VideoDto
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                Genre = source.Genre,
                BlobExists = true
            });

            // Act
            var result = await _controller.GetAllVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }


        [Fact]
        public async Task GetUploaderUsername_ReturnsOkWithUsername()
        {
            // Arrange
            var video = new Video { Id = 1, UserId = 1, Title = "Video 1", Description = "Description 1", Genre = "Genre 1" };
            var user = new User { Id = 1, Username = "TestUser" , Email = "example@live.se", Password ="pwd123", Role = "User" };
            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1)).ReturnsAsync(video);
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUploaderUsername(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("TestUser", okResult.Value);
        }

        [Fact]
        public async Task GetUploaderUsername_ReturnsNotFound_WhenVideoDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1)).ReturnsAsync((Video)null);

            // Act
            var result = await _controller.GetUploaderUsername(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUploaderUsername_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var video = new Video { Id = 1, UserId = 1, Title = "Video 1", Description = "Description 1", Genre = "Genre 1" };
            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1)).ReturnsAsync(video);
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(1)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUploaderUsername(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetVideoById_ReturnsOkWithVideo()
        {
            // Arrange
            var video = new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Genre 1", BlobName = "blob1" };
            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1)).ReturnsAsync(video);
            _mockVideoService.Setup(vs => vs.BlobExistsAsync(video.BlobName)).ReturnsAsync(true);
            var videoDto = new VideoDto { Id = video.Id, Title = video.Title, Description = video.Description, Genre = video.Genre, BlobExists = true };
            _mockMapper.Setup(m => m.Map<VideoDto>(video)).Returns(videoDto);

            // Act
            var result = await _controller.GetVideoById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<VideoDto>(okResult.Value);
            Assert.Equal(video.Id, model.Id);
        }

        [Fact]
        public async Task GetVideoById_ReturnsNotFound_WhenVideoDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1)).ReturnsAsync((Video)null);

            // Act
            var result = await _controller.GetVideoById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsOkWithVideos()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
            {
                new Video { Id = 1, UserId = userId, Title = "Video 1", Description = "Description 1", Genre = "Genre 1" },
                new Video { Id = 2, UserId = userId, Title = "Video 2", Description = "Description 2", Genre = "Genre 2" }
            };
            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideosByUserIdAsync(userId)).ReturnsAsync(videos);
            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetVideosByUserId();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetVideosByUserId_ReturnsUnauthorized_WhenUserIdIsZero()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.GetVideosByUserId();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetRecommendedVideos_ReturnsOkWithVideos()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
            {
                new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Genre 1" },
                new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = "Genre 2" }
            };
            _mockUnitOfWork.Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5)).ReturnsAsync(videos);
            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetRecommendedVideos_ReturnsUnauthorized_WhenUserIdIsZero()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithLikedVideosButNoGenres_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            var emptyList = new List<Video>();

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithMultipleGenresLiked_ShouldReturnTopGenreVideos()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
    {
        new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Programming" },
        new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = "Programming" }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(videos);

            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
            Assert.All(model, v => Assert.Equal("Programming", v.Genre));
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithOneLikedVideo_ShouldReturnRecommendedVideos()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
    {
        new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Programming" },
        new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = "Programming" }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(videos);

            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
            Assert.All(model, v => Assert.Equal("Programming", v.Genre));
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithNoVideos_ShouldReturnRecentVideos()
        {
            // Arrange
            var userId = 1;
            var recentVideos = new List<Video>
    {
        new Video { Id = 1, Title = "Recent Video 1", Description = "Description 1", Genre = "Music" },
        new Video { Id = 2, Title = "Recent Video 2", Description = "Description 2", Genre = "Music" }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(recentVideos);

            var videoDtos = recentVideos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(recentVideos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
            Assert.All(model, v => Assert.Contains("Recent", v.Title));
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithLikedVideosFromSameUser_ShouldExcludeOwnVideos()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
    {
        new Video { Id = 1, UserId = 2, Title = "Video 1", Description = "Description 1", Genre = "Programming" },
        new Video { Id = 2, UserId = 3, Title = "Video 2", Description = "Description 2", Genre = "Programming" }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(videos);

            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
            Assert.All(model, v => Assert.NotEqual(userId, v.UserId));
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithNoMatchingGenreVideos_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            var emptyList = new List<Video>();

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithAllLikedVideos_ShouldNotRecommendAlreadyLikedVideos()
        {
            // Arrange
            var userId = 1;
            var emptyList = new List<Video>();

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithDeletedLikedVideos_ShouldHandleGracefully()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
    {
        new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Programming" },
        new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = "Programming" }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(videos);

            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithLikedVideosWithoutGenre_ShouldHandleGracefully()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
    {
        new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Programming" },
        new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = null }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(videos);

            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetRecommendedVideos_UserWithInvalidGenreValues_ShouldHandleGracefully()
        {
            // Arrange
            var userId = 1;
            var videos = new List<Video>
    {
        new Video { Id = 1, Title = "Video 1", Description = "Description 1", Genre = "Programming" },
        new Video { Id = 2, Title = "Video 2", Description = "Description 2", Genre = "InvalidGenre" }
    };

            _mockUnitOfWork
                .Setup(uow => uow.Videos.GetRecommendedVideosForUserAsync(userId, 5))
                .ReturnsAsync(videos);

            var videoDtos = videos.Select(v => new VideoDto { Id = v.Id, Title = v.Title, Description = v.Description, Genre = v.Genre });
            _mockMapper.Setup(m => m.Map<IEnumerable<VideoDto>>(videos)).Returns(videoDtos);

            // Act
            var result = await _controller.GetRecommendedVideos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VideoDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }
    }
}
