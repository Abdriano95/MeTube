using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.JSInterop;
using Moq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MeTube.Test.ClientServices
{
    public class UserServiceTests
    {
        private readonly Mock<IClientService> _mockClientService;
        private readonly Mock<IJSRuntimeWrapper> _mockJsRuntime;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockClientService = new Mock<IClientService>();
            _mockJsRuntime = new Mock<IJSRuntimeWrapper>();
            _mockHttpClient = new Mock<HttpClient>();
            _userService = new UserService(_mockClientService.Object, _mockJsRuntime.Object, _mockHttpClient.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnTrue_WhenUserIsRegistered()
        {
            var user = new User();
            _mockClientService.Setup(x => x.RegisterUserAsync(user)).ReturnsAsync(true);

            var result = await _userService.RegisterUserAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            var users = new List<User> { new User() };
            _mockClientService.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await _userService.GetAllUsersAsync();

            Assert.Equal(users, result);
        }

        [Fact]
        public async Task GetUserIdByEmailAsync_ShouldReturnUserId()
        {
            var email = "test@example.com";
            _mockClientService.Setup(x => x.GetUserIdByEmailAsync(email)).ReturnsAsync(1);

            var result = await _userService.GetUserIdByEmailAsync(email);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUser_WhenLoginIsSuccessful()
        {
            var username = "test";
            var password = "password";
            var loginResponse = new LoginResponse { User = new User() };
            _mockClientService.Setup(x => x.LoginAsync(username, password)).ReturnsAsync(loginResponse);

            var result = await _userService.LoginAsync(username, password);

            Assert.Equal(loginResponse.User, result);
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnTrue_WhenLogoutIsSuccessful()
        {
            _mockClientService.Setup(x => x.LogoutAsync()).ReturnsAsync(true);
            _mockJsRuntime.Setup(x => x.InvokeVoidAsync("localStorage.removeItem", "jwtToken")).Returns(ValueTask.CompletedTask);

            var result = await _userService.LogoutAsync();

            Assert.True(result);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldReturnToken_WhenLoginIsSuccessful()
        {
            var username = "test";
            var password = "password";
            var token = "token";
            var loginResponse = new LoginResponse { Token = token };
            _mockClientService.Setup(x => x.LoginAsync(username, password)).ReturnsAsync(loginResponse);

            var result = await _userService.GetTokenAsync(username, password);

            Assert.Equal(token, result);
        }

        [Fact]
        public async Task IsUserAuthenticated_ShouldReturnAuthenticationStatus()
        {
            // Create a valid JWT token
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>("localStorage.getItem", "jwtToken")).ReturnsAsync(token);

            var result = await _userService.IsUserAuthenticated();

            Assert.Equal("true", result["IsAuthenticated"]);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserIsDeleted()
        {
            var userId = 1;
            _mockClientService.Setup(x => x.DeleteUser(userId)).ReturnsAsync(true);

            var result = await _userService.DeleteUserAsync(userId);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnTrue_WhenUserIsUpdated()
        {
            var userId = 1;
            var updateUserDto = new UpdateUserDto();
            _mockClientService.Setup(x => x.UpdateUserAsync(userId, updateUserDto)).ReturnsAsync(true);

            var result = await _userService.UpdateUserAsync(userId, updateUserDto);

            Assert.True(result);
        }

        [Fact]
        public async Task DoesUserExistAsync_ShouldReturnUserExistenceStatus()
        {
            var username = "test";
            var email = "test@example.com";
            var userData = new Dictionary<string, string> { { "username", username }, { "email", email } };
            var response = new Dictionary<string, string> { { "Exists", "true" } };
            _mockClientService.Setup(x => x.DoesUserExistAsync(userData)).ReturnsAsync(response);

            var result = await _userService.DoesUserExistAsync(username, email);

            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser()
        {
            var userId = 1;
            var user = new User();
            _mockClientService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(user);

            var result = await _userService.GetUserByIdAsync(userId);

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task GetAllUsersDetailsAsync_ShouldReturnUserDetails()
        {
            var userDetails = new List<UserDetails> { new UserDetails() };
            _mockClientService.Setup(x => x.GetAllUsersDetailsAsync()).ReturnsAsync(userDetails);

            var result = await _userService.GetAllUsersDetailsAsync();

            Assert.Equal(userDetails, result);
        }
    }
}
