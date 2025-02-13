using Moq;
using Xunit;
using MeTube.Client.ViewModels.VideoViewModels;
using MeTube.Client.Services;
using MeTube.Client.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MeTube.Client.Tests.ViewModels
{
    public class VideoListViewModelTests
    {
        private readonly Mock<IVideoService> _videoServiceMock;
        private readonly VideoListViewModel _viewModel;

        public VideoListViewModelTests()
        {
            _videoServiceMock = new Mock<IVideoService>();
            _viewModel = new VideoListViewModel(_videoServiceMock.Object);
        }

        [Fact]
        public async Task LoadVideosAsync_ShouldLoadVideos()
        {
            // Arrange
            var videos = new List<Video>
            {
                new Video { Id = 1, Title = "Video 1" },
                new Video { Id = 2, Title = "Video 2" }
            };
            _videoServiceMock.Setup(service => service.GetAllVideosAsync()).ReturnsAsync(videos);
            _videoServiceMock.Setup(service => service.GetUploaderUsernameAsync(It.IsAny<int>())).ReturnsAsync("Uploader");

            // Act
            await _viewModel.LoadVideosAsync();

            // Assert
            Assert.NotNull(_viewModel.Videos);
            Assert.Equal(2, _viewModel.Videos.Count);
            Assert.Equal("Uploader", _viewModel.Videos[0].UploaderUsername);
            Assert.Equal("Uploader", _viewModel.Videos[1].UploaderUsername);
            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task LoadRecommendedVideosAsync_ShouldLoadRecommendedVideos()
        {
            // Arrange
            var recommendedVideos = new List<Video>
            {
                new Video { Id = 3, Title = "Recommended Video 1" },
                new Video { Id = 4, Title = "Recommended Video 2" }
            };
            _videoServiceMock.Setup(service => service.GetRecommendedVideosAsync()).ReturnsAsync(recommendedVideos);
            _videoServiceMock.Setup(service => service.GetUploaderUsernameAsync(It.IsAny<int>())).ReturnsAsync("Uploader");

            // Act
            await _viewModel.LoadRecommendedVideosAsync();

            // Assert
            Assert.NotNull(_viewModel.RecommendedVideos);
            Assert.Equal(2, _viewModel.RecommendedVideos.Count);
            Assert.Equal("Uploader", _viewModel.RecommendedVideos[0].UploaderUsername);
            Assert.Equal("Uploader", _viewModel.RecommendedVideos[1].UploaderUsername);
            Assert.False(_viewModel.IsLoading);
        }
    }
}
