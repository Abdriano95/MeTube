using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MeTube.Client.Models;
using MeTube.Client.ViewModels.ManageUsersViewModels;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MeTube.DTO;

namespace MeTube.Test.ViewModels
{
    public class ManageUsersViewModelTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly MockNavigationManager _mockNavigation;
        private readonly ManageUsersViewModel _viewModel;

        public ManageUsersViewModelTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockJsRuntime = new Mock<IJSRuntime>();

            // 🟢 Använd den uppdaterade mockade NavigationManager
            _mockNavigation = new MockNavigationManager();

            _viewModel = new ManageUsersViewModel(_mockUserService.Object, _mockJsRuntime.Object, _mockNavigation);
        }

        [Fact]
        public async Task LoadUsers_ShouldPopulateUserLists()
        {
            var mockUsers = new List<User>
            {
                new User { Username = "Alice", Email = "alice@example.com" },
                new User { Username = "Bob", Email = "bob@example.com" }
            };

            _mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            await _viewModel.LoadUsers();

            Assert.Equal(2, _viewModel.AllUsers.Count);
            Assert.Equal(2, _viewModel.FilteredUsers.Count);
        }

        [Fact]
        public async Task EditUserButton_ShouldSetSelectedUserAndShowUserCard()
        {
            var user = new User { Username = "Alice", Email = "alice@example.com" };
            _mockUserService.Setup(s => s.GetUserIdByEmailAsync(user.Email)).ReturnsAsync(1);

            await _viewModel.EditUserButton(user);

            Assert.True(_viewModel.ShowUserCard);
            Assert.Equal(user, _viewModel.SelectedUser);
            Assert.Equal(1, _viewModel.ChosenUserId);
        }

     //   [Fact]
     //   public async Task DeleteUserButton_ShouldDeleteUser_WhenConfirmed()
     //   {
     //       var user = new User { Username = "TestUser", Email = "test@example.com" };

     //       _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
     //           .ReturnsAsync(true);

     //       // 🟢 Fix för `InvokeVoidAsync`
     //       _mockJsRuntime.Setup(js => js.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<object[]>()))
     //.Callback<string, object[]>((method, args) => { /* Gör ingenting, bara fånga upp anropet */ })
     //.Returns(ValueTask.CompletedTask);

     //       _mockUserService.Setup(s => s.DeleteUserAsync(It.IsAny<int>())).ReturnsAsync(true);

     //       // Act
     //       await _viewModel.DeleteUserButton(user);

     //       // Assert
     //       _mockJsRuntime.Verify(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()), Times.Once);
     //       _mockUserService.Verify(s => s.DeleteUserAsync(It.IsAny<int>()), Times.Once);
     //       _mockJsRuntime.Verify(js => js.InvokeVoidAsync("alert", It.IsAny<object[]>()), Times.Once);
     //   }

        //[Fact]
        //public async Task SaveChangesButton_ShouldUpdateUser_WhenConfirmed()
        //{
        //    var user = new User { Username = "Alice", Email = "alice@example.com", Password = "password", Role = "User" };
        //    _viewModel.AllUsers.Add(user);
        //    _viewModel.ChosenUserId = 1;

        //    _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("confirm", "You sure you want to update this user?")).ReturnsAsync(true);
        //    _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<int>(), It.IsAny<UpdateUserDto>())).ReturnsAsync(true);
        //    _mockJsRuntime.Setup(js => js.InvokeVoidAsync(It.IsAny<string>(), It.IsAny<object[]>())).Returns(ValueTask.CompletedTask);

        //    await _viewModel.SaveChangesButton(user);

        //    _mockJsRuntime.Verify(js => js.InvokeVoidAsync("alert", "User succesfully saved!"), Times.Once);
        //}

        [Fact]
        public void SearchButton_ShouldFilterUsers()
        {
            var user1 = new User { Username = "Alice", Email = "alice@example.com", Password = "123", Role = "User" };
            var user2 = new User { Username = "Bob", Email = "bob@example.com", Password = "456", Role = "Admin" };

            _viewModel.FilteredUsers = new ObservableCollection<User> { user1, user2 };

            _viewModel.Search = "Alice";
            _viewModel.SearchButton();

            Assert.Single(_viewModel.AllUsers);
            Assert.Equal("Alice", _viewModel.AllUsers.First().Username);
        }

        [Fact]
        public void CreateUserAccount_ShouldNavigateToSignup()
        {
            _viewModel.CreateUserAccount();

            // Assert
            Assert.Equal("/signup", _mockNavigation.LastUri);
        }
    }

    public class MockNavigationManager : NavigationManager
    {
        public string LastUri { get; private set; } = string.Empty;

        public MockNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            LastUri = uri; // Spara den senaste navigeringen för verifiering i tester
        }
    }
}
