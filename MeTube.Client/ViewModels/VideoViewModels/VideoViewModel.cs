using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class VideoViewModel
    {
        private readonly VideoService _videoService;
        private readonly NavigationManager _navigationManager;

        public VideoViewModel(VideoService videoService, NavigationManager navigationManager)
        {
            _videoService = videoService;
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
        private Stream _videoStream;

        public ObservableCollection<Comment> Comments { get; }

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

                if (string.IsNullOrEmpty(CurrentVideo.VideoUrl))
                {
                    CurrentVideo.VideoUrl = $"{Constants.VideoStreamUrl}/{videoId}";
                    VideoStream = await _videoService.GetVideoStreamAsync(videoId);
                }


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
    }
}
