using MeTube.API.Controllers;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeTube.Test.APIControllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new UserController(_mockUnitOfWork.Object, _mockMapper.Object);

            // Setup ClaimsPrincipal för autentiserade anrop
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"), // Mockar inloggad användare med ID 1
                new Claim(ClaimTypes.Role, "Admin") // Om metoder kräver admin-behörighet
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
        public async Task GetUser_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User 
            { 
                Id = userId, 
                Username = "TestUser", 
                Email = "test@example.com", 
                Password = "Svartlosenord",
                Role = "User" 
            
            };
            var userDto = new UserDto { Id = userId, Username = "TestUser", Email = "test@example.com" };

            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userDto.Username, returnedUser.Username);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SignUp_ShouldCreateUserSuccessfully()
        {
            var request = new CreateUserDto { Username = "NewUser", Email = "new@example.com", Password = "password" };
            var user = new User { Id = 1, Username = "NewUser", Email = "new@example.com", Password = "password", Role = "User" };

            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByUsernameAsync(request.Username)).ReturnsAsync((User)null);
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByEmailAsync(request.Email)).ReturnsAsync((User)null);
            _mockMapper.Setup(m => m.Map<User>(request)).Returns(user);
            _mockUnitOfWork.Setup(uow => uow.Users.AddUserAsync(user)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _controller.SignUp(request);

            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var response = okObjectResult.Value;

            Assert.NotNull(response);
            Assert.True(response.GetType().GetProperty("Message") != null, "Response saknar Message");

            var message = response.GetType().GetProperty("Message")?.GetValue(response, null);
            Assert.NotNull(message);
            Assert.Equal("User signed up successfully", message);
        }

        [Fact]
        public async Task SignUp_ShouldReturnBadRequest_WhenUsernameExists()
        {
            var request = new CreateUserDto { Username = "ExistingUser", Email = "existing@example.com", Password = "password" };
            var existingUser = new User { Id = 1, Username = "ExistingUser", Email = "existing@example.com", Password = "password", Role = "User" };

            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByUsernameAsync(request.Username)).ReturnsAsync(existingUser);

            var result = await _controller.SignUp(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;

            Assert.NotNull(response);
            Assert.True(response.GetType().GetProperty("Message") != null, "Response saknar Message");

            var message = response.GetType().GetProperty("Message")?.GetValue(response, null);
            Assert.NotNull(message);
            Assert.Equal("Username already exists", message);
        }

        [Fact]
        public async Task DeleteUser_ShouldDeleteUser_WhenUserExists()
        {
            var userId = 2;
            var user = new User
            {
                Id = userId,
                Username = "Pärsan",
                Email = "Hej@gmail.com",
                Password = "Hej123",
                Role = "Admin"
            };

            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockUnitOfWork.Setup(uow => uow.Users.DeleteUser(user)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _controller.DeleteUser(userId);

            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var response = okObjectResult.Value;

            Assert.NotNull(response);

            Assert.True(response.GetType().GetProperty("Message") != null, "Response saknar Message");

            var message = response.GetType().GetProperty("Message")?.GetValue(response, null);
            Assert.NotNull(message);
            Assert.Equal("User deleted", message);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var userId = 5;

            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _controller.DeleteUser(userId);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundObjectResult.Value;

            Assert.NotNull(response);
            Assert.True(response.GetType().GetProperty("Message") != null, "Response saknar Message");

            var message = response.GetType().GetProperty("Message")?.GetValue(response, null);
            Assert.NotNull(message);
            Assert.Equal("User not found", message);
        }

        [Fact]
        public async Task GetUserIdByEmail_ShouldReturnUserId_WhenEmailExists()
        {
            // Arrange
            var email = "test@example.com";
            var userId = 1;
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserIdByEmailAsync(email)).ReturnsAsync(userId);

            // Act
            var result = await _controller.GetUserIdByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<UserIdDto>(okResult.Value);
            Assert.Equal(userId, returnedDto.Id);
        }

        [Fact]
        public async Task GetUserIdByEmail_ShouldReturnNotFound_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserIdByEmailAsync(email)).ReturnsAsync((int?)null);

            // Act
            var result = await _controller.GetUserIdByEmail(email);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            var request = new LoginDto { Username = "TestUser", Password = "password" };
            var user = new User { Id = 1, Username = "TestUser", Email = "test@example.com", Password = "password", Role = "User" };

            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByUsernameAsync(request.Username)).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(new UserDto { Id = user.Id, Username = user.Username });

            var result = await _controller.Login(request);

            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var response = okObjectResult.Value;

            Assert.NotNull(response);

            Assert.True(response.GetType().GetProperty("Token") != null, "Response object saknar Token");

            var token = response.GetType().GetProperty("Token")?.GetValue(response, null);
            Assert.NotNull(token);
            Assert.IsType<string>(token);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var request = new LoginDto { Username = "TestUser", Password = "wrongpassword" };
            _mockUnitOfWork.Setup(uow => uow.Users.GetUserByUsernameAsync(request.Username)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
