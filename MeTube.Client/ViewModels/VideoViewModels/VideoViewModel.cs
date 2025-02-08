using AutoMapper;
using Azure.Identity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
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

        public VideoViewModel(IVideoService videoService, ICommentService commentService, UserService userService, NavigationManager navigationManager, IMapper mapper)
        {
            _videoService = videoService;
            _commentService = commentService;
            _userService = userService;
            _navigationManager = navigationManager;
            _mapper = mapper;
            Comments = new ObservableCollection<Comment>();
        }

        /// <summary>
        /// Returns true if the user is authenticated and not a "Customer".
        /// </summary>
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
                    // Load comments from the API
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

        private async Task LoadCommentsAsync(int videoId)
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
                Console.WriteLine($"Loaded {Comments.Count} comments for video {videoId}");
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
