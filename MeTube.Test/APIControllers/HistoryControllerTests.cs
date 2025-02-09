using AutoMapper;
using MeTube.API.Controllers;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.HistoryDTOs;
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

        //---------------------------------------------------
        //          TESTS FOR AUTHENTICATED USERS
        //---------------------------------------------------

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
            var historyDtos = _histories.Select(h => new HistoryDto { VideoId = h.VideoId });
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

        //---------------------------------------------------
        //          TESTS FOR ADMIN USERS
        //---------------------------------------------------

        [Fact]
        public async Task GetHistoryForUser_ReturnsNotFound_WhenNoHistoryExists()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Username = "TestUser", Email = "Example@example.se", Password ="pwd123", Role ="User" });
            _mockUnitOfWork.Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1)).ReturnsAsync(new List<History>());

            // Act
            var result = await _controller.GetHistoryForUser(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetHistoryForUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(1)).ReturnsAsync((User)null);
            // Act
            var result = await _controller.GetHistoryForUser(1);
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetHistoryForUser_ReturnsNotFound_WhenUserHistoryIsEmpty()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(uow => uow.Users.GetUserByIdAsync(1))
                .ReturnsAsync(new User { Id = 1, Username = "MockUser" , Email = "example@live.se", Password = "pwd213", Role = "User"});

            // History is empty
            _mockUnitOfWork
                .Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1))
                .ReturnsAsync(new List<History>());

            // Act
            var result = await _controller.GetHistoryForUser(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetHistoryForUser_ReturnsOk_WhenUserHasHistory()
        {
            // Arrange
            // User exists
            _mockUnitOfWork
                .Setup(uow => uow.Users.GetUserByIdAsync(1))
                .ReturnsAsync(new User { Id = 1, Username = "MockUser", Email = "example@live.se", Password = "pwd213", Role = "User" });

            // History exists
            _mockUnitOfWork
                .Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1))
                .ReturnsAsync(_histories);

            // Mappa History -> HistoryAdminDto
            var mappedDtos = _histories.Select(h => new HistoryAdminDto
            {
                VideoId = h.VideoId,
                UserName = "MockUser",
                DateWatched = h.DateWatched,
                VideoTitle = "MockVideo",
                UserId = h.UserId,
                Id = h.Id
            });
            _mockMapper
                .Setup(m => m.Map<IEnumerable<HistoryAdminDto>>(_histories))
                .Returns(mappedDtos);

            // Act
            var result = await _controller.GetHistoryForUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDtos = Assert.IsAssignableFrom<IEnumerable<HistoryAdminDto>>(okResult.Value);

            Assert.Equal(_histories.Count, returnedDtos.Count());

        }

        [Fact]
        public async Task GetHistoryForUser_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockUnitOfWork
                .Setup(uow => uow.Users.GetUserByIdAsync(1))
                .ReturnsAsync(new User {Id = 1, Username = "MockUser", Email = "example@live.se", Password = "pwd213", Role = " User" });

            _mockUnitOfWork
                .Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetHistoryForUser(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetHistoryForUser_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // Giltigt användar-ID
                new Claim(ClaimTypes.Role, "User") // Inte admin
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Set the user in HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // Mock if user exists
            _mockUnitOfWork
                .Setup(uow => uow.Users.GetUserByIdAsync(1))
                .ReturnsAsync(new User { Id = 1, Username = "MockUser", Email = "test@example.com", Role = "User", Password = "pwe234" });

            // Mock if user has history
            _mockUnitOfWork
                .Setup(uow => uow.Histories.GetHistoriesByUserIdAsync(1))
                .ReturnsAsync(new List<History> { new History { UserId = 1, VideoId = 1 } });

            // Act
            var result = await _controller.GetHistoryForUser(1);

            // Assert
            Assert.IsType<ForbidResult>(result); // 403 Forbidden
        }

        [Fact]
        public async Task CreateHistory_ReturnsCreated_WhenHistoryIsSuccessfullyAdded()
        {
            // Arrange
            var historyDto = new HistoryCreateDto { UserId = 1, VideoId = 1 };
            var history = new History { UserId = 1, VideoId = 1 };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Username = "TestUser", Email = "Example@example.com", Password = "pwd234", Role = "User" });
            _mockMapper.Setup(m => m.Map<History>(historyDto)).Returns(history);
            _mockUnitOfWork.Setup(u => u.Histories.AddAsync(It.IsAny<History>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<HistoryAdminDto>(history)).Returns(new HistoryAdminDto { UserId = 1, VideoId = 1 });

            // Act
            var result = await _controller.CreateHistory(historyDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<HistoryAdminDto>(createdResult.Value);
        }

        [Fact]
        public async Task CreateHistory_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("UserId", "Required");

            var historyDto = new HistoryCreateDto { UserId = 0, VideoId = 1 };

            // Act
            var result = await _controller.CreateHistory(historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateHistory_ReturnsBadRequest_WhenUserIdIsZero()
        {
            // Arrange
            var historyDto = new HistoryCreateDto { UserId = 0, VideoId = 1 };

            // Act
            var result = await _controller.CreateHistory(historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateHistory_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var historyDto = new HistoryCreateDto { UserId = 1, VideoId = 1 };

            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(1)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.CreateHistory(historyDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateHistory_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var historyDto = new HistoryCreateDto { UserId = 1, VideoId = 1 };

            _mockUnitOfWork.Setup(u => u.Users.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Username = "TestUser", Email = "Example@example.com", Password = "pwd234", Role = "User" });
            _mockUnitOfWork.Setup(u => u.Histories.AddAsync(It.IsAny<History>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateHistory(historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateHistory_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User") // Not admin
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            var historyDto = new HistoryCreateDto { UserId = 1, VideoId = 1 };

            // Act
            var result = await _controller.CreateHistory(historyDto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateHistory_ReturnsOk_WhenHistoryIsSuccessfullyUpdated()
        {
            // Arrange
            var historyDto = new HistoryUpdateDto { Id = 1, UserId = 1, VideoId = 2 };
            var existingHistory = new History { Id = 1, UserId = 1, VideoId = 1 };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync(existingHistory);
            _mockMapper.Setup(m => m.Map(historyDto, existingHistory));
            _mockUnitOfWork.Setup(u => u.Histories.Update(existingHistory));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            var responseObject = Assert.IsType<OkObjectResult>(result).Value;

            // Convert to JSON and deseralize to Dictionary<string, string>
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(responseObject);
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Assert.Equal("History updated", deserialized["Message"]);
        }



        [Fact]
        public async Task UpdateHistory_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("UserId", "Required");

            var historyDto = new HistoryUpdateDto { Id = 1, UserId = 0, VideoId = 2 };

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHistory_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var historyDto = new HistoryUpdateDto { Id = 2, UserId = 1, VideoId = 2 };

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHistory_ReturnsBadRequest_WhenUserIdIsZero()
        {
            // Arrange
            var historyDto = new HistoryUpdateDto { Id = 1, UserId = 0, VideoId = 2 };

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHistory_ReturnsNotFound_WhenHistoryDoesNotExist()
        {
            // Arrange
            var historyDto = new HistoryUpdateDto { Id = 1, UserId = 1, VideoId = 2 };

            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync((History)null);

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHistory_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var historyDto = new HistoryUpdateDto { Id = 1, UserId = 1, VideoId = 2 };
            var existingHistory = new History { Id = 1, UserId = 1, VideoId = 1 };

            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync(existingHistory);
            _mockMapper.Setup(m => m.Map(historyDto, existingHistory));
            _mockUnitOfWork.Setup(u => u.Histories.Update(existingHistory));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateHistory_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User") // Inte admin
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            var historyDto = new HistoryUpdateDto { Id = 1, UserId = 1, VideoId = 2 };

            // Act
            var result = await _controller.UpdateHistory(1, historyDto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteHistory_ReturnsNoContent_WhenHistoryIsSuccessfullyDeleted()
        {
            // Arrange
            var existingHistory = new History { Id = 1, UserId = 1, VideoId = 2 };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync(existingHistory);
            _mockUnitOfWork.Setup(u => u.Histories.Delete(existingHistory));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteHistory(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteHistory_ReturnsNotFound_WhenHistoryDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync((History)null);

            // Act
            var result = await _controller.DeleteHistory(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteHistory_ReturnsBadRequest_WhenUserIdIsZero()
        {
            // Arrange
            var existingHistory = new History { Id = 1, UserId = 0, VideoId = 2 };

            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync(existingHistory);

            // Act
            var result = await _controller.DeleteHistory(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteHistory_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var existingHistory = new History { Id = 1, UserId = 1, VideoId = 2 };

            _mockUnitOfWork.Setup(u => u.Histories.GetHistoryWithRelationsAsync(1)).ReturnsAsync(existingHistory);
            _mockUnitOfWork.Setup(u => u.Histories.Delete(existingHistory));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteHistory(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteHistory_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User") // Inte admin
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            // Act
            var result = await _controller.DeleteHistory(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
