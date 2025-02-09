using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MeTube.Client.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IJSRuntime _jsRuntime;

        public HistoryService(HttpClient httpClient, IMapper mapper, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task AddHistoryAsync(History history)
        {
            await AddAuthorizationHeader();
            try
            {
                var historyDto = _mapper.Map<HistoryDto>(history);
                var content = new StringContent(
                    JsonSerializer.Serialize(historyDto),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await _httpClient.PostAsync(Constants.HistoryAddUrl, content);
                Console.WriteLine($"Response: {await response.Content.ReadAsStringAsync()}");
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<IEnumerable<History>> GetUserHistoryAsync()
        {
            await AddAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync(Constants.HistoryGetAllUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var historyDtos = JsonSerializer.Deserialize<IEnumerable<HistoryDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return _mapper.Map<IEnumerable<History>>(historyDtos);
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
