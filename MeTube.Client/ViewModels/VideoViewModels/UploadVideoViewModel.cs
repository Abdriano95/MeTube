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
        [Required(ErrorMessage = "Genre is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Please select a genre")]
        private string genre = string.Empty;

        [ObservableProperty]
        private IBrowserFile? videoFile;

        [ObservableProperty]
        private IBrowserFile? thumbnailFile;

        [ObservableProperty]
        private bool isUploading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string successMessage = string.Empty;

        public UploadVideoViewModel(
            IVideoService videoService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime)
        {
            _videoService = videoService;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        // Handle the event when a video file is selected by the user
        // This method sets the selected video file to the VideoFile property
        // It runs asynchronously to avoid blocking the UI thread
        public async Task HandleVideoFileSelected(InputFileChangeEventArgs e)
        {
            await Task.Run(() => VideoFile = e.File);
        }

        // Handle the event when a thumbnail file is selected by the user
        // This method sets the selected thumbnail file to the ThumbnailFile property
        // It runs asynchronously to avoid blocking the UI thread
        public async Task HandleThumbnailFileSelected(InputFileChangeEventArgs e)
        {
            await Task.Run(() => ThumbnailFile = e.File);
        }

        // Upload the video and its optional thumbnail to the server
        // Validate all properties and ensure a video file is selected before uploading
        // Handle any exceptions that occur during the upload process
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

                // Create a memory stream from the video file
                using var videoMS = new MemoryStream();
                await VideoFile.OpenReadStream(500 * 1024 * 1024).CopyToAsync(videoMS);
                videoMS.Position = 0;  // Reset stream position

                // Handle thumbnail file if it exists
                MemoryStream? thumbnailMS = null;
                if (ThumbnailFile != null)
                {
                    thumbnailMS = new MemoryStream();
                    await ThumbnailFile.OpenReadStream(5 * 1024 * 1024).CopyToAsync(thumbnailMS);
                    thumbnailMS.Position = 0;
                }

                var video = new Video
                {
                    Title = Title,
                    Description = Description,
                    Genre = Genre
                };

                var uploadedVideo = await _videoService.UploadVideoAsync(
                    video,
                    videoMS,
                    VideoFile.Name,
                    thumbnailMS,
                    ThumbnailFile?.Name);

                if (uploadedVideo != null)
                {
                    SuccessMessage = "Video uploaded successfully!";
                    await Task.Delay(2000);
                    _navigationManager.NavigateTo("/videos/manage");
                }
                else
                {
                    ErrorMessage = "Failed to upload video. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Upload error: {ex.Message}";
            }
            finally
            {
                IsUploading = false;
            }
        }
    }
}