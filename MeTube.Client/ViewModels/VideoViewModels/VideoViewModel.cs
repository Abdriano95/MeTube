using AutoMapper;
using Azure.Identity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class VideoViewModel
    {
        private readonly IVideoService _videoService;
        private readonly ILikeService _likeService;
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
        public string _username = "Guest";

        public VideoViewModel(IVideoService videoService, ICommentService commentService, UserService userService, NavigationManager navigationManager, IMapper mapper, ILikeService likeService)
        {
            _videoService = videoService;
            _commentService = commentService;
            _userService = userService;
            _likeService = likeService;
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

        [ObservableProperty]
        private bool _hasUserLiked;

        [ObservableProperty]
        private int _likeCount;

        [ObservableProperty]
        private bool _showLoginPrompt;


        public ObservableCollection<Comment> Comments { get; }

        [RelayCommand]
        private void RedirectToLogin()
        {
            _navigationManager.NavigateTo("/login");
        }

        [RelayCommand]
        public async Task LoadVideoAsync(int videoId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                CurrentVideo = await _videoService.GetVideoByIdAsync(videoId);

                if (CurrentVideo == null)
                {
                    CurrentVideo.VideoUrl = Constants.VideoStreamUrl(videoId);
                    // Load comments from the API
                    await LoadCommentsAsync(videoId);
                }
                else
                {
                    ErrorMessage = "Video could not be found";
                    _navigationManager.NavigateTo("/");
                    return;
                }
                CurrentVideo.VideoUrl = Constants.VideoStreamUrl(videoId);

                HasUserLiked = await _likeService.HasUserLikedVideoAsync(videoId);
                LikeCount = await _likeService.GetLikeCountForVideoAsync(videoId);

                // TODO: Hämta kommentarer från API
                await LoadCommentsAsync(videoId);
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

        [RelayCommand]
        public async Task ToggleLikeAsync()
        {
            try
            {
                bool success = HasUserLiked
                    ? await _likeService.RemoveLikeAsync(CurrentVideo.Id)
                    : await _likeService.AddLikeAsync(CurrentVideo.Id);

                if (success)
                {
                    HasUserLiked = !HasUserLiked;
                    LikeCount += HasUserLiked ? 1 : -1;
                }
                else
                {
                    ShowLoginPrompt = true;
                    OnPropertyChanged(nameof(ShowLoginPrompt));
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                ShowLoginPrompt = true;
                OnPropertyChanged(nameof(ShowLoginPrompt));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error has occured: {ex.Message}";
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }
}
