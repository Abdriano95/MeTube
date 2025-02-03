using Microsoft.JSInterop;

namespace MeTube.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;

        public AuthenticationService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            return !string.IsNullOrEmpty(token);
        }

        public async Task Login(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
        }

        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
        }
    }
}
