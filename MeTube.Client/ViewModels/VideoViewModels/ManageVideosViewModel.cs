using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels
{
    [ObservableObject]
    public partial class ManageVideosViewModel
    {
        private readonly IVideoService _videoService;
 

        [ObservableProperty]
        private ObservableCollection<Video> _userVideos = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        public ManageVideosViewModel(IVideoService videoService)
        {
            _videoService = videoService;
 
        }

        [RelayCommand]
        public async Task LoadUserVideosAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                ////var userId = _authService.CurrentUser?.Id;
                //if (string.IsNullOrEmpty(userId))
                //{
                //    ErrorMessage = "User not authenticated";
                //    return;
                //}

                //var videos = await _videoService.GetVideosByUserIdAsync();
                //UserVideos = new ObservableCollection<Video>(videos);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading videos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteVideoAsync(int videoId)
        {
            try
            {
                var success = await _videoService.DeleteVideoAsync(videoId);
                if (success)
                {
                    var video = UserVideos.FirstOrDefault(v => v.Id == videoId);
                    if (video != null) UserVideos.Remove(video);
                }
                else
                {
                    ErrorMessage = "Failed to delete video";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Delete error: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task UpdateVideoMetadataAsync(Video video)
        {
            try
            {
                var updatedVideo = await _videoService.UpdateVideoAsync(video);
                if (updatedVideo != null)
                {
                    var index = UserVideos.IndexOf(video);
                    UserVideos[index] = updatedVideo;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Update error: {ex.Message}";
            }
        }
    }
}