using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace MeTube.Client.ViewModels.SignupViewModels
{
    public partial class SignupViewModel : ObservableValidator
    {
        private readonly IUserService _userService;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigation;

        [ObservableProperty]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be 3-20 characters")]
        public string username = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid format. Must be using ****@****.***")]
        public string email = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 20 characters")]
        public string password = string.Empty;

        [ObservableProperty]
        private string usernameError = string.Empty;

        [ObservableProperty]
        private string passwordError = string.Empty;

        [ObservableProperty]
        private string emailError = string.Empty;

        [ObservableProperty]
        public string errorMessage = string.Empty;

        [ObservableProperty]
        public string successMessage = string.Empty;

        public SignupViewModel(IUserService userService, IJSRuntime jSRuntime, NavigationManager navigation) 
        {
            _userService = userService;
            _jsRuntime = jSRuntime;
            _navigation = navigation;
        }

        private void ClearAllFields()
        {
            Username = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }

        private async Task<bool> CheckIfUserExist()
        {
            Dictionary<string, string> response = await _userService.DoesUserExistAsync(Username, Email);
            string userExists = response.Keys.FirstOrDefault();
            string responseMessage = response[userExists];
            if (Convert.ToBoolean(userExists))
            {
                await _jsRuntime.InvokeAsync<bool>("alert", responseMessage);
                return true;
            }
            else
                return false;
        }

        public async Task SignupButton()
        {
            ValidateAllProperties();
            OnPropertyChanged(nameof(Email));
            if (HasErrors)
            {
                var usernameErrors = GetErrors(nameof(Username)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                var passwordErrors = GetErrors(nameof(Password)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                var emailErrors = GetErrors(nameof(Email)).OfType<ValidationResult>().Select(e => e.ErrorMessage);
                UsernameError = string.Join("\n", usernameErrors);
                PasswordError = string.Join("\n", passwordErrors);
                EmailError = string.Join("\n", emailErrors);
                return;
            }

            var userExist = await CheckIfUserExist();
            if (userExist)
                return;

            var newUser = new User
            {
                Username = Username,
                Email = Email,
                Password = Password,
            };

            var success = await _userService.RegisterUserAsync(newUser);
            if (!success)
            {
                await _jsRuntime.InvokeVoidAsync("alert", "Unable to signup!");
                return;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("alert", "Account succesfully created!");
                ClearAllFields();
                if(_navigation != null)
                    _navigation.NavigateTo("/login", forceLoad: true);
            }
        }
    }
}
