using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;


namespace MeTube.Test.ClientServices
{
    public class HistoryServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly HttpClient _httpClient;
        private readonly HistoryService _historyService;

        public HistoryServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockMapper = new Mock<IMapper>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            _historyService = new HistoryService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);
            // Setup JWT token
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                         .ReturnsAsync("fake-jwt-token");
        }

        // AddHistoryAsync Tests
        [Fact]
        public async Task AddHistoryAsync_WithValidHistory_ShouldCallPostAsync()
        {
            // Arrange
            var history = new History();
            _mockMapper.Setup(x => x.Map<HistoryDto>(history))
                       .Returns(new HistoryDto());
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK
                                   });
            // Act
            await _historyService.AddHistoryAsync(history);
            // Assert
            _mockHttpMessageHandler.Protected()
                                   .Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AddHistoryAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            var history = new History();
            _mockMapper.Setup(x => x.Map<HistoryDto>(history))
                       .Returns(new HistoryDto());
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var exception = await Record.ExceptionAsync(() => _historyService.AddHistoryAsync(history));
            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task AddHistoryAsync_WithInvalidResponse_ShouldNotThrowException()
        {
            // Arrange
            var history = new History();
            _mockMapper.Setup(x => x.Map<HistoryDto>(history))
                       .Returns(new HistoryDto());
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.BadRequest
                                   });
            // Act
            var exception = await Record.ExceptionAsync(() => _historyService.AddHistoryAsync(history));
            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task AddHistoryAsync_WithNullResponse_ShouldNotThrowException()
        {
            // Arrange
            var history = new History();
            _mockMapper.Setup(x => x.Map<HistoryDto>(history))
                       .Returns(new HistoryDto());
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync((HttpResponseMessage)null);
            // Act
            var exception = await Record.ExceptionAsync(() => _historyService.AddHistoryAsync(history));
            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task AddHistoryAsync_WithEmptyContent_ShouldNotThrowException()
        {
            // Arrange
            var history = new History();
            _mockMapper.Setup(x => x.Map<HistoryDto>(history))
                       .Returns(new HistoryDto());
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent(string.Empty)
                                   });
            // Act
            var exception = await Record.ExceptionAsync(() => _historyService.AddHistoryAsync(history));
            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task AddHistoryAsync_WithInvalidContent_ShouldNotThrowException()
        {
            // Arrange
            var history = new History();
            _mockMapper.Setup(x => x.Map<HistoryDto>(history))
                       .Returns(new HistoryDto());
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent("invalid-content")
                                   });
            // Act
            var exception = await Record.ExceptionAsync(() => _historyService.AddHistoryAsync(history));
            // Assert
            Assert.Null(exception);
        }

        // GetUserHistoryAsync Tests

        [Fact]
        public async Task GetUserHistoryAsync_WithValidResponse_ShouldReturnHistoryCollection()
        {
            // Arrange
            var historyDtos = new List<HistoryDto>
            {
                new HistoryDto()
            };
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent(JsonSerializer.Serialize(historyDtos))
                                   });
            _mockMapper.Setup(x => x.Map<IEnumerable<History>>(historyDtos))
                       .Returns(new List<History>());
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithException_ShouldNotThrowException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var exception = await Record.ExceptionAsync(() => _historyService.GetUserHistoryAsync());
            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithInvalidResponse_ShouldReturnNull()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.BadRequest
                                   });
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithNullResponse_ShouldReturnNull()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync((HttpResponseMessage)null);
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetUserHistoryAsync_WithEmptyContent_ShouldReturnNull()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent(string.Empty)
                                   });
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithInvalidContent_ShouldReturnNull()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = new StringContent("invalid-content")
                                   });
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithException_ShouldReturnNull()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .Throws(new Exception());
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserHistoryAsync_WithNullContent_ShouldReturnNull()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                   .ReturnsAsync(new HttpResponseMessage
                                   {
                                       StatusCode = HttpStatusCode.OK,
                                       Content = null
                                   });
            // Act
            var result = await _historyService.GetUserHistoryAsync();
            // Assert
            Assert.Null(result);
        }
    }
}
