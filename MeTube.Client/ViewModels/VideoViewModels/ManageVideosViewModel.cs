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

        // Load videos uploaded by the current user
        // Clear any existing videos in the collection
        // Handle any exceptions that occur during the process
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

        // Load all videos available in the system
        // Clear any existing videos in the collection
        // Handle any exceptions that occur during the process
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

        // Navigate to the edit page for a specific video
        // Construct the URL using the video ID
        // Use the NavigationManager to perform the navigation
        public void NavigateToEdit(int videoId)
        {
            _navigationManager.NavigateTo($"/videos/edit/{videoId}");
        }

        // Show the delete confirmation dialog for a specific video
        // Set the video to be deleted
        // Update the flag to show the confirmation dialog
        public void ShowDeleteConfirmation(Video video)
        {
            VideoToDelete = video;
            ShowDeleteConfirmationBool = true;
        }

        // Hide the delete confirmation dialog
        // Clear the video to be deleted
        // Update the flag to hide the confirmation dialog
        public void HideDeleteConfirmation()
        {
            VideoToDelete = null;
            ShowDeleteConfirmationBool = false;
        }

        // Confirm the deletion of a video
        // Remove the video from the collection if deletion is successful
        // Handle any exceptions that occur during the process
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
                    await Task.Delay(2000); // Show the message for 2 seconds
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
