using System.Threading.Tasks;
using Moq;
using Xunit;
using MeTube.Client.Models;
using MeTube.Client.ViewModels.SignupViewModels;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;

namespace MeTube.Test.ViewModels
{
    public class SignupViewModelTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly Mock<NavigationManager> _mockNavigation;
        private readonly SignupViewModel _viewModel;

        public SignupViewModelTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            _mockNavigation = new Mock<NavigationManager>();

            _viewModel = new SignupViewModel(_mockUserService.Object, _mockJsRuntime.Object, _mockNavigation.Object);

            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(
                It.IsAny<string>(),
                It.IsAny<object[]>()))
                .ReturnsAsync("fake-jwt-token");
        }

        [Fact]
        public async Task SignupButton_ShouldFail_WhenUsernameOrPasswordOrEmailIsInvalid()
        {
            _viewModel.Username = "ab";  // För kort
            _viewModel.Password = "12";  // För kort
            _viewModel.Email = "invalid-email"; // Fel format

            await _viewModel.SignupButton();

            Assert.False(string.IsNullOrEmpty(_viewModel.UsernameError));
            Assert.False(string.IsNullOrEmpty(_viewModel.PasswordError));
            Assert.False(string.IsNullOrEmpty(_viewModel.EmailError));
        }

        [Fact]
        public async Task SignupButton_ShouldFail_WhenUserAlreadyExists()
        {
            _viewModel.Username = "ExistingUser";
            _viewModel.Email = "existing@example.com";
            _viewModel.Password = "validPassword";

            var existingUserResponse = new Dictionary<string, string>
            {
                { "True", "User already exists" }
            };

            _mockUserService.Setup(u => u.DoesUserExistAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(existingUserResponse);

            await _viewModel.SignupButton();

            _mockJsRuntime.Verify(js => js.InvokeAsync<bool>("alert", It.IsAny<object[]>()), Times.Once);
        }
    }
}
