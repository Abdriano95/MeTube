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
        public bool IsLoading { get; set; }

        public VideoListViewModel(IVideoService videoService)
        {
            _videoService = videoService;     
        }

        [RelayCommand]
        public async Task LoadVideosAsync()
        {
            IsLoading = true;

            try
            {
                var videos = await _videoService.GetAllVideosAsync();
                Videos = new ObservableCollection<Video>(videos);
            }
            finally
            {
                IsLoading = false;
            }           
        }

    }
}
