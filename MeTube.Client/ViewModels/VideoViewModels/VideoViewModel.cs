using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class VideoViewModel
    {
        private readonly IVideoService _videoService;
        private readonly ICommentService _commentService;
        private readonly UserService _userService;
        private readonly NavigationManager _navigationManager;
        private readonly IMapper _mapper;

        [ObservableProperty]
        private string _commentErrorMessage;

        [ObservableProperty]
        private bool _isAuthenticated;

        [ObservableProperty]
        private string _userRole = "Customer";

        [ObservableProperty]
        private bool _isEditingComment;

        [ObservableProperty]
        private Comment _commentToEdit;

        public VideoViewModel(IVideoService videoService, ICommentService commentService, UserService userService, NavigationManager navigationManager, IMapper mapper)
        {
            _videoService = videoService;
            _commentService = commentService;
            _userService = userService;
            _navigationManager = navigationManager;
            _mapper = mapper;
            Comments = new ObservableCollection<Comment>();
        }

        public bool CanPostComment => IsAuthenticated && UserRole != "Customer";

        public async Task InitializeAsync()
        {
            var authData = await _userService.IsUserAuthenticated();
            IsAuthenticated = authData["IsAuthenticated"] == "true";
            UserRole = authData["Role"];
        }

        [ObservableProperty]
        private Video _currentVideo;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        public ObservableCollection<Comment> Comments { get; set; }

        [ObservableProperty]
        private string _newCommentText = string.Empty;

        public async Task PostComment()
        {
            if (!string.IsNullOrWhiteSpace(NewCommentText))
            {
                try
                {
                    var newCommentDto = new CommentDto
                    {
                        VideoId = CurrentVideo.Id,
                        UserId = 0, // this is checked in the API properly
                        Content = NewCommentText,
                        DateAdded = DateTime.Now
                    };

                    var postedComment = await _commentService.AddCommentAsync(newCommentDto);

                    if (postedComment != null)
                    {
                        await LoadCommentsAsync(CurrentVideo.Id);
                        NewCommentText = string.Empty;
                        CommentErrorMessage = string.Empty;
                    }
                    else
                    {
                        CommentErrorMessage = "Failed to post your comment. Please try again.";
                    }
                }
                catch (Exception ex)
                {
                    CommentErrorMessage = "An error occurred while posting the comment. Please try again.";
                    Console.Error.WriteLine($"Error posting comment: {ex.Message}");
                }
            }
            else
            {
                CommentErrorMessage = "Comment cannot be empty.";
            }
        }

        public void StartEditingComment(Comment comment)
        {
            CommentToEdit = comment;
            IsEditingComment = true;
        }

        public async Task SaveCommentChanges()
        {
            if (!string.IsNullOrEmpty(CommentToEdit.Content))
            {
                await EditCommentAsync(CommentToEdit);
                IsEditingComment = false;
                await LoadCommentsAsync(CurrentVideo.Id);
            }
        }

        public void CancelEdit()
        {
            IsEditingComment = false;
        }

        public async Task DeleteCommentWithConfirmation(Comment comment, IJSRuntime jsRuntime)
        {
            var confirmation = await jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this comment?");
            if (confirmation)
            {
                await DeleteCommentAsync(comment);
                await LoadCommentsAsync(CurrentVideo.Id);
            }
        }

        [RelayCommand]
        public async Task LoadVideoAsync(int videoId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                CurrentVideo = await _videoService.GetVideoByIdAsync(videoId);
                if (CurrentVideo != null)
                {
                    CurrentVideo.VideoUrl = Constants.VideoStreamUrl(videoId);
                    await LoadCommentsAsync(videoId);
                }
                else
                {
                    ErrorMessage = "Video could not be found";
                    _navigationManager.NavigateTo("/");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadCommentsAsync(int videoId)
        {
            var commentDtos = await _commentService.GetCommentsByVideoIdAsync(videoId);
            Comments.Clear();

            if (commentDtos != null)
            {
                foreach (var commentDto in commentDtos)
                {
                    var comment = _mapper.Map<Comment>(commentDto);
                    comment.PosterUsername = await _commentService.GetPosterUsernameAsync(comment.UserId);
                    Comments.Add(comment);
                }
            }
        }

        [RelayCommand]
        public async Task EditCommentAsync(Comment comment)
        {
            try
            {
                var updatedCommentDto = new CommentDto
                {
                    Id = comment.Id,
                    VideoId = comment.VideoId,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    DateAdded = comment.DateAdded
                };

                var updatedComment = await _commentService.UpdateCommentAsync(updatedCommentDto);

                if (updatedComment != null)
                {
                    var index = Comments.IndexOf(comment);
                    if (index >= 0)
                    {
                        Comments[index] = updatedComment;
                    }
                }
                else
                {
                    CommentErrorMessage = "Failed to edit comment. Please try again.";
                }
            }
            catch (Exception ex)
            {
                CommentErrorMessage = "An error occurred while editing the comment. Please try again.";
                Console.Error.WriteLine($"Error editing comment: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteCommentAsync(Comment comment)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(comment.Id);
                if (result)
                {
                    Comments.Remove(comment);
                }
                else
                {
                    CommentErrorMessage = "Failed to delete the comment. Please try again.";
                }
            }
            catch (Exception ex)
            {
                CommentErrorMessage = "An error occurred while deleting the comment. Please try again.";
                Console.Error.WriteLine($"Error deleting comment: {ex.Message}");
            }
        }
    }
}