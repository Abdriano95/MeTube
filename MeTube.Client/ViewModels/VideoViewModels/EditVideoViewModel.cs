using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace MeTube.Client.ViewModels
{
    public partial class EditVideoViewModel : ObservableValidator
    {
        private readonly IVideoService _videoService;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        private Video? currentVideo;

        [ObservableProperty]
        //[Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Title must be between 3 and 100 characters")]
        private string title = string.Empty;

        [ObservableProperty]
        //[Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 0, ErrorMessage = "Description must be between 10 and 1000 characters")]
        private string description = string.Empty;

        [ObservableProperty]
        private IBrowserFile? newVideoFile;

        [ObservableProperty]
        private IBrowserFile? newThumbnailFile;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public EditVideoViewModel(
            IVideoService videoService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime)
        {
            _videoService = videoService;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadVideoAsync(int videoId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                CurrentVideo = await _videoService.GetVideoByIdAsync(videoId);
                if (CurrentVideo != null)
                {
                    Title = CurrentVideo.Title;
                    Description = CurrentVideo.Description;
                }
                else
                {
                    ErrorMessage = "Video not found.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to load video details.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateMetadataAsync()
        {
            if (CurrentVideo == null) return;

            try
            {
                ValidateAllProperties();
                if (HasErrors)
                {
                    return;
                }

                IsLoading = true;
                CurrentVideo.Title = Title;
                CurrentVideo.Description = Description;

                var updatedVideo = await _videoService.UpdateVideoAsync(CurrentVideo);
                if (updatedVideo != null)
                {
                    await _jsRuntime.InvokeVoidAsync("alert", "Video details updated successfully!");
                    _navigationManager.NavigateTo("/videos/manage");
                }
                else
                {
                    ErrorMessage = "Failed to update video details.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred while updating video details.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateVideoFileAsync()
        {
            if (CurrentVideo == null || NewVideoFile == null) return;

            try
            {
                IsLoading = true;
                using var videoStream = NewVideoFile.OpenReadStream(500 * 1024 * 1024); // Max 500MB
                var updatedVideo = await _videoService.UpdateVideoFileAsync(CurrentVideo.Id, videoStream, NewVideoFile.Name);

                if (updatedVideo != null)
                {
                    await _jsRuntime.InvokeVoidAsync("alert", "Video file updated successfully!");
                    _navigationManager.NavigateTo("/videos/manage");
                }
                else
                {
                    ErrorMessage = "Failed to update video file.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred while updating video file.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateThumbnailAsync()
        {
            if (CurrentVideo == null || NewThumbnailFile == null) return;

            try
            {
                IsLoading = true;
                using var thumbnailStream = NewThumbnailFile.OpenReadStream(5 * 1024 * 1024); // Max 5MB
                var updatedVideo = await _videoService.UpdateVideoThumbnailAsync(CurrentVideo.Id, thumbnailStream, NewThumbnailFile.Name);

                if (updatedVideo != null)
                {
                    await _jsRuntime.InvokeVoidAsync("alert", "Thumbnail updated successfully!");
                    CurrentVideo = updatedVideo;
                }
                else
                {
                    ErrorMessage = "Failed to update thumbnail.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred while updating thumbnail.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ResetToDefaultThumbnailAsync()
        {
            if (CurrentVideo == null) return;

            try
            {
                IsLoading = true;
                var confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to reset to the default thumbnail?");
                if (!confirmed) return;

                // Implementation needed based on your requirements
                bool resetOk = await _videoService.ResetThumbnail(CurrentVideo.Id);
                if(resetOk)
                {
                    await _jsRuntime.InvokeVoidAsync("alert", "Reset to default thumbnail successful!");
                    NewThumbnailFile = null;
                    await LoadVideoAsync(CurrentVideo.Id);
                }
                else
                    await _jsRuntime.InvokeVoidAsync("alert", "Reset to default thumbnail not successful!");

            }
            catch (Exception)
            {
                ErrorMessage = "Failed to reset thumbnail.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}