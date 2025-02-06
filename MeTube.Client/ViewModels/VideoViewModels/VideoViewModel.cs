using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
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
        private readonly NavigationManager _navigationManager;

        public VideoViewModel(IVideoService videoService, ILikeService likeService, NavigationManager navigationManager)
        {
            _videoService = videoService;
            _likeService = likeService;
            _navigationManager = navigationManager;
            Comments = new ObservableCollection<Comment>();
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
                    ErrorMessage = "Video kunde inte hittas";
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
                ErrorMessage = $"Ett fel uppstod: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCommentsAsync(int videoId)
        {
            // Implementera när du har kommentars-API
            var mockComments = new List<Comment>
            {
                new Comment { Author = "Användare1", Text = "Bra video!", Date = DateTime.Now.AddHours(-2) },
                new Comment { Author = "Användare2", Text = "Mycket informativt", Date = DateTime.Now.AddHours(-1) }
            };

            Comments.Clear();
            foreach (var comment in mockComments)
            {
                Comments.Add(comment);
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
