using AutoMapper;
using MeTube.Client;
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
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly VideoViewModel _viewModel;

        public VideoViewModelTests()
        {
            _mockVideoService = new Mock<IVideoService>();
            _mockLikeService = new Mock<ILikeService>();
            _mockHistoryService = new Mock<IHistoryService>();
            _mockUserService = new Mock<IUserService>();
            _mockCommentService = new Mock<ICommentService>();
            _mockMapper = new Mock<IMapper>();
            _mockNavigationManager = new Mock<NavigationManager>();
            _viewModel = new VideoViewModel(_mockVideoService.Object,
                                            _mockLikeService.Object,
                                            _mockCommentService.Object,
                                            _mockUserService.Object as UserService,
                                            _mockMapper.Object,
                                            _mockNavigationManager.Object,
                                            _mockHistoryService.Object);
        }

        [Fact]
        public async Task LoadVideoAsync_WhenVideoExists_SetsPropertiesAndLoadsComments()
        {
            // Arrange
            int videoId = 1;
            var expectedVideo = new Video();
            var expectedUsername = "testUser";
            bool expectedHasLiked = true;
            int expectedLikeCount = 10;
            var expectedComments = new List<Comment>();

            _mockVideoService.Setup(x => x.GetVideoByIdAsync(videoId)).ReturnsAsync(expectedVideo);
            _mockVideoService.Setup(x => x.GetUploaderUsernameAsync(videoId)).ReturnsAsync(expectedUsername);
            _mockLikeService.Setup(x => x.HasUserLikedVideoAsync(videoId)).ReturnsAsync(expectedHasLiked);
            _mockLikeService.Setup(x => x.GetLikeCountForVideoAsync(videoId)).ReturnsAsync(expectedLikeCount);
            _mockCommentService.Setup(x => x.GetCommentsByVideoIdAsync(videoId)).ReturnsAsync(expectedComments);

            // Act
            await _viewModel.LoadVideoAsync(videoId);

            // Assert
            _mockVideoService.Verify(x => x.GetVideoByIdAsync(videoId), Times.Once);
            Assert.Equal(expectedVideo, _viewModel.CurrentVideo);
            Assert.Equal(Constants.VideoStreamUrl(videoId), _viewModel.CurrentVideo.VideoUrl);

            _mockVideoService.Verify(x => x.GetUploaderUsernameAsync(videoId), Times.Once);
            Assert.Equal(expectedUsername, _viewModel.UploaderUsername);

            _mockLikeService.Verify(x => x.HasUserLikedVideoAsync(videoId), Times.Once);
            Assert.Equal(expectedHasLiked, _viewModel.HasUserLiked);

            _mockLikeService.Verify(x => x.GetLikeCountForVideoAsync(videoId), Times.Once);
            Assert.Equal(expectedLikeCount, _viewModel.LikeCount);

            _mockCommentService.Verify(x => x.GetCommentsByVideoIdAsync(videoId), Times.Exactly(2));
            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task LoadVideoAsync_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            int videoId = 1;
            var exception = new Exception("Test error");
            _mockVideoService.Setup(x => x.GetVideoByIdAsync(videoId)).ThrowsAsync(exception);

            // Act
            await _viewModel.LoadVideoAsync(videoId);

            // Assert
            Assert.Equal($"An error occurred: {exception.Message}", _viewModel.ErrorMessage);
            Assert.False(_viewModel.IsLoading);
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
            Assert.NotNull(_viewModel.ErrorMessage);
            Assert.Contains("Test error", _viewModel.ErrorMessage);
        }

    }
}
