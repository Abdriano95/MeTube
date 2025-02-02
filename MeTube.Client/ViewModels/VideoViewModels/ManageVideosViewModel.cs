using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class ManageVideosViewModel
    {
        private readonly IVideoService _videoService;
        private readonly NavigationManager _navigationManager;

        public ManageVideosViewModel(IVideoService videoService, NavigationManager navigationManager)
        {
            _videoService = videoService;
            _navigationManager = navigationManager;
            Videos = new ObservableCollection<Video>();

            // Load videos when the ViewModel is created
            LoadVideosCommand.Execute(null);
        }

        [ObservableProperty]
        private ObservableCollection<Video> _videos;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isUploading;

        [ObservableProperty]
        private Video? _selectedVideo;

        [RelayCommand]
        public async Task LoadVideosAsync()
        {
            if (_isLoading) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var videos = await _videoService.GetAllVideosAsync();

                if (videos == null)
                {
                    ErrorMessage = "Kunde inte hämta videor";
                    return;
                }

                Videos.Clear();
                foreach (var video in videos)
                {
                    Videos.Add(video);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ett fel uppstod vid hämtning av videor";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteVideoAsync(int videoId)
        {
            if (_isLoading) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _videoService.DeleteVideoAsync(videoId);
                if (!result)
                {
                    ErrorMessage = "Kunde inte ta bort videon";
                    return;
                }

                var videoToRemove = Videos.FirstOrDefault(v => v.Id == videoId);
                if (videoToRemove != null)
                {
                    Videos.Remove(videoToRemove);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ett fel uppstod vid borttagning av video";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task UpdateVideoAsync(Video video)
        {
            if (_isLoading) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var updatedVideo = await _videoService.UpdateVideoAsync(video);
                if (updatedVideo == null)
                {
                    ErrorMessage = "Kunde inte uppdatera videon";
                    return;
                }

                var index = Videos.IndexOf(video);
                if (index != -1)
                {
                    Videos[index] = updatedVideo;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ett fel uppstod vid uppdatering av video";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task UpdateThumbnailAsync(UpdateThumbnailRequest request)
        {
            if (_isLoading) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var updatedVideo = await _videoService.UpdateVideoThumbnailAsync(request.VideoId, request.ThumbnailStream);
                if (updatedVideo == null)
                {
                    ErrorMessage = "Kunde inte uppdatera videominiatyren";
                    return;
                }

                var videoToUpdate = Videos.FirstOrDefault(v => v.Id == request.VideoId);
                if (videoToUpdate != null)
                {
                    var index = Videos.IndexOf(videoToUpdate);
                    Videos[index] = updatedVideo;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ett fel uppstod vid uppdatering av videominiatyr";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void NavigateToUpload()
        {
            _navigationManager.NavigateTo("/upload");
        }

        [RelayCommand]
        public void SelectVideo(Video video)
        {
            SelectedVideo = video;
        }

        [RelayCommand]
        public void NavigateToEditVideo(int videoId)
        {
            _navigationManager.NavigateTo($"/edit-video/{videoId}");
        }

        [RelayCommand]
        public void NavigateToWatchVideo(int videoId)
        {
            _navigationManager.NavigateTo($"/watch/{videoId}");
        }
    }

    public class UpdateThumbnailRequest
    {
        public int VideoId { get; set; }
        public Stream ThumbnailStream { get; set; } = null!;
    }
}