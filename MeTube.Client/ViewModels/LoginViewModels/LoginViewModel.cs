using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Services;
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

        private readonly IUserService _userService;
        public LoginViewModel(IUserService userService) 
        {
            _userService = userService;
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
                ClearAllFields();
                return;
            }

        }
        private void ClearAllFields()
        {
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}
