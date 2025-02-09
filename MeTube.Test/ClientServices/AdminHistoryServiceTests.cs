using AutoMapper;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO.HistoryDTOs;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MeTube.Test.ClientServices
{
    public class AdminHistoryServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly HttpClient _httpClient;
        private readonly AdminHistoryService _adminHistoryService;

        public AdminHistoryServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockMapper = new Mock<IMapper>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            _adminHistoryService = new AdminHistoryService(_httpClient, _mockMapper.Object, _mockJsRuntime.Object);

            // Setup JWT token
            _mockJsRuntime.Setup(x => x.InvokeAsync<string>(It.IsAny<string>(), It.IsAny<object[]>()))
                          .ReturnsAsync("fake-jwt-token");
        }

        // --------ADMIN GET ----------

        [Fact]
        public async Task GetHistoryByUserAsync_WithValidResponse_ShouldReturnHistoryList()
        {
            // Arrange
            var historyDtos = new List<HistoryAdminDto>
                {
                    new HistoryAdminDto { UserId = 1, VideoId = 1 },
                    new HistoryAdminDto { UserId = 1, VideoId = 2 }
                };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(historyDtos), Encoding.UTF8, "application/json")
                });

            _mockMapper.Setup(m => m.Map<List<HistoryAdmin>>(It.IsAny<List<HistoryAdminDto>>()))
                       .Returns((List<HistoryAdminDto> dtos) => dtos.Select(dto => new HistoryAdmin
                       {
                           UserId = dto.UserId,
                           VideoId = dto.VideoId
                       }).ToList());

            // Act
            var result = await _adminHistoryService.GetHistoryByUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }


        [Fact]
        public async Task GetHistoryByUserAsync_WithErrorResponse_ShouldReturnEmptyList()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            // Act
            var result = await _adminHistoryService.GetHistoryByUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoryByUserAsync_WithEmptyContent_ShouldReturnEmptyList()
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
                    Content = new StringContent(string.Empty)
                });

            // Act
            var result = await _adminHistoryService.GetHistoryByUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoryByUserAsync_WithInvalidContent_ShouldReturnEmptyList()
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
                    Content = new StringContent("invalid-json")
                });

            // Act
            var result = await _adminHistoryService.GetHistoryByUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoryByUserAsync_WithException_ShouldReturnEmptyList()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _adminHistoryService.GetHistoryByUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // --------ADMIN POST ----------
        [Fact]
        public async Task CreateHistoryAsync_WithValidResponse_ShouldReturnCreatedHistory()
        {
            // Arrange
            var newHistory = new HistoryAdmin { UserId = 1, VideoId = 1 };
            var createDto = new HistoryCreateDto { UserId = 1, VideoId = 1 };
            var responseDto = new HistoryAdminDto { UserId = 1, VideoId = 1 };

            _mockMapper.Setup(m => m.Map<HistoryCreateDto>(newHistory)).Returns(createDto);
            _mockMapper.Setup(m => m.Map<HistoryAdmin>(It.IsAny<HistoryAdminDto>()))
                       .Returns((HistoryAdminDto dto) => dto == null ? null : new HistoryAdmin { UserId = dto.UserId, VideoId = dto.VideoId });

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(JsonSerializer.Serialize(responseDto), Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _adminHistoryService.CreateHistoryAsync(newHistory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal(1, result.VideoId);
        }


        [Fact]
        public async Task CreateHistoryAsync_WithErrorResponse_ShouldReturnNull()
        {
            // Arrange
            var newHistory = new HistoryAdmin { UserId = 1, VideoId = 1 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            // Act
            var result = await _adminHistoryService.CreateHistoryAsync(newHistory);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateHistoryAsync_WithEmptyContent_ShouldReturnNull()
        {
            // Arrange
            var newHistory = new HistoryAdmin { UserId = 1, VideoId = 1 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _adminHistoryService.CreateHistoryAsync(newHistory);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateHistoryAsync_WithInvalidContent_ShouldReturnNull()
        {
            // Arrange
            var newHistory = new HistoryAdmin { UserId = 1, VideoId = 1 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent("invalid-json", Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _adminHistoryService.CreateHistoryAsync(newHistory);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateHistoryAsync_WithException_ShouldReturnNull()
        {
            // Arrange
            var newHistory = new HistoryAdmin { UserId = 1, VideoId = 1 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _adminHistoryService.CreateHistoryAsync(newHistory);

            // Assert
            Assert.Null(result);
        }

        // --------ADMIN PUT ----------
        [Fact]
        public async Task UpdateHistoryAsync_WithValidResponse_ShouldReturnTrue()
        {
            // Arrange
            var history = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
            var updateDto = new HistoryUpdateDto { Id = 1, UserId = 1, VideoId = 1 };

            _mockMapper.Setup(m => m.Map<HistoryUpdateDto>(history)).Returns(updateDto);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            var result = await _adminHistoryService.UpdateHistoryAsync(history);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateHistoryAsync_WithErrorResponse_ShouldReturnFalse()
        {
            // Arrange
            var history = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };
            var updateDto = new HistoryUpdateDto { Id = 1, UserId = 1, VideoId = 1 };

            _mockMapper.Setup(m => m.Map<HistoryUpdateDto>(history)).Returns(updateDto);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            var result = await _adminHistoryService.UpdateHistoryAsync(history);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateHistoryAsync_WithException_ShouldReturnFalse()
        {
            // Arrange
            var history = new HistoryAdmin { Id = 1, UserId = 1, VideoId = 1 };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _adminHistoryService.UpdateHistoryAsync(history);

            // Assert
            Assert.False(result);
        }

        // --------ADMIN DELETE ----------
        [Fact]
        public async Task DeleteHistoryAsync_WithValidResponse_ShouldReturnTrue()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            var result = await _adminHistoryService.DeleteHistoryAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteHistoryAsync_WithErrorResponse_ShouldReturnFalse()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _adminHistoryService.DeleteHistoryAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteHistoryAsync_WithException_ShouldReturnFalse()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _adminHistoryService.DeleteHistoryAsync(1);

            // Assert
            Assert.False(result);
        }
    }
}
