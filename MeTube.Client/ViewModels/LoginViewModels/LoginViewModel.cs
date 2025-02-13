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
        // Method to handle the login button click event
        public async Task LoginButton()
        {
            // Validate all properties
            ValidateAllProperties();
            PasswordError = string.Empty;
            if (HasErrors)
            {
                // Get and display validation errors for username and password
                var usernameErrors = GetErrors(nameof(Username)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                var passwordErrors = GetErrors(nameof(Password)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                UsernameError = string.Join("\n", usernameErrors);
                PasswordError = string.Join("\n", passwordErrors);
                return;
            }

            // Attempt to log in the user
            var userFound = await _userService.LoginAsync(Username, Password);

            if (userFound != null)
            {
                // Get and store the JWT token if login is successful
                string token = await _userService.GetTokenAsync(Username, Password);
                if (!string.IsNullOrEmpty(token))
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
                await _jsRuntime.InvokeVoidAsync("alert", "Login succesfull!");
                ClearAllFields();
                IsUserLoggedIn = true;
            }
            else
            {
                // Display an error message if login fails
                await _jsRuntime.InvokeVoidAsync("alert", "Wrong username or password!");
                ClearAllFields();
                return;
            }
            // Navigate to the home page
            _navigation.NavigateTo("", forceLoad: true);
        }
        // Method to clear all input fields
        private void ClearAllFields()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        // Method to handle the logout process
        public async Task Logout()
        {
            // Confirm the logout action with the user
            bool secureLogoff = await _jsRuntime.InvokeAsync<bool>("confirm", $"You sure you want to log-off?");
            if (secureLogoff)
            {
                // Remove the JWT token from local storage and display a logoff message
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
                await _jsRuntime.InvokeVoidAsync("alert", "User loged-off!");
                _navigation.NavigateTo("/login", forceLoad: true);
            }
            else
            {
                // Display an error message if logoff is not confirmed
                await _jsRuntime.InvokeVoidAsync("alert", "Unable to succesfully log-off user!");
                _navigation.NavigateTo(_navigation.Uri, forceLoad: true);
            }
        }
    }
}
