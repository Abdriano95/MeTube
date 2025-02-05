using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace MeTube.Client.ViewModels.LoginViewModels
{
    public partial class LoginViewModel : ObservableValidator
    {
        [ObservableProperty]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be 3-20 characters")]
        public string username = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Password must be 3-20 characters")]
        public string password = string.Empty;

        [ObservableProperty]
        private string usernameError = string.Empty;

        [ObservableProperty]
        private string passwordError = string.Empty;

        [ObservableProperty]
        public bool isUserLoggedIn = false;

        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;
        public LoginViewModel(IUserService userService, IAuthenticationService authService, IJSRuntime jsRuntime, NavigationManager navigation) 
        {
            _userService = userService;
            _authService = authService;
            _jsRuntime = jsRuntime;
            _navigation = navigation;
        }
        public async Task LoginButton()
        {
            ValidateAllProperties();
            PasswordError = string.Empty;
            if (HasErrors)
            {
                var usernameErrors = GetErrors(nameof(Username)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                var passwordErrors = GetErrors(nameof(Password)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                UsernameError = string.Join("\n", usernameErrors);
                PasswordError = string.Join("\n", passwordErrors);
                return;
            }

            var userFound = await _userService.LoginAsync(Username, Password);

            if (userFound != null) 
            {
                string token = await _userService.GetTokenAsync(Username, Password);
                if (!string.IsNullOrEmpty(token))
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
                await _jsRuntime.InvokeAsync<bool>("alert", "Login succesfull!");
                ClearAllFields();
                IsUserLoggedIn = true;
                _navigation.NavigateTo("/", forceLoad: true);
                return;
            }
            else
            {
                 await _jsRuntime.InvokeVoidAsync("alert", "Wrong username or password!");
                 ClearAllFields();
            }

        }
        private void ClearAllFields()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        public async Task Logout()
        {
            bool secureLogoff = await _jsRuntime.InvokeAsync<bool>("confirm", $"You sure you want to log-off?");
            if (secureLogoff)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
                _navigation.NavigateTo("/login", forceLoad: true);
                await _jsRuntime.InvokeAsync<bool>("alert", "User loged-off!");
            }
            else
            {
                _navigation.NavigateTo(_navigation.Uri, forceLoad: true);
                await _jsRuntime.InvokeAsync<bool>("alert", "Unable to succesfully log-off user!");
            }
        }
    }
}
