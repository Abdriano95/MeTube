using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    public partial class ManageVideosViewModel : ObservableValidator
    {
        private readonly IVideoService _videoService;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public ObservableCollection<Video> UserVideos { get; } = new();

        public ManageVideosViewModel(IVideoService videoService, NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            _videoService = videoService;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadUserVideosAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                UserVideos.Clear();

                var videos = await _videoService.GetVideosByUserIdAsync();
                if (videos != null)
                {
                    foreach (var video in videos.OrderByDescending(v => v.DateUploaded))
                    {
                        UserVideos.Add(video);
                    }
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to load videos. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void NavigateToEdit(int videoId)
        {
            _navigationManager.NavigateTo($"/videos/edit/{videoId}");
        }

        public async Task DeleteVideoAsync(Video video)
        {
            bool confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this video?");
            if (!confirmed) return;

            try
            {
                IsLoading = true;
                var success = await _videoService.DeleteVideoAsync(video.Id);
                if (success)
                {
                    UserVideos.Remove(video);
                    await _jsRuntime.InvokeVoidAsync("alert", "Video successfully deleted!");
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("alert", "Failed to delete video. Please try again.");
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred while deleting the video.";
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
