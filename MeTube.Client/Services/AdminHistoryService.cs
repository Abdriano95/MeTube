using AutoMapper;
using MeTube.Client.Models;
using MeTube.DTO.HistoryDTOs;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MeTube.Client.Services
{
    public class AdminHistoryService : IAdminHistoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IJSRuntime _jsRuntime;

        public AdminHistoryService(HttpClient httpClient, IMapper mapper, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Helper method: Get JWT from localStorage and set Authorization-header.
        /// </summary>
        private async Task AddAuthorizationHeaderAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        /// <summary>
        /// 1) GET: /api/History/admin/user/{userId}
        ///    Get a list of HistoryAdminDto, map too List<HistoryAdmin>
        /// </summary>
        public async Task<List<HistoryAdmin>> GetHistoryByUserAsync(int userId)
        {
            await AddAuthorizationHeaderAsync();

            try
            {
                var response = await _httpClient.GetAsync(Constants.HistoryAdminGetByUserIdUrl(userId));
                if (!response.IsSuccessStatusCode)
                {
                    // E.g 404, 400, 403 etc.
                    return new List<HistoryAdmin>();
                }

                // Deserialize JSON → List<HistoryAdminDto>
                var dtoList = await response.Content.ReadFromJsonAsync<List<HistoryAdminDto>>();
                if (dtoList == null) return new List<HistoryAdmin>();

                // Map HistoryAdminDto → List<HistoryAdmin>
                var clientList = _mapper.Map<List<HistoryAdmin>>(dtoList);
                return clientList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetHistoryByUserAsync] Error: {ex.Message}");
                return new List<HistoryAdmin>();
            }
        }

        /// <summary>
        /// 2) POST: /api/History/admin
        ///    Create a new History-post (admin)
        ///</summary>
        public async Task<HistoryAdmin?> CreateHistoryAsync(HistoryAdmin newHistory)
        {
            await AddAuthorizationHeaderAsync();

            try
            {
                // CLIENT -> CREATE-DTO
                var createDto = _mapper.Map<HistoryCreateDto>(newHistory);

                // Send JSON to server
                var response = await _httpClient.PostAsJsonAsync(Constants.HistoryAdminAddUrl, createDto);
                if (!response.IsSuccessStatusCode)
                {
                    // e.g 400 BadRequest, 404 NotFound, 403 Forbid, ...
                    return null;
                }

                // Server expects to return a HistoryAdminDto
                var responseDto = await response.Content.ReadFromJsonAsync<HistoryAdminDto>();
                if (responseDto == null) return null;

                // DTO -> CLIENT
                var createdHistory = _mapper.Map<HistoryAdmin>(responseDto);
                return createdHistory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateHistoryAsync] Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 3) PUT: /api/History/admin/{historyId}
        ///    Updates existing History-post (admin)
        /// </summary>
        public async Task<bool> UpdateHistoryAsync(HistoryAdmin history)
        {
            await AddAuthorizationHeaderAsync();

            try
            {
                // CLIENT -> UPDATE-DTO
                var updateDto = _mapper.Map<HistoryUpdateDto>(history);

                var response = await _httpClient.PutAsJsonAsync(
                    Constants.HistoryAdminUpdateUrl(history.Id),
                    updateDto
                );
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateHistoryAsync] Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 4) DELETE: /api/History/admin/{historyId}
        ///    Deletes an existing History-post (admin)
        /// </summary>
        public async Task<bool> DeleteHistoryAsync(int historyId)
        {
            await AddAuthorizationHeaderAsync();

            try
            {
                var response = await _httpClient.DeleteAsync(Constants.HistoryAdminDeleteUrl(historyId));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteHistoryAsync] Error: {ex.Message}");
                return false;
            }
        }


    }
}
