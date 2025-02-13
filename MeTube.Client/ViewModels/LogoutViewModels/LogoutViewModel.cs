using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace MeTube.Client.ViewModels.LogoutViewModels
{
    public class LogoutViewModel
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;
        public LogoutViewModel(IJSRuntime jsRuntime, NavigationManager navigation)
        {
            _jsRuntime = jsRuntime;
            _navigation = navigation;
        }
        public async Task Logout()
        {
            bool secureLogoff = await _jsRuntime.InvokeAsync<bool>("confirm", $"You sure you want to log-off?");
            if (secureLogoff)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
                await _jsRuntime.InvokeVoidAsync("alert", "User loged-off!");
                _navigation.NavigateTo("/login", forceLoad: true);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("alert", "Unable to succesfully log-off user!");
                _navigation.NavigateTo(_navigation.Uri, forceLoad: true);
            }
        }
    }
}
