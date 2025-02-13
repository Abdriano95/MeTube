using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.HistoryViewModels;
using Moq;
using Xunit;
using System.Collections.ObjectModel;

namespace MeTube.Test.ViewModels
{
    public class AdminHistoryViewModelTests
    {
        private readonly Mock<IAdminHistoryService> _mockAdminHistoryService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly AdminHistoryViewModel _viewModel;

        public AdminHistoryViewModelTests()
        {
            _mockAdminHistoryService = new Mock<IAdminHistoryService>();
            _mockUserService = new Mock<IUserService>();
            _mockVideoService = new Mock<IVideoService>();

            _viewModel = new AdminHistoryViewModel(
                _mockAdminHistoryService.Object,
                _mockUserService.Object,
                _mockVideoService.Object
            );
        }

        // ------------------------------------------------------------------
        // LoadHistoriesAsync TESTS
        // ------------------------------------------------------------------

        [Fact]
        public async Task LoadHistoriesAsync_WithValidResponse_ShouldFillHistories()
        {
            // Arrange
            var historyList = new List<HistoryAdmin>
            {
                new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1, VideoTitle = "Test Video" }
            };

            _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(It.IsAny<int>()))
                                    .ReturnsAsync(historyList);

            _viewModel.SelectedUserId = 1;

            // Act
            await _viewModel.LoadHistoriesAsync();

            // Assert
            Assert.Single(_viewModel.Histories);
            Assert.Equal("Test Video", _viewModel.Histories.First().VideoTitle);
            Assert.True(string.IsNullOrEmpty(_viewModel.ErrorMessage));
        }

        [Fact]
        public async Task LoadHistoriesAsync_WithNoUserSelected_ShouldSetInfoMessage()
        {
            // Act
            await _viewModel.LoadHistoriesAsync();

            // Assert
            Assert.Equal("Please select a user first!", _viewModel.InfoMessage);
        }

        [Fact]
        public async Task LoadHistoriesAsync_WithEmptyResponse_ShouldSetInfoMessage()
        {
            // Arrange
            _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(It.IsAny<int>()))
                                    .ReturnsAsync(new List<HistoryAdmin>());

            _viewModel.SelectedUserId = 1;

            // Act
            await _viewModel.LoadHistoriesAsync();

            // Assert
            Assert.Empty(_viewModel.Histories);
            Assert.Contains("No history found", _viewModel.InfoMessage);
        }

        [Fact]
        public async Task LoadHistoriesAsync_WhenUserSelected_ShouldPopulateHistories()
        {
            // Arrange
            _viewModel.SelectedUserId = 1;
            var histories = new List<HistoryAdmin>
                            {
                                new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 }
                            };

            _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(1)).ReturnsAsync(histories);

            // Act
            await _viewModel.LoadHistoriesAsync();

            // Assert
            Assert.Single(_viewModel.Histories);
            Assert.Equal(1, _viewModel.Histories[0].Id);
        }


        // ------------------------------------------------------------------
        // LoadAllUsersAndVideosAsync TESTS
        // ------------------------------------------------------------------

        [Fact]
        public async Task LoadAllUsersAndVideosAsync_ShouldLoadUsersAndVideos()
        {
            // Arrange
            var users = new List<UserDetails>
            {
                new UserDetails { Id = 1, Username = "User1" },
                new UserDetails { Id = 2, Username = "User2" }
            };

            var videos = new List<Video>
            {
                new Video { Id = 1, Title = "Video1" },
                new Video { Id = 2, Title = "Video2" }
            };

            _mockUserService.Setup(s => s.GetAllUsersDetailsAsync()).ReturnsAsync(users);
            _mockVideoService.Setup(s => s.GetAllVideosAsync()).ReturnsAsync(videos);

            // Act
            await _viewModel.LoadAllUsersAndVideosAsync();

            // Assert
            Assert.Equal(2, _viewModel.Users.Count);
            Assert.Equal(2, _viewModel.Videos.Count);
            Assert.Empty(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadAllUsersAndVideosAsync_WithException_ShouldSetErrorMessage()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllUsersDetailsAsync()).ThrowsAsync(new Exception("User API error"));

            // Act
            await _viewModel.LoadAllUsersAndVideosAsync();

            // Assert
            Assert.Contains("Error loading users/videos", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadAllUsersAndVideosAsync_WhenSuccessful_ShouldPopulateUsersAndVideos()
        {
            // Arrange
            var users = new List<UserDetails> { new UserDetails { Id = 1, Username = "User1" } };
            var videos = new List<Video> { new Video { Id = 1, Title = "Video1" } };

            _mockUserService.Setup(s => s.GetAllUsersDetailsAsync()).ReturnsAsync(users);
            _mockVideoService.Setup(s => s.GetAllVideosAsync()).ReturnsAsync(videos);

            // Act
            await _viewModel.LoadAllUsersAndVideosAsync();

            // Assert
            Assert.Single(_viewModel.Users);
            Assert.Single(_viewModel.Videos);
            Assert.Equal("User1", _viewModel.Users[0].Username);
            Assert.Equal("Video1", _viewModel.Videos[0].Title);
        }


        // ------------------------------------------------------------------
        // CreateHistoryAsync TESTS
        // ------------------------------------------------------------------
        [Fact]
        public async Task CreateHistoryAsync_WhenServiceFails_ShouldSetErrorMessage()
        {
            // Arrange
            _viewModel.SelectedUserId = 1;
            _viewModel.SelectedVideoId = 1;
            _mockAdminHistoryService
                .Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
                .ReturnsAsync((HistoryAdmin)null); // Simulera att tjänsten misslyckas

            // Act
            await _viewModel.CreateHistoryAsync();

            // Assert
            Assert.Equal("Failed to create history. Check logs or server response.", _viewModel.ErrorMessage);
            Assert.Empty(_viewModel.Histories); // Ingen historia ska ha lagts till
        }

        [Fact]
        public async Task CreateHistoryAsync_WhenSuccessful_ShouldAddHistoryToCollection()
        {
            // Arrange
            var newHistory = new HistoryAdmin { UserId = 1, VideoId = 1 };
            _viewModel.SelectedUserId = 1;
            _viewModel.SelectedVideoId = 1;

            _mockAdminHistoryService
                .Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
                .ReturnsAsync(newHistory);

            // Act
            await _viewModel.CreateHistoryAsync();

            // Assert
            Assert.Contains(_viewModel.Histories, h => h.UserId == 1 && h.VideoId == 1);
        }


        [Fact]
        public async Task CreateHistoryAsync_WhenExceptionThrown_ShouldSetErrorMessage()
        {
            // Arrange
            _viewModel.SelectedUserId = 1;
            _viewModel.SelectedVideoId = 1;

            _mockAdminHistoryService
                .Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
                .ThrowsAsync(new Exception("Unexpected error")); // Simulera en exception

            // Act
            await _viewModel.CreateHistoryAsync();

            // Assert
            Assert.Contains("Error creating history: Unexpected error", _viewModel.ErrorMessage);
        }




        [Fact]
        public async Task CreateHistoryAsync_WithValidationError_ShouldNotCallService()
        {
            // Arrange
            _viewModel.SelectedUserId = 0;  // Ingen användare vald
            _viewModel.SelectedVideoId = 0; // Ingen video vald

            // Act
            await _viewModel.CreateHistoryAsync();

            // Assert
            _mockAdminHistoryService.Verify(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()), Times.Never);
            Assert.Equal("Please select both a user and a video.", _viewModel.ErrorMessage);
        }


        // ------------------------------------------------------------------
        // DeleteHistoryAsync TESTS
        // ------------------------------------------------------------------

        [Fact]
        public async Task DeleteHistoryAsync_WithValidResponse_ShouldRemoveFromHistories()
        {
            // Arrange
            var historyToDelete = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
            _viewModel.Histories.Add(historyToDelete);

            _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(It.IsAny<int>()))
                                    .ReturnsAsync(true);

            // Act
            await _viewModel.DeleteHistoryAsync(historyToDelete);

            // Assert
            Assert.Empty(_viewModel.Histories);
        }

        [Fact]
        public async Task DeleteHistoryAsync_WithFailedResponse_ShouldSetErrorMessage()
        {
            // Arrange
            var historyToDelete = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };

            _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(It.IsAny<int>()))
                                    .ReturnsAsync(false);

            // Act
            await _viewModel.DeleteHistoryAsync(historyToDelete);

            // Assert
            Assert.Contains("Failed to delete history", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task DeleteHistoryAsync_WhenSuccessful_ShouldRemoveHistoryFromList()
        {
            // Arrange
            var history = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
            _viewModel.Histories.Add(history);

            _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(1)).ReturnsAsync(true);

            // Act
            await _viewModel.DeleteHistoryAsync(history);

            // Assert
            Assert.Empty(_viewModel.Histories);
        }


        // ------------------------------------------------------------------
        // UpdateHistoryAsync TESTS
        // 
        [Fact]
        public async Task UpdateHistoryAsync_WhenSuccessful_ShouldUpdateHistoryInList()
        {
            // Arrange
            var history = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1, VideoTitle = "Old Title" };
            _viewModel.Histories.Add(history);
            _viewModel.EditingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1, VideoTitle = "New Title" };

            _mockAdminHistoryService.Setup(s => s.UpdateHistoryAsync(It.IsAny<HistoryAdmin>())).ReturnsAsync(true);

            // Act
            await _viewModel.UpdateHistoryAsync();

            // Assert
            Assert.Equal("New Title", _viewModel.Histories[0].VideoTitle);
        }



        // ------------------------------------------------------------------
        // EditHistory TESTS
        // ------------------------------------------------------------------

        [Fact]
        public void EditHistory_ShouldSetEditingHistoryAndSelectedIds()
        {
            // Arrange
            var history = new HistoryAdmin { Id = 1, UserId = 2, VideoId = 3, UserName = "TestUser", VideoTitle = "TestVideo" };

            // Act
            _viewModel.EditHistory(history);

            // Assert
            Assert.Equal(1, _viewModel.EditingHistory.Id);
            Assert.Equal(2, _viewModel.SelectedUserId);
            Assert.Equal(3, _viewModel.SelectedVideoId);
        }

    }
}
