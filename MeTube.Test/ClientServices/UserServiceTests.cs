using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.JSInterop;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MeTube.Test.ClientServices
{
    public class UserServiceTests
    {
        private readonly Mock<IClientService> _mockClientService;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HttpClient _httpClient;
        private readonly UserService _userService;
        private ClientService _clientService;


        public UserServiceTests()
        {
            _mockClientService = new Mock<IClientService>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            _mockMapper = new Mock<IMapper>();
            _httpClient = new HttpClient();

            //_userService = new UserService(_mockClientService.Object, _mockJsRuntime.Object, _httpClient);
            _clientService = new ClientService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);


            // ✅ Mocka att en JWT-token finns i `localStorage`
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(
        It.IsAny<string>(),
        It.IsAny<object[]>()))
    .ReturnsAsync("fake-jwt-token");
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnTrue_WhenRegistrationIsSuccessful()
        {
            var user = new User { Username = "NewUser", Email = "new@example.com", Password = "password" };

            _mockClientService.Setup(cs => cs.RegisterUserAsync(user)).ReturnsAsync(true);

            var result = await _userService.RegisterUserAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            var username = "TestUser";
            var password = "password";

            var expectedUser = new User
            {
                Username = username,
                Email = "test@example.com",
                Password = password,
                Role = "User"
            };

            var loginResponse = new LoginResponse
            {
                User = expectedUser,
                Token = "fake-jwt-token"
            };

            // Mockar ClientService.LoginAsync så att den returnerar en LoginResponse
            _mockClientService.Setup(cs => cs.LoginAsync(username, password))
                              .ReturnsAsync(loginResponse);

            // Act
            var result = await _userService.LoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);

            // Verifiera att LoginAsync verkligen anropades
            _mockClientService.Verify(cs => cs.LoginAsync(username, password), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var username = "TestUser";
            var password = "wrongpassword";

            // Mockar så att felaktiga inloggningsuppgifter returnerar null
            _mockClientService.Setup(cs => cs.LoginAsync(username, password))
                              .ReturnsAsync((LoginResponse?)null);

            // Act
            var result = await _userService.LoginAsync(username, password);

            // Assert
            Assert.Null(result);

            // Verifiera att LoginAsync verkligen anropades
            _mockClientService.Verify(cs => cs.LoginAsync(username, password), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnTrue_WhenLogoutIsSuccessful()
        {
            _mockClientService.Setup(cs => cs.LogoutAsync()).ReturnsAsync(true);

            var result = await _userService.LogoutAsync();

            Assert.True(result);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldReturnToken_WhenLoginIsSuccessful()
        {
            var username = "TestUser";
            var password = "password";

            var loginResponse = new LoginResponse
            {
                User = new User { Username = username, Email = "test@example.com" },
                Token = "fake-jwt-token"
            };

            _mockClientService.Setup(cs => cs.LoginAsync(username, password))
                              .ReturnsAsync(loginResponse);

            var result = await _userService.GetTokenAsync(username, password);

            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserIsDeleted()
        {
            var userId = 1;

            _mockClientService.Setup(cs => cs.DeleteUser(userId)).ReturnsAsync(true);

            var result = await _userService.DeleteUserAsync(userId);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnTrue_WhenUserIsUpdated()
        {
            var userId = 1;
            var updateUserDto = new UpdateUserDto { Username = "UpdatedUser", Email = "updated@example.com" };

            _mockClientService.Setup(cs => cs.UpdateUserAsync(userId, updateUserDto)).ReturnsAsync(true);

            var result = await _userService.UpdateUserAsync(userId, updateUserDto);

            Assert.True(result);
        }
    }
}
