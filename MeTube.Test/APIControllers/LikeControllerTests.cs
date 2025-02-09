using AutoMapper;
using MeTube.API.Controllers;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Claims;


namespace MeTube.Test.APIControllers
{
    public class LikeControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly LikeController _controller;
        private readonly List<Like> _likes;

        public LikeControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new LikeController(_mockUnitOfWork.Object, _mockMapper.Object);
            _likes = new List<Like>
            {
                new Like { UserID = 1, VideoID = 1 },
                new Like { UserID = 2, VideoID = 1 }
            };

            // Setup ClaimsPrincipal för autentiserade anrop
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
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
        public async Task GetAllLikes_ReturnsOkResult_WhenLikesExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Likes.GetAllLikesAsync()).ReturnsAsync(_likes);
            var likeDtos = _likes.Select(l => new LikeDto { VideoID = l.VideoID, UserID = l.UserID });
            _mockMapper.Setup(m => m.Map<IEnumerable<LikeDto>>(_likes)).Returns(likeDtos);

            // Act
            var result = await _controller.GetAllLikes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedLikes = Assert.IsAssignableFrom<IEnumerable<LikeDto>>(okResult.Value);
            Assert.Equal(2, returnedLikes.Count());
        }

        [Fact]
        public async Task GetAllLikes_ReturnsNotFound_WhenNoLikesExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Likes.GetAllLikesAsync()).ReturnsAsync(new List<Like>());

            // Act
            var result = await _controller.GetAllLikes();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddLike_ReturnsCreatedAtAction_WhenValidRequest()
        {
            // Arrange
            var likeDto = new LikeDto { VideoID = 1 };
            var like = new Like { UserID = 1, VideoID = 1 };

            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1))
                .ReturnsAsync(new Video {Id = 1, Title = "Testing 1", Description = "Testing descriptions", Genre = "Test Genre" });
            _mockUnitOfWork.Setup(uow => uow.Likes.AddLikeAsync(It.IsAny<Like>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<Like>(likeDto)).Returns(like);
            _mockMapper.Setup(m => m.Map<LikeDto>(like)).Returns(likeDto);

            // Act
            var result = await _controller.AddLike(likeDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(LikeController.GetLike), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task AddLike_ReturnsBadRequest_WhenDuplicateLike()
        {
            // Arrange
            var likeDto = new LikeDto { VideoID = 1 };
            var like = new Like { UserID = 1, VideoID = 1 };

            _mockUnitOfWork.Setup(uow => uow.Videos.GetVideoByIdAsync(1))
                .ReturnsAsync(new Video {Id = 1, Title = "Testing 1", Description = "Testing descriptions", Genre = "Test Genre" });
            _mockMapper.Setup(m => m.Map<Like>(likeDto)).Returns(like);
            _mockUnitOfWork.Setup(uow => uow.Likes.AddLikeAsync(It.IsAny<Like>()))
                .ThrowsAsync(new ArgumentException("User has already liked this video."));

            // Act
            var result = await _controller.AddLike(likeDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User has already liked this video.", badRequestResult.Value);
        }

        [Fact]
        public async Task RemoveLike_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var likeDto = new LikeDto { VideoID = 1 };
            var like = new Like { UserID = 1, VideoID = 1 };
            _mockMapper.Setup(m => m.Map<Like>(likeDto)).Returns(like);
            _mockUnitOfWork.Setup(uow => uow.Likes.RemoveLikeAsync(It.IsAny<Like>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _controller.RemoveLike(likeDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveLike_ReturnsNotFound_WhenLikeDoesNotExist()
        {
            // Arrange
            var likeDto = new LikeDto { VideoID = 999 };
            var like = new Like { UserID = 1, VideoID = 999 };
            _mockMapper.Setup(m => m.Map<Like>(likeDto)).Returns(like);
            _mockUnitOfWork.Setup(uow => uow.Likes.RemoveLikeAsync(It.IsAny<Like>()))
                .ThrowsAsync(new ArgumentException("Like does not exist."));

            // Act
            var result = await _controller.RemoveLike(likeDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Like does not exist.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetLikesForVideo_ReturnsOkResult()
        {
            // Arrange
            int videoId = 1;
            var videoLikes = _likes.Where(l => l.VideoID == videoId);
            _mockUnitOfWork.Setup(uow => uow.Likes.GetAllLikesAsync()).ReturnsAsync(_likes);
            _mockMapper.Setup(m => m.Map<IEnumerable<LikeDto>>(It.IsAny<IEnumerable<Like>>()))
                .Returns(videoLikes.Select(l => new LikeDto { VideoID = l.VideoID, UserID = l.UserID }));

            // Act
            var result = await _controller.GetLikesForVideo(videoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as LikesForVideoResponseDto;
            Assert.NotNull(response);
            Assert.Equal(2, response.Likes.Count());
        }

        [Fact]
        public async Task RemoveLikeAsAdmin_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            int videoId = 1, userId = 2;
            var like = new Like { VideoID = videoId, UserID = userId };
            _mockUnitOfWork.Setup(uow => uow.Likes.GetLikeAsync(videoId, userId))
                           .ReturnsAsync(like);

            // Act
            var result = await _controller.RemoveLikeAsAdmin(videoId, userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockUnitOfWork.Verify(uow => uow.Likes.RemoveLikeAsync(like), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveLikeAsAdmin_ReturnsNotFound_WhenLikeDoesNotExist()
        {
            // Arrange
            int videoId = 1, userId = 2;
            _mockUnitOfWork.Setup(uow => uow.Likes.GetLikeAsync(videoId, userId))
                           .ReturnsAsync((Like)null);
            // Act
            var result = await _controller.RemoveLikeAsAdmin(videoId, userId);
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
