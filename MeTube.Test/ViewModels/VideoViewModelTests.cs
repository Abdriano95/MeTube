using AutoMapper;
using MeTube.Client;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.Client.ViewModels.VideoViewModels;
using MeTube.DTO;
using MeTube.DTO.CommentDTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                                            _mockUserService.Object,
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
        public async Task InitializeAsync_SetsIsAuthenticatedAndUserRole()
        {
            // Arrange
            var authData = new Dictionary<string, string>
            {
                { "IsAuthenticated", "true" },
                { "Role", "Admin" }
            };
            
            _mockUserService.Setup(s => s.IsUserAuthenticated())
                            .ReturnsAsync(authData);

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            Assert.True(_viewModel.IsAuthenticated);
            Assert.Equal("Admin", _viewModel.UserRole);
        }
        [Fact]
        public async Task PostComment_EmptyComment_ShowsErrorMessage()
        {
            // Arrange
            _viewModel.NewCommentText = "";  // tom sträng
            _viewModel.CommentErrorMessage = null;
            _viewModel.CurrentVideo = new Video { Id = 1 };

            // Act
            await _viewModel.PostComment();

            // Assert
            Assert.Equal("Comment cannot be empty.", _viewModel.CommentErrorMessage);
        }
        [Fact]
        public async Task PostComment_SuccessfullyAddsComment()
        {
            // Arrange
            _viewModel.NewCommentText = "New test comment";
            _viewModel.CurrentVideo = new Video { Id = 1 };
            var mockPostedComment = new Comment { Id = 123, Content = "New test comment" };

            _mockCommentService
                .Setup(s => s.AddCommentAsync(It.IsAny<CommentDto>()))
                .ReturnsAsync(mockPostedComment);

            _mockCommentService
                .Setup(s => s.GetCommentsByVideoIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Comment> { mockPostedComment });

            _mockCommentService
                .Setup(s => s.GetPosterUsernameAsync(It.IsAny<int>()))
                .ReturnsAsync("PosterUsername");

            // Act
            await _viewModel.PostComment();

            // Assert
            Assert.True(string.IsNullOrWhiteSpace(_viewModel.NewCommentText), "NewCommentText is not empty");
            Assert.Equal(string.Empty, _viewModel.CommentErrorMessage);
            Assert.Contains(mockPostedComment, _viewModel.Comments);
        }




        [Fact]
        public async Task PostComment_Failure_ShowsErrorMessage()
        {
            // Arrange
            _viewModel.NewCommentText = "Fail comment";
            _viewModel.CurrentVideo = new Video { Id = 10 };

            // Simulera null från AddCommentAsync => misslyckad post
            _mockCommentService
                .Setup(s => s.AddCommentAsync(It.IsAny<CommentDto>()))
                .ReturnsAsync((Comment)null);

            // Act
            await _viewModel.PostComment();

            // Assert
            Assert.Equal("Failed to post your comment. Please try again.", _viewModel.CommentErrorMessage);
        }
        [Fact]
        public void StartEditingComment_SetsProperties()
        {
            // Arrange
            var comment = new Comment { Id = 1, Content = "Comment to edit" };

            // Act
            _viewModel.StartEditingComment(comment);

            // Assert
            Assert.True(_viewModel.IsEditingComment);
            Assert.Equal(comment, _viewModel.CommentToEdit);
        }

        [Fact]
        public async Task SaveCommentChanges_ContentNotEmpty_CallsEditComment()
        {
            // Arrange
            var comment = new Comment { Id = 1, Content = "Edited content", VideoId = 10 };
            _viewModel.CommentToEdit = comment;
            _viewModel.IsEditingComment = true;
            _viewModel.CurrentVideo = new Video { Id = 10 };

            _mockCommentService
                .Setup(s => s.UpdateCommentAsync(It.IsAny<CommentDto>()))
                .ReturnsAsync(new Comment { Id = 1, Content = "Edited content" });

            _mockCommentService
                .Setup(s => s.GetCommentsByVideoIdAsync(10))
                .ReturnsAsync(new List<Comment> { new Comment { Id = 1, Content = "Edited content", UserId = 100 } });

            _mockCommentService
                .Setup(s => s.GetPosterUsernameAsync(It.IsAny<int>()))
                .ReturnsAsync("TestUser");

            // Act
            await _viewModel.SaveCommentChanges();

            // Assert
            Assert.False(_viewModel.IsEditingComment);
            _mockCommentService.Verify(s => s.UpdateCommentAsync(It.IsAny<CommentDto>()), Times.Once);
            _mockCommentService.Verify(s => s.GetCommentsByVideoIdAsync(It.IsAny<int>()), Times.Once);
            _mockCommentService.Verify(s => s.GetPosterUsernameAsync(It.IsAny<int>()), Times.AtLeastOnce);
        }


        [Fact]
        public async Task SaveCommentChanges_EmptyContent_DoesNothing()
        {
            // Arrange
            var comment = new Comment { Id = 2, Content = "", VideoId = 20 };
            _viewModel.CommentToEdit = comment;
            _viewModel.IsEditingComment = true;

            // Act
            await _viewModel.SaveCommentChanges();

            // Assert
            // Inga anrop till UpdateCommentAsync pga content är ""
            _mockCommentService.Verify(s => s.UpdateCommentAsync(It.IsAny<CommentDto>()), Times.Never);
            Assert.True(_viewModel.IsEditingComment); // Kvar i editing-läge
        }

        [Fact]
        public void CancelEdit_SetsIsEditingCommentFalse()
        {
            // Arrange
            _viewModel.IsEditingComment = true;

            // Act
            _viewModel.CancelEdit();

            // Assert
            Assert.False(_viewModel.IsEditingComment);
        }

        [Fact]
        public async Task DeleteCommentWithConfirmation_UserCancels_NoDeleteCall()
        {
            // Arrange
            var comment = new Comment { Id = 1 };
            var mockJsRuntime = new Mock<IJSRuntime>();
            mockJsRuntime
                .Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
                .ReturnsAsync(false); // Användare klickar "Avbryt"

            // Act
            await _viewModel.DeleteCommentWithConfirmation(comment, mockJsRuntime.Object);

            // Assert
            _mockCommentService.Verify(s => s.DeleteCommentAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCommentWithConfirmation_UserConfirms_DeletesComment()
        {
            // Arrange
            var comment = new Comment { Id = 1, VideoId = 10 };
            var mockJsRuntime = new Mock<IJSRuntime>();
            mockJsRuntime
                .Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
                .ReturnsAsync(true); // Användare klickar "OK"

            _mockCommentService
                .Setup(s => s.DeleteCommentAsync(1))
                .ReturnsAsync(true);

            _mockCommentService
                .Setup(s => s.GetCommentsByVideoIdAsync(10))
                .ReturnsAsync(new List<Comment>());

            _viewModel.CurrentVideo = new Video { Id = 10 };

            // Act
            await _viewModel.DeleteCommentWithConfirmation(comment, mockJsRuntime.Object);

            // Assert
            _mockCommentService.Verify(s => s.DeleteCommentAsync(1), Times.Once);
            _mockCommentService.Verify(s => s.GetCommentsByVideoIdAsync(10), Times.Once);
        }

        [Fact]
        public async Task LoadCommentsAsync_SuccessfullyMapsAndAddsComments()
        {
            // Arrange
            int videoId = 33;
            var comments = new List<Comment>
    {
        new Comment { Id = 1, Content = "C1", UserId = 100 },
        new Comment { Id = 2, Content = "C2", UserId = 101 }
    };

            _mockCommentService
                .Setup(s => s.GetCommentsByVideoIdAsync(videoId))
                .ReturnsAsync(comments);  

            _mockMapper
                .Setup(m => m.Map<Comment>(It.IsAny<CommentDto>()))
                .Returns<CommentDto>(dto => new Comment
                {
                    Id = dto.Id,
                    Content = dto.Content,
                    UserId = dto.UserId
                });

            _mockCommentService
                .Setup(s => s.GetPosterUsernameAsync(It.IsAny<int>()))
                .ReturnsAsync("MockUser");

            // Act
            await _viewModel.LoadCommentsAsync(videoId);

            // Assert
            Assert.Equal(2, _viewModel.Comments.Count);
            Assert.Equal("C1", _viewModel.Comments[0].Content);
            Assert.Equal("MockUser", _viewModel.Comments[0].PosterUsername);
            Assert.Equal("C2", _viewModel.Comments[1].Content);
        }


        [Fact]
        public async Task LoadCommentsAsync_404ClearsComments()
        {
            // Arrange
            _mockCommentService
                .Setup(s => s.GetCommentsByVideoIdAsync(99))
                .ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

            _viewModel.Comments.Add(new Comment { Id = 555, Content = "OldOne" });

            // Act
            await _viewModel.LoadCommentsAsync(99);

            // Assert
            Assert.Empty(_viewModel.Comments);
        }

        [Fact]
        public async Task EditCommentAsync_UpdatesCommentInList()
        {
            // Arrange
            var existingComment = new Comment { Id = 1, Content = "Old content", VideoId = 10 };
            _viewModel.Comments.Add(existingComment);

            var updatedCommentDto = new CommentDto { Id = 1, Content = "New content", VideoId = 10 };
            var updatedComment = new Comment { Id = 1, Content = "New content", VideoId = 10 };

            _mockCommentService
                .Setup(s => s.UpdateCommentAsync(It.IsAny<CommentDto>()))
                .ReturnsAsync(updatedComment);

            // Act
            await _viewModel.EditCommentAsync(existingComment);

            // Assert
            Assert.Equal("New content", _viewModel.Comments[0].Content);
        }

        [Fact]
        public async Task EditCommentAsync_FailureSetsErrorMessage()
        {
            // Arrange
            var existingComment = new Comment { Id = 1, Content = "Old content", VideoId = 10 };
            _viewModel.Comments.Add(existingComment);

            // Simulera null => update failure
            _mockCommentService
                .Setup(s => s.UpdateCommentAsync(It.IsAny<CommentDto>()))
                .ReturnsAsync((Comment)null);

            // Act
            await _viewModel.EditCommentAsync(existingComment);

            // Assert
            Assert.Equal("Failed to edit comment. Please try again.", _viewModel.CommentErrorMessage);
        }

        [Fact]
        public async Task DeleteCommentAsync_RemovesCommentFromList()
        {
            // Arrange
            var comm = new Comment { Id = 1, VideoId = 10 };
            _viewModel.Comments.Add(comm);

            _mockCommentService.Setup(s => s.DeleteCommentAsync(1)).ReturnsAsync(true);

            // Act
            await _viewModel.DeleteCommentAsync(comm);

            // Assert
            Assert.Empty(_viewModel.Comments);
        }

        [Fact]
        public async Task DeleteCommentAsync_FailureSetsErrorMessage()
        {
            // Arrange
            var comm = new Comment { Id = 2, VideoId = 10, Content = "FailThis" };
            _viewModel.Comments.Add(comm);

            _mockCommentService.Setup(s => s.DeleteCommentAsync(2)).ReturnsAsync(false);

            // Act
            await _viewModel.DeleteCommentAsync(comm);

            // Assert
            Assert.Single(_viewModel.Comments); // comment ej borttagen
            Assert.Equal("Failed to delete the comment. Please try again.", _viewModel.CommentErrorMessage);
        }

        [Fact]
        public async Task RecordHistoryAsync_CallsAddHistory_QuietlyIgnoresErrors()
        {
            // Arrange
            _viewModel.CurrentVideo = new Video { Id = 10, Title = "VidTitle" };

            // Act
            await _viewModel.RecordHistoryAsync(10);

            // Assert
            _mockHistoryService
                .Verify(s => s.AddHistoryAsync(It.Is<History>(h => h.VideoId == 10 && h.VideoTitle == "VidTitle")),
                        Times.Once);

        }
    }
}
    

