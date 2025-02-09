using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.HistoryViewModels;
using Moq;
using Xunit;

namespace MeTube.Test.ViewModels
{
    public class AdminHistoryViewModelTests
    {
        //    private readonly Mock<IAdminHistoryService> _mockAdminHistoryService;
        //    private readonly AdminHistoryViewModel _viewModel;

        //    public AdminHistoryViewModelTests()
        //    {
        //        _mockAdminHistoryService = new Mock<IAdminHistoryService>();
        //        _viewModel = new AdminHistoryViewModel(_mockAdminHistoryService.Object);
        //    }

        //    [Fact]
        //    public async Task LoadHistoriesAsync_WithValidResponse_ShouldFillHistories()
        //    {
        //        // Arrange
        //        var historyList = new List<HistoryAdmin>
        //        {
        //            new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1, VideoTitle = "Test Video" }
        //        };

        //        _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(It.IsAny<int>()))
        //                                .ReturnsAsync(historyList);

        //        _viewModel.SelectedUserId = 1;

        //        // Act
        //        await _viewModel.LoadHistoriesAsync();

        //        // Assert
        //        Assert.Single(_viewModel.Histories);
        //        Assert.Equal("Test Video", _viewModel.Histories.First().VideoTitle);
        //        Assert.True(string.IsNullOrEmpty(_viewModel.ErrorMessage));
        //    }

        //    [Fact]
        //    public async Task LoadHistoriesAsync_WithEmptyResponse_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(It.IsAny<int>()))
        //                                .ReturnsAsync(new List<HistoryAdmin>());

        //        _viewModel.SelectedUserId = 1;

        //        // Act
        //        await _viewModel.LoadHistoriesAsync();

        //        // Assert
        //        Assert.Empty(_viewModel.Histories);
        //        Assert.Equal("No history found for this user or an error occurred.", _viewModel.ErrorMessage);
        //    }

        //    [Fact]
        //    public async Task LoadHistoriesAsync_WithException_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(It.IsAny<int>()))
        //                                .ThrowsAsync(new Exception("API failure"));

        //        _viewModel.SelectedUserId = 1;

        //        // Act
        //        await _viewModel.LoadHistoriesAsync();

        //        // Assert
        //        Assert.Empty(_viewModel.Histories);
        //        Assert.Contains("Error loading histories: API failure", _viewModel.ErrorMessage);
        //    }

        //    [Fact]
        //    public async Task LoadHistoriesAsync_ShouldSetIsLoading_Correctly()
        //    {
        //        // Arrange
        //        var historyList = new List<HistoryAdmin>
        //        {
        //            new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 }
        //        };

        //        _mockAdminHistoryService.Setup(s => s.GetHistoryByUserAsync(It.IsAny<int>()))
        //                                .Returns(async () =>
        //                                {
        //                                    await Task.Delay(100); 
        //                                    return historyList;
        //                                });

        //        _viewModel.SelectedUserId = 1;

        //        // Act
        //        var loadTask = _viewModel.LoadHistoriesAsync();

        //        // Assert (during execution)
        //        await Task.Delay(50);
        //        Assert.True(_viewModel.IsLoading);

        //        await loadTask;

        //        // Assert (after execution)
        //        Assert.False(_viewModel.IsLoading);
        //    }

        //    [Fact]
        //    public async Task CreateHistoryAsync_WithValidResponse_ShouldAddToHistories()
        //    {
        //        // Arrange
        //        var newHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.EditingHistory = newHistory;

        //        _mockAdminHistoryService.Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ReturnsAsync(newHistory);

        //        // Act
        //        await _viewModel.CreateHistoryAsync();

        //        // Assert
        //        Assert.Single(_viewModel.Histories);
        //        Assert.Equal(1, _viewModel.Histories.First().Id);
        //        Assert.True(string.IsNullOrEmpty(_viewModel.ErrorMessage));
        //    }

        //    [Fact]
        //    public async Task CreateHistoryAsync_WithNullResponse_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        _viewModel.EditingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };

        //        _mockAdminHistoryService.Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ReturnsAsync((HistoryAdmin)null);

        //        // Act
        //        await _viewModel.CreateHistoryAsync();

        //        // Assert
        //        Assert.Equal("Failed to create history. Check logs or server response.", _viewModel.ErrorMessage);
        //        Assert.Empty(_viewModel.Histories);
        //    }

        //    [Fact]
        //    public async Task CreateHistoryAsync_ShouldResetEditingHistory()
        //    {
        //        // Arrange
        //        var newHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.EditingHistory = newHistory;

        //        _mockAdminHistoryService.Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ReturnsAsync(newHistory);

        //        // Act
        //        await _viewModel.CreateHistoryAsync();

        //        // Assert
        //        Assert.NotEqual(newHistory, _viewModel.EditingHistory); // Ska vara en ny instans
        //        Assert.Equal(0, _viewModel.EditingHistory.Id); // Alla fält ska vara nollställda
        //    }

        //    [Fact]
        //    public async Task CreateHistoryAsync_WithException_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        _viewModel.EditingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };

        //        _mockAdminHistoryService.Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ThrowsAsync(new Exception("API failure"));

        //        // Act
        //        await _viewModel.CreateHistoryAsync();

        //        // Assert
        //        Assert.Contains("Error creating history: API failure", _viewModel.ErrorMessage);
        //    }

        //    [Fact]
        //    public async Task CreateHistoryAsync_ShouldSetIsLoading_Correctly()
        //    {
        //        // Arrange
        //        var newHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.EditingHistory = newHistory;

        //        _mockAdminHistoryService.Setup(s => s.CreateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .Returns(async () =>
        //                                {
        //                                    await Task.Delay(100); // Simulera en liten fördröjning
        //                                    return newHistory;
        //                                });

        //        // Act
        //        var createTask = _viewModel.CreateHistoryAsync();

        //        // Assert (under exekvering)
        //        await Task.Delay(50);
        //        Assert.True(_viewModel.IsLoading);

        //        await createTask;

        //        // Assert (efter exekvering)
        //        Assert.False(_viewModel.IsLoading);
        //    }

        //    [Fact]
        //    public async Task UpdateHistoryAsync_WithValidResponse_ShouldUpdateHistories()
        //    {
        //        // Arrange
        //        var existingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1, VideoTitle = "Old Title" };
        //        var updatedHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1, VideoTitle = "New Title" };

        //        _viewModel.Histories.Add(existingHistory);
        //        _viewModel.EditingHistory = updatedHistory;

        //        _mockAdminHistoryService.Setup(s => s.UpdateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ReturnsAsync(true);

        //        // Act
        //        await _viewModel.UpdateHistoryAsync();

        //        // Assert
        //        Assert.Equal("New Title", _viewModel.Histories.First().VideoTitle);
        //        Assert.True(string.IsNullOrEmpty(_viewModel.ErrorMessage));
        //    }

        //    [Fact]
        //    public async Task UpdateHistoryAsync_WithFailedResponse_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        var existingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.Histories.Add(existingHistory);
        //        _viewModel.EditingHistory = existingHistory;

        //        _mockAdminHistoryService.Setup(s => s.UpdateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ReturnsAsync(false);

        //        // Act
        //        await _viewModel.UpdateHistoryAsync();

        //        // Assert
        //        Assert.Equal("Failed to update history. Possibly not found or server error.", _viewModel.ErrorMessage);
        //    }

        //    [Fact]
        //    public async Task UpdateHistoryAsync_ShouldResetEditingHistory()
        //    {
        //        // Arrange
        //        var existingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.Histories.Add(existingHistory);
        //        _viewModel.EditingHistory = existingHistory;

        //        _mockAdminHistoryService.Setup(s => s.UpdateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ReturnsAsync(true);

        //        // Act
        //        await _viewModel.UpdateHistoryAsync();

        //        // Assert
        //        Assert.NotEqual(existingHistory, _viewModel.EditingHistory); // Ska vara en ny instans
        //        Assert.Equal(0, _viewModel.EditingHistory.Id); // Alla fält ska vara nollställda
        //    }

        //    [Fact]
        //    public async Task UpdateHistoryAsync_WithException_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        var existingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.EditingHistory = existingHistory;

        //        _mockAdminHistoryService.Setup(s => s.UpdateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .ThrowsAsync(new Exception("API failure"));

        //        // Act
        //        await _viewModel.UpdateHistoryAsync();

        //        // Assert
        //        Assert.Contains("Error updating history: API failure", _viewModel.ErrorMessage);
        //    }

        //    [Fact]
        //    public async Task UpdateHistoryAsync_ShouldSetIsLoading_Correctly()
        //    {
        //        // Arrange
        //        var existingHistory = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.EditingHistory = existingHistory;

        //        _mockAdminHistoryService.Setup(s => s.UpdateHistoryAsync(It.IsAny<HistoryAdmin>()))
        //                                .Returns(async () =>
        //                                {
        //                                    await Task.Delay(100); // Simulera en liten fördröjning
        //                                    return true;
        //                                });

        //        // Act
        //        var updateTask = _viewModel.UpdateHistoryAsync();

        //        // Assert (under exekvering)
        //        await Task.Delay(50);
        //        Assert.True(_viewModel.IsLoading);

        //        await updateTask;

        //        // Assert (efter exekvering)
        //        Assert.False(_viewModel.IsLoading);
        //    }

        //    [Fact]
        //    public async Task DeleteHistoryAsync_WithValidResponse_ShouldRemoveFromHistories()
        //    {
        //        // Arrange
        //        var historyToDelete = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.Histories.Add(historyToDelete);

        //        _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(It.IsAny<int>()))
        //                                .ReturnsAsync(true);

        //        // Act
        //        await _viewModel.DeleteHistoryAsync(historyToDelete);

        //        // Assert
        //        Assert.Empty(_viewModel.Histories);
        //        Assert.True(string.IsNullOrEmpty(_viewModel.ErrorMessage));
        //    }

        //    [Fact]
        //    public async Task DeleteHistoryAsync_WithFailedResponse_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        var historyToDelete = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
        //        _viewModel.Histories.Add(historyToDelete);

        //        _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(It.IsAny<int>()))
        //                                .ReturnsAsync(false);

        //        // Act
        //        await _viewModel.DeleteHistoryAsync(historyToDelete);

        //        // Assert
        //        Assert.Contains("Failed to delete history. Possibly not found or server error.", _viewModel.ErrorMessage);
        //        Assert.NotEmpty(_viewModel.Histories);
        //    }

        //    [Fact]
        //    public async Task DeleteHistoryAsync_WithException_ShouldSetErrorMessage()
        //    {
        //        // Arrange
        //        var historyToDelete = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };

        //        _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(It.IsAny<int>()))
        //                                .ThrowsAsync(new Exception("API failure"));

        //        // Act
        //        await _viewModel.DeleteHistoryAsync(historyToDelete);

        //        // Assert
        //        Assert.Contains("Error deleting history: API failure", _viewModel.ErrorMessage);
        //    }

        //    [Fact]
        //    public async Task DeleteHistoryAsync_ShouldSetIsLoading_Correctly()
        //    {
        //        // Arrange
        //        var historyToDelete = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };

        //        _mockAdminHistoryService.Setup(s => s.DeleteHistoryAsync(It.IsAny<int>()))
        //                                .Returns(async () =>
        //                                {
        //                                    await Task.Delay(100); // Simulera en liten fördröjning
        //                                    return true;
        //                                });

        //        // Act
        //        var deleteTask = _viewModel.DeleteHistoryAsync(historyToDelete);

        //        // Assert (under exekvering)
        //        await Task.Delay(50);
        //        Assert.True(_viewModel.IsLoading);

        //        await deleteTask;

        //        // Assert (efter exekvering)
        //        Assert.False(_viewModel.IsLoading);
        //    }

        //    [Fact]
        //    public void EditHistory_ShouldCopyHistoryProperties()
        //    {
        //        // Arrange
        //        var history = new HistoryAdmin
        //        {
        //            Id = 1,
        //            UserId = 10,
        //            VideoId = 100,
        //            UserName = "TestUser",
        //            VideoTitle = "Test Video",
        //            DateWatched = DateTime.UtcNow
        //        };

        //        // Act
        //        _viewModel.EditHistory(history);

        //        // Assert
        //        Assert.Equal(history.Id, _viewModel.EditingHistory.Id);
        //        Assert.Equal(history.UserId, _viewModel.EditingHistory.UserId);
        //        Assert.Equal(history.VideoId, _viewModel.EditingHistory.VideoId);
        //        Assert.Equal(history.UserName, _viewModel.EditingHistory.UserName);
        //        Assert.Equal(history.VideoTitle, _viewModel.EditingHistory.VideoTitle);
        //        Assert.Equal(history.DateWatched, _viewModel.EditingHistory.DateWatched);
        //    }

        //    [Fact]
        //    public void EditHistory_ShouldCreateNewInstance()
        //    {
        //        // Arrange
        //        var history = new HistoryAdmin { Id = 1, UserId = 10, VideoId = 100 };

        //        // Act
        //        _viewModel.EditHistory(history);

        //        // Assert
        //        Assert.NotSame(history, _viewModel.EditingHistory);
        //    }

    }
}
