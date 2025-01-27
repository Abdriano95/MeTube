using Microsoft.JSInterop;

namespace MeTube.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthenticationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
            return !string.IsNullOrEmpty(token); // Kontrollera om token finns
        }

        public async Task Login(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token); // Spara token
        }

        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken"); // Ta bort token
        }
    }
}
