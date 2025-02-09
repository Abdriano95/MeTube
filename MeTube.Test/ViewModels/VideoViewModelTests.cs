using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.VideoViewModels;
using Microsoft.AspNetCore.Components;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeTube.Test.ViewModels
{
    public class VideoViewModelTests
    {
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly Mock<IHistoryService> _mockHistoryService;

        private readonly Mock<ILikeService> _mockLikeService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly VideoViewModel _viewModel;

        public VideoViewModelTests()
        {
            _mockVideoService = new Mock<IVideoService>();
            _mockLikeService = new Mock<ILikeService>();
            _mockHistoryService = new Mock<IHistoryService>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _viewModel = new VideoViewModel(_mockVideoService.Object, _mockLikeService.Object, _mockNavigationManager.Object, _mockHistoryService.Object);
        }

        [Fact]
        public async Task LoadVideoAsync_LoadsVideoAndLikeInfo()
        {
            // Arrange
            var video = new Video { Id = 1, Title = "Test Video" };
            _mockVideoService.Setup(s => s.GetVideoByIdAsync(1)).ReturnsAsync(video);
            _mockLikeService.Setup(s => s.HasUserLikedVideoAsync(1)).ReturnsAsync(true);
            _mockLikeService.Setup(s => s.GetLikeCountForVideoAsync(1)).ReturnsAsync(5);

            // Act
            await _viewModel.LoadVideoAsync(1);

            // Assert
            Assert.Equal(video, _viewModel.CurrentVideo);
            Assert.True(_viewModel.HasUserLiked);
            Assert.Equal(5, _viewModel.LikeCount);
            Assert.False(_viewModel.IsLoading);
            Assert.Empty(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadVideoAsync_HandlesError()
        {
            // Arrange
            _mockVideoService.Setup(s => s.GetVideoByIdAsync(1))
                .ThrowsAsync(new Exception("Test error"));

            // Act
            await _viewModel.LoadVideoAsync(1);

            // Assert
            Assert.Null(_viewModel.CurrentVideo);
            Assert.Contains("Test error", _viewModel.ErrorMessage);
            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task ToggleLikeCommand_AddLike_WhenNotLiked()
        {
            // Arrange
            _viewModel.CurrentVideo = new Video { Id = 1 };
            _viewModel.HasUserLiked = false;
            _viewModel.LikeCount = 5;
            _mockLikeService.Setup(s => s.AddLikeAsync(1)).ReturnsAsync(true);

            // Act
            await _viewModel.ToggleLikeCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.HasUserLiked);
            Assert.Equal(6, _viewModel.LikeCount);
            _mockLikeService.Verify(s => s.AddLikeAsync(1), Times.Once);
        }

        [Fact]
        public async Task ToggleLikeCommand_RemoveLike_WhenLiked()
        {
            // Arrange
            _viewModel.CurrentVideo = new Video { Id = 1 };
            _viewModel.HasUserLiked = true;
            _viewModel.LikeCount = 5;
            _mockLikeService.Setup(s => s.RemoveLikeAsync(1)).ReturnsAsync(true);

            // Act
            await _viewModel.ToggleLikeCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.HasUserLiked);
            Assert.Equal(4, _viewModel.LikeCount);
            _mockLikeService.Verify(s => s.RemoveLikeAsync(1), Times.Once);
        }

        [Fact]
        public async Task ToggleLikeCommand_HandlesError()
        {
            // Arrange
            _viewModel.CurrentVideo = new Video { Id = 1 };
            _mockLikeService.Setup(s => s.AddLikeAsync(1))
                .ThrowsAsync(new Exception("Test error"));

            // Act
            await _viewModel.ToggleLikeCommand.ExecuteAsync(null);

            // Assert
            // Assert
            Assert.NotNull(_viewModel.ErrorMessage);
            Assert.Contains("Test error", _viewModel.ErrorMessage);
        }

    }
}
