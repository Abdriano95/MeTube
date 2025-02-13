using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO.VideoDTOs;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using static System.Net.WebRequestMethods;

namespace MeTube.Test.ClientServices
{
    public class VideoServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly HttpClient _httpClient;
        private readonly VideoService _videoService;

        public VideoServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockMapper = new Mock<IMapper>();
            _mockJsRuntime = new Mock<IJSRuntime>();

            
            _videoService = new VideoService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);

            
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync("fake-jwt-token");
        }


        private void SetupHttpResponse(HttpStatusCode statusCode, string content = "")
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
        }


        [Fact]
        public async Task GetAllVideosAsync_ReturnsList_WhenSuccessful()
        {
            // Skapa testdatan som JSON
            var videoDtos = new List<VideoDto>
            {
                new VideoDto {
                    Id = 1,
                    Title = "Title1",
                    Description = "Desc1",
                    Genre = "Genre1",
                    VideoUrl = "http://some/video1.mp4",
                    ThumbnailUrl = "http://some/thumb1.jpg",
                    DateUploaded = DateTime.UtcNow,
                    UserId = 10,
                    BlobExists = false
                },
                new VideoDto {
                    Id = 2,
                    Title = "Title2",
                    Description = "Desc2",
                    Genre = "Genre2",
                    VideoUrl = "http://some/video2.mp4",
                    ThumbnailUrl = "http://some/thumb2.jpg",
                    DateUploaded = DateTime.UtcNow,
                    UserId = 20,
                    BlobExists = true
                }
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(videoDtos, options);

            // Mocka respons 200 OK + JSON
            SetupHttpResponse(HttpStatusCode.OK, json);

            // Mocka AutoMapper
            _mockMapper
                .Setup(m => m.Map<List<Video>>(It.IsAny<List<VideoDto>>()))
                .Returns(new List<Video>
                {
                      new Video { Id=1, Title="Title1", Description="Desc1", Genre="Genre1" },
                      new Video { Id=2, Title="Title2", Description="Desc2", Genre="Genre2" }
                  });

            
            var result = await _videoService.GetAllVideosAsync();

           
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
        [Fact]
        public async Task GetAllVideosAsync_ReturnsNull_WhenException()
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                 )
                .ThrowsAsync(new Exception("Network error"));

            var result = await _videoService.GetAllVideosAsync();
            Assert.Null(result);
        }

        [Fact]
        public async Task GetVideoByIdAsync_ReturnsVideo_WhenSuccessful()
        {
            var dto = new VideoDto
            {
                Id = 99,
                Title = "TestVid",
                Description = "DescTest",
                Genre = "GenreTest",
                VideoUrl = "http://video/vid99.mp4",
                ThumbnailUrl = "http://video/thumb99.jpg",
                DateUploaded = DateTime.UtcNow,
                UserId = 5,
                BlobExists = true
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(dto, options);
            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper
            .Setup(m => m.Map<Video>(It.IsAny<VideoDto>()))
            .Returns(new Video { Id = 99, Title = "TestVid" });

            var result = await _videoService.GetVideoByIdAsync(99);

            Assert.NotNull(result);
            Assert.Equal(99, result.Id);
        }
        [Fact]
        public async Task GetVideoByIdAsync_ReturnsNull_WhenFail()
        {
            SetupHttpResponse(HttpStatusCode.BadRequest);
            var result = await _videoService.GetVideoByIdAsync(1000);
            Assert.Null(result);
        }
        [Fact]
        public async Task GetVideosByUserIdAsync_ReturnsList_WhenOk()
        {
            var videoDtos = new List<VideoDto>
            {
                new VideoDto {
                    Id=10, Title="UserVid", Description="Udesc", Genre="Ugenre",
                    VideoUrl="http://some/uvideo.mp4", ThumbnailUrl=null,
                    DateUploaded=DateTime.UtcNow, UserId=1, BlobExists=false
                }
            };

            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(videoDtos, opts);
            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper
                .Setup(m => m.Map<List<Video>>(It.IsAny<List<VideoDto>>()))
                .Returns(new List<Video>
                {
                    new Video { Id=10, Title="UserVid", Description="Udesc", Genre="Ugenre" }
                });

            var result = await _videoService.GetVideosByUserIdAsync();
            Assert.NotNull(result);
            Assert.Single(result);
        }
        [Fact]
        public async Task GetVideosByUserIdAsync_ReturnsNull_WhenException()
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception("Some error"));

            var result = await _videoService.GetVideosByUserIdAsync();
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteVideoAsync_ReturnsTrue_WhenSuccess()
        {
            SetupHttpResponse(HttpStatusCode.OK);
            var result = await _videoService.DeleteVideoAsync(77);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteVideoAsync_ReturnsFalse_WhenException()
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception());

            var result = await _videoService.DeleteVideoAsync(1);
            Assert.False(result);
        }

        [Fact]
        public async Task UploadVideoAsync_ReturnsVideo_WhenSuccess()
        {
            var createdDto = new VideoDto
            {
                Id = 123,
                Title = "CreatedTitle",
                Description = "DescCreated",
                Genre = "GenreC",
                VideoUrl = "http://created/url.mp4",
                ThumbnailUrl = "http://created/thumb.jpg",
                DateUploaded = DateTime.UtcNow,
                UserId = 5,
                BlobExists = false
            };
            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(createdDto, opts);

            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper
                .Setup(m => m.Map<Video>(It.IsAny<VideoDto>()))
                .Returns(new Video { Id = 123, Title = "CreatedTitle" });

            var video = new Video
            {
                Id = 0,
                Title = "ToCreate",
                Description = "DescToCreate",
                Genre = "GenreToCreate"
            };

            using var ms = new MemoryStream(new byte[] { 1, 2, 3 });
            var result = await _videoService.UploadVideoAsync(video, ms, "someVideo.mp4");
            Assert.NotNull(result);
            Assert.Equal(123, result.Id);
        }

        [Fact]
        public async Task UploadVideoAsync_ReturnsNull_WhenFailStatusCode()
        {
            SetupHttpResponse(HttpStatusCode.BadRequest);

            var video = new Video { Title = "FailVid", Description = "FailDesc", Genre = "FailGenre" };
            using var ms = new MemoryStream(new byte[] { 1, 2, 3 });
            var result = await _videoService.UploadVideoAsync(video, ms, "fail.mp4");
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateVideoAsync_ReturnsUpdatedVideo_WhenSuccess()
        {
            var dto = new VideoDto
            {
                Id = 10,
                Title = "UpdatedTitle",
                Description = "UpdatedDesc",
                Genre = "UpdatedGenre",
                VideoUrl = "http://some/update.mp4",
                ThumbnailUrl = "http://some/upthumb.jpg",
                DateUploaded = DateTime.UtcNow,
                UserId = 5,
                BlobExists = true
            };
            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(dto, opts);
            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper.Setup(m => m.Map<VideoDto>(It.IsAny<Video>()))
                       .Returns(dto);
            _mockMapper.Setup(m => m.Map<Video>(It.IsAny<VideoDto>()))
                       .Returns(new Video { Id = 10, Title = "UpdatedTitle" });

            var result = await _videoService.UpdateVideoAsync(new Video { Id = 10, Title = "OldTitle" });
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("UpdatedTitle", result.Title);
        }

        [Fact]
        public async Task UpdateVideoAsync_ReturnsNull_WhenException()
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception());

            var result = await _videoService.UpdateVideoAsync(new Video { Id = 99 });
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateVideoFileAsync_ReturnsVideo_WhenSuccess()
        {
            var dto = new VideoDto
            {
                Id = 88,
                Title = "FileUpdated",
                VideoUrl = "http://fileUpdated.mp4",
                ThumbnailUrl = "http://some/filethumb.jpg",
                DateUploaded = DateTime.UtcNow,
                UserId = 5,
                BlobExists = false
            };
            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(dto, opts);

            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper
                .Setup(m => m.Map<Video>(It.IsAny<VideoDto>()))
                .Returns(new Video { Id = 88, Title = "FileUpdated" });

            using var fileStream = new MemoryStream(new byte[] { 10, 20, 30 });
            var result = await _videoService.UpdateVideoFileAsync(88, fileStream, "myfile.mp4");
            Assert.NotNull(result);
            Assert.Equal(88, result.Id);
        }

        [Fact]
        public async Task UpdateVideoFileAsync_ReturnsNull_WhenNotSuccess()
        {
            SetupHttpResponse(HttpStatusCode.InternalServerError);
            using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });

            var result = await _videoService.UpdateVideoFileAsync(99, fileStream, "fail.mp4");
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateVideoThumbnailAsync_ReturnsVideo_WhenSuccess()
        {
            var dto = new VideoDto { Id = 55, Title = "ThumbOk", VideoUrl = "http://some/t55.mp4" };
            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(dto, opts);

            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper
                .Setup(m => m.Map<Video>(It.IsAny<VideoDto>()))
                .Returns(new Video { Id = 55, Title = "ThumbOk" });

            using var thumbStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var result = await _videoService.UpdateVideoThumbnailAsync(55, thumbStream, "thumb.jpg");
            Assert.NotNull(result);
            Assert.Equal(55, result.Id);
        }

        [Fact]
        public async Task UpdateVideoThumbnailAsync_ReturnsNull_WhenFailure()
        {
            SetupHttpResponse(HttpStatusCode.BadRequest);
            using var thumbStream = new MemoryStream(new byte[] { 1, 2, 3 });

            var result = await _videoService.UpdateVideoThumbnailAsync(10, thumbStream, "failthumb.jpg");
            Assert.Null(result);
        }

        [Fact]
        public async Task ResetThumbnail_ReturnsTrue_WhenOk()
        {
            SetupHttpResponse(HttpStatusCode.OK);
            var result = await _videoService.ResetThumbnail(77);
            Assert.True(result);
        }

        [Fact]
        public async Task ResetThumbnail_ReturnsFalse_WhenException()
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new Exception());

            var result = await _videoService.ResetThumbnail(77);
            Assert.False(result);
        }

        [Fact]
        public async Task GetUploaderUsernameAsync_ReturnsName_WhenSuccess()
        {
            SetupHttpResponse(HttpStatusCode.OK, "MockUser");
            var result = await _videoService.GetUploaderUsernameAsync(11);
            Assert.Equal("MockUser", result);
        }

        [Fact]
        public async Task GetUploaderUsernameAsync_ReturnsNull_WhenFailure()
        {
            SetupHttpResponse(HttpStatusCode.NotFound);
            var result = await _videoService.GetUploaderUsernameAsync(55);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRecommendedVideosAsync_WithEmptyResponse_ShouldReturnEmptyList()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            _mockMapper.Setup(m => m.Map<List<Video>>(It.IsAny<List<VideoDto>>()))
                       .Returns(new List<Video>());

            // Act
            var result = await _videoService.GetRecommendedVideosAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        // test to see if videos are returned mapped correctly
        public async Task GetRecommendedVideosAsync_WithVideos_ShouldReturnVideos()
        {
            // Arrange
            var videoDtos = new List<VideoDto>
                            {
                                new VideoDto { Id = 1, Title = "Video 1" },
                                new VideoDto { Id = 2, Title = "Video 2" }
                            };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(videoDtos))
                });
            _mockMapper.Setup(m => m.Map<List<Video>>(It.IsAny<List<VideoDto>>()))
                       .Returns(new List<Video>
                       {
                       new Video { Id = 1, Title = "Video 1" },
                       new Video { Id = 2, Title = "Video 2" }
                       });
            // Act
            var result = await _videoService.GetRecommendedVideosAsync();
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Video 1", result[0].Title);
            Assert.Equal("Video 2", result[1].Title);
        }

        [Fact]
        public async Task GetRecommendedVideosAsync_WithValidResponse_ShouldReturnMappedVideos()
        {
            // Arrange
            var videoDtos = new List<VideoDto>
                            {
                                new VideoDto { Id = 1, Title = "Video 1" },
                                new VideoDto { Id = 2, Title = "Video 2" }
                            };
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(videoDtos))
                });
            _mockMapper.Setup(m => m.Map<List<Video>>(It.IsAny<List<VideoDto>>()))
                       .Returns(new List<Video>
                       {
                       new Video { Id = 1, Title = "Video 1" },
                       new Video { Id = 2, Title = "Video 2" }
                       });
            // Act
            var result = await _videoService.GetRecommendedVideosAsync();
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Video 1", result[0].Title);
            Assert.Equal("Video 2", result[1].Title);
        }
    }
}  