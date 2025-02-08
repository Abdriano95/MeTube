using AutoMapper;
using MeTube.Client.Services;
using Microsoft.JSInterop;
using Moq;


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
    }
}
