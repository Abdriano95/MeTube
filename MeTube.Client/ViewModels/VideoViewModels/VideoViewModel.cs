using AutoMapper;
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
        private readonly NavigationManager _navigationManager;
        private readonly IMapper _mapper;

        [ObservableProperty]
        private string _commentErrorMessage;

        public VideoViewModel(IVideoService videoService, ICommentService commentService, NavigationManager navigationManager, IMapper mapper)
        {
            _videoService = videoService;
            _commentService = commentService;
            _navigationManager = navigationManager;
            _mapper = mapper;
            Comments = new ObservableCollection<Comment>();
        }

        [ObservableProperty]
        private Video _currentVideo;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        public ObservableCollection<Comment> Comments { get; }

        [RelayCommand]
        public async Task LoadVideoAsync(int videoId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                CurrentVideo = await _videoService.GetVideoByIdAsync(videoId);
                CurrentVideo.VideoUrl = Constants.VideoStreamUrl(videoId);

                if (CurrentVideo == null)
                {
                    ErrorMessage = "Video could not be found";
                    _navigationManager.NavigateTo("/");
                    return;
                }

                // Load comments from the API
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
                // Skapa en DTO med den nya kommentarens innehåll
                var updatedCommentDto = new CommentDto
                {
                    Id = comment.Id,
                    VideoId = comment.VideoId,
                    UserId = comment.UserId,
                    Content = comment.Content, // Här kan du lägga till logik för att hantera den nya texten
                    DateAdded = comment.DateAdded
                };

                var updatedComment = await _commentService.UpdateCommentAsync(updatedCommentDto);

                if (updatedComment != null)
                {
                    // Uppdatera kommentaren i listan
                    var index = Comments.IndexOf(comment);
                    if (index >= 0)
                    {
                        Comments[index] = updatedComment;  // Uppdatera den gamla kommentaren med den nya
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
                    // Ta bort kommentaren från listan
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
