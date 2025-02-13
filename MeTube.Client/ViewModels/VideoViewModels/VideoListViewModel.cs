using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.VideoViewModels
{
    [ObservableObject]
    public partial class VideoListViewModel
    {
        private readonly IVideoService _videoService;

        [ObservableProperty]
        private ObservableCollection<Video> videos;

        [ObservableProperty]
        private ObservableCollection<Video> recommendedVideos;
        public bool IsLoading { get; set; }



        public VideoListViewModel(IVideoService videoService)
        {
            _videoService = videoService;
        }
        //This method loads all videos from the database and sets the Videos property to the result
        [RelayCommand]
        public async Task LoadVideosAsync()
        {
            IsLoading = true;

            try
            {
                var videos = await _videoService.GetAllVideosAsync();
                Videos = new ObservableCollection<Video>(videos);

                // Set the username of the uploader for each video
                foreach (var video in Videos)
                {
                    video.UploaderUsername = await _videoService.GetUploaderUsernameAsync(video.Id);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadRecommendedVideosAsync()
        {
            IsLoading = true;
            try
            {
                // Anropar service
                var videos = await _videoService.GetRecommendedVideosAsync();

                // Dubbelkolla om du vill:
                if (videos == null)
                {
                    videos = new List<Video>();
                    videos.Clear();
                }

                // Skapa en ObservableCollection från listan
                RecommendedVideos = new ObservableCollection<Video>(videos);

                // Sätt UploaderUsername för varje video, precis som du gör i LoadVideosAsync()
                foreach (var video in RecommendedVideos)
                {
                    video.UploaderUsername = await _videoService.GetUploaderUsernameAsync(video.Id);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
