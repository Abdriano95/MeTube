using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class VideoViewModel
    {
        private readonly IVideoService _videoService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly ILikeService _likeService;
        private readonly IHistoryService _historyService;
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

        public VideoViewModel(IVideoService videoService, 
                              ILikeService likeService,
                              ICommentService commentService,
                              IUserService userService,
                              IMapper mapper,
                              NavigationManager navigationManager,
                              IHistoryService historyService)
        {
            _videoService = videoService;
            _likeService = likeService;
            _historyService = historyService;
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
        [ObservableProperty]
        private bool _hasUserLiked;

        [ObservableProperty]
        private int _likeCount;

        [ObservableProperty]
        private bool _showLoginPrompt;

        [ObservableProperty]
        private string _uploaderUsername;

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
                if (CurrentVideo != null)
                {
                    CurrentVideo.VideoUrl = Constants.VideoStreamUrl(videoId);
                    await LoadCommentsAsync(videoId);
                }
                else
                {
                    ErrorMessage = "Video could not be found";
                    _navigationManager.NavigateTo("/");
                    return;
                }
                CurrentVideo.VideoUrl = Constants.VideoStreamUrl(videoId);
                UploaderUsername = await _videoService.GetUploaderUsernameAsync(videoId);
                HasUserLiked = await _likeService.HasUserLikedVideoAsync(videoId);
                LikeCount = await _likeService.GetLikeCountForVideoAsync(videoId);
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

        public async Task LoadCommentsAsync(int videoId)
        {
            try
            {
                var commentDtos = await _commentService.GetCommentsByVideoIdAsync(videoId);
                Comments.Clear();

                foreach (var commentDto in commentDtos)
                {
                    var comment = _mapper.Map<Comment>(commentDto);
                    comment.PosterUsername = await _commentService.GetPosterUsernameAsync(comment.UserId);
                    Comments.Add(comment);
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Comments.Clear();
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

        public async Task RecordHistoryAsync(int videoId)
        {
            try
            {
                History history = new History
                {
                    VideoId = videoId,
                    DateWatched = DateTime.Now,
                    VideoTitle = CurrentVideo.Title

                };

                await _historyService.AddHistoryAsync(history);
            }
            catch { } // quietly ignore errors
        }

    }
}
