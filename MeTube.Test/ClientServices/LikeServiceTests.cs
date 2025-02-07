using AutoMapper;
using Microsoft.JSInterop;
using MeTube.Client.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using MeTube.DTO;
using MeTube.Client;
using MeTube.Client.Models;

namespace MeTube.Test.ClientServices
{
    public class LikeServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly HttpClient _httpClient;
        private readonly LikeService _likeService;

        public LikeServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockMapper = new Mock<IMapper>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            _likeService = new LikeService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);

            // Setup JWT token
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                         .ReturnsAsync("fake-jwt-token");
        }

        [Fact]
        public async Task AddLikeAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            SetupSuccessfulHttpResponse();

            // Act
            var result = await _likeService.AddLikeAsync(1);

            // Assert
            Assert.True(result);
            VerifyHttpRequestSent(HttpMethod.Post, Constants.LikeAddUrl);
        }

        [Fact]
        public async Task RemoveLikeAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            SetupSuccessfulHttpResponse();

            // Act
            var result = await _likeService.RemoveLikeAsync(1);

            // Assert
            Assert.True(result);
            VerifyHttpRequestSent(HttpMethod.Delete, Constants.LikeRemoveUrl);
        }

        [Fact]
        public async Task HasUserLikedVideoAsync_ReturnsTrue_WhenLikeExists()
        {
            // Arrange
            var responseContent = new { hasLiked = true };
            SetupSuccessfulHttpResponse(JsonSerializer.Serialize(responseContent));

            // Act
            var result = await _likeService.HasUserLikedVideoAsync(1);

            // Assert
            Assert.True(result);
            VerifyHttpRequestSent(HttpMethod.Get, Constants.LikeGetByVideoIdUrl(1));
        }

        [Fact]
        public async Task GetLikeCountForVideoAsync_ReturnsCorrectCount()
        {
            // Arrange
            var responseContent = new { count = 2, likes = new Like() };
            SetupSuccessfulHttpResponse(JsonSerializer.Serialize(responseContent));

            // Act
            var result = await _likeService.GetLikeCountForVideoAsync(1);

            // Assert
            Assert.Equal(2, result);
            VerifyHttpRequestSent(HttpMethod.Get, Constants.LikeGetForVideoUrl(1));
        }

        private void SetupSuccessfulHttpResponse(string content = "")
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                });
        }

        [Fact]
        public async Task RemoveLikesForVideoAsync_ReturnsSuccessfully_WhenVideoExists()
        {
            // Arrange
            int videoId = 1;
            SetupSuccessfulHttpResponse();

            // Act
            await _likeService.RemoveLikesForVideoAsync(videoId);

            // Assert
            VerifyHttpRequestSent(HttpMethod.Delete, $"/video/{videoId}");
        }

        [Fact]
        public async Task RemoveLikesForVideoAsync_ThrowsException_WhenRequestFails()
        {
            // Arrange
            int videoId = 1;
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Failed to remove likes")
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _likeService.RemoveLikesForVideoAsync(videoId));
        }

        private void VerifyHttpRequestSent(HttpMethod method, string url)
        {
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri.ToString().EndsWith(url)),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
