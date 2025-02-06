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

        [ObservableProperty]
        private bool showDeleteConfirmationBool;

        [ObservableProperty]
        private Video? videoToDelete;

        [ObservableProperty]
        private string successMessage = string.Empty;

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

        public async Task LoadAllVideos()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                UserVideos.Clear();

                var videos = await _videoService.GetAllVideosAsync();
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

        public void ShowDeleteConfirmation(Video video)
        {
            VideoToDelete = video;
            ShowDeleteConfirmationBool = true;
        }

        public void HideDeleteConfirmation()
        {
            VideoToDelete = null;
            ShowDeleteConfirmationBool = false;
        }

        public async Task ConfirmDeleteVideoAsync()
        {
            if (VideoToDelete == null) return;

            try
            {
                IsLoading = true;
                var success = await _videoService.DeleteVideoAsync(VideoToDelete.Id);
                if (success)
                {
                    UserVideos.Remove(VideoToDelete);
                    SuccessMessage = "Video successfully deleted!";
                    await Task.Delay(2000); // Visa meddelandet i 2 sekunder
                    SuccessMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "Failed to delete video. Please try again.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred while deleting the video.";
            }
            finally
            {
                HideDeleteConfirmation();
                IsLoading = false;
            }
        }

    }
}
