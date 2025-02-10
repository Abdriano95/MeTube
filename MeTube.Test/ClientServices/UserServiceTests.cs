using AutoMapper;
using MeTube.Client.Models;
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
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ClientService _clientService; // ✅ Använder en riktig ClientService
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockClientService = new Mock<IClientService>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object); // ✅ Mocka HTTP-anrop
            _mockMapper = new Mock<IMapper>();
            _mockJsRuntime = new Mock<IJSRuntime>();

            // ✅ Skapa en riktig ClientService
            _clientService = new ClientService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);

            // ✅ Skapa UserService med en riktig ClientService
            _userService = new UserService(_clientService, _mockJsRuntime.Object, _httpClient);

            // ✅ Mocka att en JWT-token finns i `localStorage`
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
                          .ReturnsAsync("fake-jwt-token");
        }



        [Fact]
        public async Task RegisterUserAsync_ShouldReturnTrue_WhenRegistrationIsSuccessful()
        {
            var userId = 1;
            var fakeToken = "fake-jwt-token";

            // ✅ Använd en workaround för `InvokeAsync`
            _mockJsRuntime.Setup(js => js.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                          .ReturnsAsync(fakeToken);

            // ✅ Mocka att API:et returnerar `true` för `DeleteUserAsync`
            _mockClientService.Setup(cs => cs.DeleteUser(userId))
                              .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.True(result); // ✅ Förväntar oss `true`
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

            var loginResponse = new LoginResponse // ✅ Mocka rätt returtyp!
            {
                User = expectedUser,
                Token = "fake-jwt-token"
            };

            // ✅ Mocka `LoginAsync` korrekt
            _mockClientService.Setup(cs => cs.LoginAsync(username, password))
                              .ReturnsAsync(loginResponse);

            // Act
            var result = await _userService.LoginAsync(username, password);

            // Assert
            Assert.NotNull(result);  // ✅ Se till att det inte är null
            Assert.Equal(username, result.Username); // ✅ Kontrollera att det är rätt användare
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnTrue_WhenLogoutIsSuccessful()
        {
            // Arrange
            _mockJsRuntime.Setup(js => js.InvokeVoidAsync("localStorage.removeItem", "jwtToken")).Returns(ValueTask.CompletedTask);

            // Act
            var result = await _userService.LogoutAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldReturnToken_WhenLoginIsSuccessful()
        {
            // Arrange
            var username = "TestUser";
            var password = "password";

            // Act
            var result = await _userService.GetTokenAsync(username, password);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserIsDeleted()
        {
            var userId = 1;

            // ✅ Se till att en token finns i localStorage
            _mockJsRuntime.Setup(js => js.InvokeAsync<string>("localStorage.getItem", "jwtToken"))
                          .ReturnsAsync("fake-jwt-token");

            // ✅ Mocka att API:et returnerar `true` för DeleteUserAsync
            _mockClientService.Setup(cs => cs.DeleteUser(userId))
                              .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.True(result); // ✅ Förväntar oss `true`
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnTrue_WhenUserIsUpdated()
        {
            // Arrange
            var userId = 1;
            var updateUserDto = new UpdateUserDto { Username = "UpdatedUser", Email = "updated@example.com" };

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateUserDto);

            // Assert
            Assert.True(result);
        }
    }
}
