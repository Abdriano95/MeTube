using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO.CommentDTOs;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MeTube.Test.ClientServices
{
    public class CommentServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly HttpClient _httpClient;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockMapper = new Mock<IMapper>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            _commentService = new CommentService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);
            // Setup JWT token
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                         .ReturnsAsync("fake-jwt-token");
        }

        // AddCommentAsync Tests
        [Fact]
        public async Task AddCommentAsync_WithValidComment_ShouldCallPostAsync()
        {
            // Arrange
            var commentDto = new CommentDto();
            var comment = new Comment();
            _mockMapper.Setup(x => x.Map<Comment>(commentDto)).Returns(comment);
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK
                                   });
            // Act
            await _commentService.AddCommentAsync(commentDto);
            // Assert
            _mockHttpMessageHandler.Protected()
                                   .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AddCommentAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            var commentDto = new CommentDto();
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var exception = await Record.ExceptionAsync(() => _commentService.AddCommentAsync(commentDto));
            // Assert
            Assert.Null(exception);
        }

        // GetCommentsByVideoIdAsync Tests
        [Fact]
        public async Task GetCommentsByVideoIdAsync_WithValidResponse_ShouldReturnCommentCollection()
        {
            // Arrange
            var commentDtos = new List<CommentDto> { new CommentDto() };
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent(JsonSerializer.Serialize(commentDtos))
                                   });
            _mockMapper.Setup(x => x.Map<List<Comment>>(commentDtos)).Returns(new List<Comment>());
            // Act
            var result = await _commentService.GetCommentsByVideoIdAsync(1);
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetCommentsByVideoIdAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var exception = await Record.ExceptionAsync(() => _commentService.GetCommentsByVideoIdAsync(1));
            // Assert
            Assert.Null(exception);
        }

        // UpdateCommentAsync Tests
        [Fact]
        public async Task UpdateCommentAsync_WithValidComment_ShouldCallPutAsync()
        {
            // Arrange
            var commentDto = new CommentDto { Id = 1 };
            var comment = new Comment();
            _mockMapper.Setup(x => x.Map<Comment>(commentDto)).Returns(comment);
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK
                                   });
            // Act
            await _commentService.UpdateCommentAsync(commentDto);
            // Assert
            _mockHttpMessageHandler.Protected()
                                   .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task UpdateCommentAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            var commentDto = new CommentDto { Id = 1 };
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var exception = await Record.ExceptionAsync(() => _commentService.UpdateCommentAsync(commentDto));
            // Assert
            Assert.Null(exception);
        }

        // DeleteCommentAsync Tests
        [Fact]
        public async Task DeleteCommentAsync_WithValidId_ShouldCallDeleteAsync()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK
                                   });
            // Act
            var result = await _commentService.DeleteCommentAsync(1);
            // Assert
            Assert.True(result);
            _mockHttpMessageHandler.Protected()
                                   .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DeleteCommentAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var result = await _commentService.DeleteCommentAsync(1);
            // Assert
            Assert.False(result);
        }

        // GetPosterUsernameAsync Tests
        [Fact]
        public async Task GetPosterUsernameAsync_WithValidResponse_ShouldReturnUsername()
        {
            // Arrange
            var username = "testuser";
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent(username)
                                   });
            // Act
            var result = await _commentService.GetPosterUsernameAsync(1);
            // Assert
            Assert.Equal(username, result);
        }

        [Fact]
        public async Task GetPosterUsernameAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var result = await _commentService.GetPosterUsernameAsync(1);
            // Assert
            Assert.Null(result);
        }
    }
}
