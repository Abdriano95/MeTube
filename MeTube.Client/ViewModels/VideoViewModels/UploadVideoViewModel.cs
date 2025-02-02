using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace MeTube.Client.ViewModels
{
    public partial class UploadVideoViewModel : ObservableValidator
    {
        private readonly IVideoService _videoService;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        [ObservableProperty]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        private string title = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        private string description = string.Empty;

        [ObservableProperty]
        private IBrowserFile? videoFile;

        [ObservableProperty]
        private IBrowserFile? thumbnailFile;

        [ObservableProperty]
        private bool isUploading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public UploadVideoViewModel(
            IVideoService videoService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime)
        {
            _videoService = videoService;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        public async Task HandleVideoFileSelected(InputFileChangeEventArgs e)
        {
            await Task.Run(() => VideoFile = e.File);
        }

        public async Task HandleThumbnailFileSelected(InputFileChangeEventArgs e)
        {
            await Task.Run(() => ThumbnailFile = e.File);
        }

        public async Task UploadVideoAsync()
        {
            try
            {
                ValidateAllProperties();
                if (HasErrors || VideoFile == null)
                {
                    ErrorMessage = "Please fill in all required fields and select a video file.";
                    return;
                }

                IsUploading = true;
                ErrorMessage = string.Empty;

                var video = new Video
                {
                    Title = Title,
                    Description = Description
                };

                using var videoStream = VideoFile.OpenReadStream(500 * 1024 * 1024); // Max 500MB
                using var thumbnailStream = ThumbnailFile?.OpenReadStream(5 * 1024 * 1024); // Max 5MB

                var uploadedVideo = await _videoService.UploadVideoAsync(video, videoStream, VideoFile.Name);

                if (uploadedVideo != null)
                {
                    await _jsRuntime.InvokeVoidAsync("alert", "Video uploaded successfully!");
                    _navigationManager.NavigateTo("/videos/manage");
                }
                else
                {
                    ErrorMessage = "Failed to upload video. Please try again.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred during upload. Please try again.";
            }
            finally
            {
                IsUploading = false;
            }
        }
    }
}