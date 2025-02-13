using System.Threading.Tasks;
using Moq;
using Xunit;
using MeTube.Client.Models;
using MeTube.Client.ViewModels.LoginViewModels;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MeTube.Test.ViewModels
{
    public class UserLoginViewModelTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly LoginViewModel _viewModel;
        private readonly Mock<NavigationManager> _mockNavigation;

        public UserLoginViewModelTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockJsRuntime = new Mock<IJSRuntime>();

            _viewModel = new LoginViewModel(_mockUserService.Object, _mockAuthService.Object, _mockJsRuntime.Object, null);

            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(
                It.IsAny<string>(),
                It.IsAny<object[]>()))
                .ReturnsAsync("fake-jwt-token");
        }

        [Fact]
        public async Task LoginButton_ShouldFail_WhenUsernameOrPasswordIsInvalid()
        {
            _viewModel.Username = "a";
            _viewModel.Password = "ab";

            await _viewModel.LoginButton();

            Assert.False(string.IsNullOrEmpty(_viewModel.UsernameError));
            Assert.False(string.IsNullOrEmpty(_viewModel.PasswordError));
        }

        [Fact]
        public async Task LoginButton_ShouldFail_WhenUserNotFound()
        {
            _viewModel.Username = "validUser";
            _viewModel.Password = "validPass";

            _mockUserService.Setup(u => u.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            await _viewModel.LoginButton();

            // Assert
            _mockJsRuntime.Verify(js => js.InvokeAsync<object>("alert", It.IsAny<object[]>()), Times.Once);
            Assert.False(_viewModel.IsUserLoggedIn);
        }
    }
}