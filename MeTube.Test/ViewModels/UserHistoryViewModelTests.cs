using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.HistoryViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Test.ViewModels
{
    public class UserHistoryViewModelTests
    {
        private readonly Mock<IHistoryService> _mockHistoryService;
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<UserHistoryViewModel> _userHistoryViewModel;
        private readonly Mock<IJSRuntime> _mockIJSRuntime;
        public UserHistoryViewModelTests()
        {
            _mockHistoryService = new Mock<IHistoryService>();
            _mockVideoService = new Mock<IVideoService>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockIJSRuntime = new Mock<IJSRuntime>();
            _userHistoryViewModel = new Mock<UserHistoryViewModel>(_mockHistoryService.Object, _mockNavigationManager.Object, _mockVideoService.Object, _mockIJSRuntime.Object);
        }

        [Fact]
        public async Task LoadUserHistoryAsync_LoadsUserHistory()
        {
            // Arrange
            var history = new List<History>
            {
                new History { VideoId = 1, VideoTitle = "Test Video", DateWatched = DateTime.Now }
            };
            _mockHistoryService.Setup(s => s.GetUserHistoryAsync()).ReturnsAsync(history);
            // Act
            await _userHistoryViewModel.Object.LoadUserHistoryAsync();
            // Assert
            Assert.Single(_userHistoryViewModel.Object.UserHistory);
            Assert.Equal(history.First().VideoTitle, _userHistoryViewModel.Object.UserHistory.First().VideoTitle);
        }

        [Fact]
        public async Task LoadUserHistoryAsync_HandlesError()
        {
            // Arrange
            _mockHistoryService.Setup(s => s.GetUserHistoryAsync())
                .ThrowsAsync(new Exception("Test error"));
            // Act
            await _userHistoryViewModel.Object.LoadUserHistoryAsync();
            // Assert
            Assert.Empty(_userHistoryViewModel.Object.UserHistory);
        }

        [Fact]
        public async Task LoadUserHistoryAsync_HandlesNullHistory()
        {
            // Arrange
            _mockHistoryService.Setup(s => s.GetUserHistoryAsync())
                .ReturnsAsync((List<History>)null);
            // Act
            await _userHistoryViewModel.Object.LoadUserHistoryAsync();
            // Assert
            Assert.Empty(_userHistoryViewModel.Object.UserHistory);
        }

        [Fact]
        public async Task LoadUserHistoryAsync_HandlesEmptyHistory()
        {
            // Arrange
            _mockHistoryService.Setup(s => s.GetUserHistoryAsync())
                .ReturnsAsync(new List<History>());
            // Act
            await _userHistoryViewModel.Object.LoadUserHistoryAsync();
            // Assert
            Assert.Empty(_userHistoryViewModel.Object.UserHistory);
        }

        [Fact]
        public async Task LoadUserHistoryAsync_SortsHistoryByDateWatched()
        {
            // Arrange
            var history = new List<History>
            {
                new History { VideoId = 1, VideoTitle = "Test Video 1", DateWatched = DateTime.Now.AddDays(-1) },
                new History { VideoId = 2, VideoTitle = "Test Video 2", DateWatched = DateTime.Now.AddDays(-2) },
                new History { VideoId = 3, VideoTitle = "Test Video 3", DateWatched = DateTime.Now.AddDays(-3) }
            };
            _mockHistoryService.Setup(s => s.GetUserHistoryAsync()).ReturnsAsync(history);
            // Act
            await _userHistoryViewModel.Object.LoadUserHistoryAsync();
            // Assert
            Assert.Equal("Test Video 1", _userHistoryViewModel.Object.UserHistory[0].VideoTitle);
            Assert.Equal("Test Video 2", _userHistoryViewModel.Object.UserHistory[1].VideoTitle);
            Assert.Equal("Test Video 3", _userHistoryViewModel.Object.UserHistory[2].VideoTitle);
        }

        [Fact]
        public async Task LoadUserHistoryAsync_HandlesException()
        {
            // Arrange
            _mockHistoryService.Setup(s => s.GetUserHistoryAsync())
                .ThrowsAsync(new Exception("Test error"));
            // Act
            await _userHistoryViewModel.Object.LoadUserHistoryAsync();
            // Assert
            Assert.Empty(_userHistoryViewModel.Object.UserHistory);
        }
    }
}
