using AutoMapper;
using MeTube.Client;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.VideoViewModels;
using MeTube.DTO.CommentDTOs;
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

        [Fact]
        public async Task PostComment_WhenCommentIsValid_AddsComment()
        {
            // Arrange
            _viewModel.CurrentVideo = new Video { Id = 1 };
            _viewModel.NewCommentText = "Test comment";
            var newCommentDto = new CommentDto { Id = 1, Content = "Test comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now };
            var newComment = new Comment { Id = 1, Content = "Test comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now };
            _mockCommentService.Setup(s => s.AddCommentAsync(It.IsAny<CommentDto>())).ReturnsAsync(newComment);

            // Act
            await _viewModel.PostComment();

            // Assert
            _mockCommentService.Verify(s => s.AddCommentAsync(It.IsAny<CommentDto>()), Times.Once);
            Assert.Empty(_viewModel.NewCommentText);
            Assert.Empty(_viewModel.CommentErrorMessage);
        }

        [Fact]
        public async Task PostComment_WhenCommentIsEmpty_SetsErrorMessage()
        {
            // Arrange
            _viewModel.NewCommentText = string.Empty;

            // Act
            await _viewModel.PostComment();

            // Assert
            Assert.Equal("Comment cannot be empty.", _viewModel.CommentErrorMessage);
        }

        [Fact]
        public async Task SaveCommentChanges_WhenCommentIsValid_UpdatesComment()
        {
            // Arrange
            var comment = new Comment { Id = 1, Content = "Updated comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now };
            var updatedCommentDto = new CommentDto { Id = 1, Content = "Updated comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now };
            _viewModel.CommentToEdit = comment;
            _viewModel.IsEditingComment = true;
            _mockCommentService.Setup(s => s.UpdateCommentAsync(It.IsAny<CommentDto>())).ReturnsAsync(comment);

            // Act
            await _viewModel.SaveCommentChanges();

            // Assert
            _mockCommentService.Verify(s => s.UpdateCommentAsync(It.IsAny<CommentDto>()), Times.Once);
            Assert.False(_viewModel.IsEditingComment);
        }

        [Fact]
        public async Task DeleteCommentWithConfirmation_WhenConfirmed_DeletesComment()
        {
            // Arrange
            var comment = new Comment { Id = 1, Content = "Test comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now };
            _mockCommentService.Setup(s => s.DeleteCommentAsync(comment.Id)).ReturnsAsync(true);
            var jsRuntimeMock = new Mock<IJSRuntime>();
            jsRuntimeMock.Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>())).ReturnsAsync(true);

            // Act
            await _viewModel.DeleteCommentWithConfirmation(comment, jsRuntimeMock.Object);

            // Assert
            _mockCommentService.Verify(s => s.DeleteCommentAsync(comment.Id), Times.Once);
        }

        [Fact]
        public async Task LoadCommentsAsync_WhenCalled_LoadsComments()
        {
            // Arrange
            int videoId = 1;
            var commentDtos = new List<CommentDto> { new CommentDto { Id = 1, Content = "Test comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now } };
            var comments = new List<Comment> { new Comment { Id = 1, Content = "Test comment", VideoId = 1, UserId = 0, DateAdded = DateTime.Now } };
            _mockCommentService.Setup(s => s.GetCommentsByVideoIdAsync(videoId)).ReturnsAsync(comments);
            _mockMapper.Setup(m => m.Map<Comment>(It.IsAny<CommentDto>())).Returns(comments.First());

            // Act
            await _viewModel.LoadCommentsAsync(videoId);

            // Assert
            _mockCommentService.Verify(s => s.GetCommentsByVideoIdAsync(videoId), Times.Once);
            Assert.Single(_viewModel.Comments);
        }
    }
}
