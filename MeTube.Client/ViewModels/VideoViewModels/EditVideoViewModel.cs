using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace MeTube.Client.ViewModels
{
    public partial class EditVideoViewModel : ObservableValidator
    {
        private readonly IVideoService _videoService;
        private readonly ILikeService _likeService;
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
        //[Required(ErrorMessage = "Genre is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Please select a genre")]
        private string genre = string.Empty;

        [ObservableProperty]
        private IBrowserFile? newVideoFile;

        [ObservableProperty]
        private IBrowserFile? newThumbnailFile;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private IEnumerable<Like>? likes;

        public EditVideoViewModel(
            IVideoService videoService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime,
            ILikeService likeService)
        {
            _videoService = videoService;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
            _likeService = likeService;
        }

        public async Task LoadLikesAsync(int videoId)
        {
            try
            {
                Likes = await _likeService.GetLikesForVideoManagementAsync(videoId);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load likes: " + ex.Message;
            }
        }

        public async Task LoadVideoAsync(int videoId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                CurrentVideo = await _videoService.GetVideoByIdAsync(videoId);
                await LoadLikesAsync(videoId);
                if (CurrentVideo != null)
                {
                    Title = CurrentVideo.Title;
                    Description = CurrentVideo.Description;
                    Genre = CurrentVideo.Genre;
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

        public async Task RemoveLikeAsAdmin(int userId)
        {
            if (CurrentVideo == null) return;

            try
            {
                var confirmed = await _jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this like?");
                if (!confirmed) return;

                var likeDto = new LikeDto
                {
                    VideoID = CurrentVideo.Id,
                    UserID = userId
                };

                await _likeService.RemoveLikesForVideoAsync(CurrentVideo.Id, userId);
                await LoadLikesAsync(CurrentVideo.Id);
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to remove like.";
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
                CurrentVideo.Genre = Genre;

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
                DateTime dateUploaded = DateTime.Now;
                string uniqeVideWithDate = dateUploaded.ToString();
                var updatedVideo = await _videoService.UpdateVideoFileAsync(CurrentVideo.Id, videoStream, NewVideoFile.Name+ uniqeVideWithDate);

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