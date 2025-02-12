using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO.VideoDTOs;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

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
            var videoDtos = new List<VideoDto>
            {
                new VideoDto { Id = 1, Title = "Title1" },
                new VideoDto { Id = 2, Title = "Title2" }
            };
            var json = JsonSerializer.Serialize(videoDtos);
            SetupHttpResponse(HttpStatusCode.OK, json);

            _mockMapper
                .Setup(m => m.Map<List<Video>>(videoDtos))
                .Returns(new List<Video>
                {
                    new Video { Id = 1, Title = "Title1" },
                    new Video { Id = 2, Title = "Title2" }
                });

            var result = await _videoService.GetAllVideosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }


 }  