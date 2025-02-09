using AutoMapper;
using MeTube.API.Controllers;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace MeTube.Test.APIControllers
{
    public class HistoryControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HistoryController _controller;
        private readonly List<History> _histories;

        public HistoryControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new HistoryController(_mockUnitOfWork.Object, _mockMapper.Object);
            _histories = new List<History>
            {
                new History { UserId = 1, VideoId = 1 },
                new History { UserId = 2, VideoId = 1 }
            };
            // Setup ClaimsPrincipal for authenticated requests
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
        public async Task GetMyHistory_ReturnsNotFound_WhenNoHistoryExists()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1)).ReturnsAsync(new List<History>());
            // Act
            var result = await _controller.GetAllHistory();
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetMyHistory_ReturnsOkWithData()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1)).ReturnsAsync(_histories);
            var historyDtos = _histories.Select(h => new HistoryDto { VideoId = h.VideoId});
            _mockMapper.Setup(m => m.Map<IEnumerable<HistoryDto>>(_histories)).Returns(historyDtos);
            // Act
            var result = await _controller.GetAllHistory();
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<HistoryDto>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetMyHistory_ReturnsUnauthorized_WhenUserIdIsZero()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            // Act
            var result = await _controller.GetAllHistory();
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetMyHistory_ReturnsUnauthorized_WhenUserIdIsNotSet()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            // Act
            var result = await _controller.GetAllHistory();
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetMyHistory_ReturnsUnauthorized_WhenUserIdIsZeroString()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "0")
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
            // Act
            var result = await _controller.GetAllHistory();
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task AddHistory_ReturnsOk_WhenHistoryIsAdded()
        {
            // Arrange
            var historyDto = new HistoryDto { VideoId = 1 };
            var history = new History { VideoId = 1, UserId = 1 };

            // Mocka mappningen från DTO till History
            _mockMapper.Setup(m => m.Map<History>(historyDto)).Returns(history);

            // Mocka AddAsync och SaveChangesAsync för att returnera "klar" utan undantag
            _mockUnitOfWork.Setup(u => u.Histories.AddAsync(It.IsAny<History>()))
                          .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                          .ReturnsAsync(1);

            // Act
            var result = await _controller.AddHistory(historyDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddHistory_ReturnsBadRequest_WhenHistoryIsNotAdded()
        {
            // Arrange
            var historyDto = new HistoryDto { VideoId = 1 };
            var history = new History { VideoId = 1, UserId = 1 };
            _mockMapper.Setup(m => m.Map<History>(historyDto)).Returns(history);
            _mockUnitOfWork.Setup(uow => uow.Histories.AddAsync(history)).Throws(new Exception("Error adding history"));
            // Act
            var result = await _controller.AddHistory(historyDto);
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddHistory_ReturnsUnauthorized_WhenUserIdIsZero()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            // Act
            var result = await _controller.AddHistory(new HistoryDto());
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task AddHistory_ReturnsUnauthorized_WhenUserIdIsNotSet()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            // Act
            var result = await _controller.AddHistory(new HistoryDto());
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
