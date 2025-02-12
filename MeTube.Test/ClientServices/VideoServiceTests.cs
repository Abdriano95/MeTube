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

            // VideoService kräver en IJSRuntime (för token) + IMapper + HttpClient
            _videoService = new VideoService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);

            // Låt den returnera "fake-jwt-token" när den hämtar från localStorage
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

            // Anropa metoden i VideoService
            var result = await _videoService.GetAllVideosAsync();

            // Kolla att vi inte får null + att listan har rätt antal
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


    }


}  