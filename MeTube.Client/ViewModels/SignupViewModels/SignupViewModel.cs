using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeTube.Client.Models;
using MeTube.Client.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace MeTube.Client.ViewModels.SignupViewModels
{
    public partial class SignupViewModel : ObservableValidator
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, ErrorMessage = "Username cannot exceed more or less than 3-20 characters.")]
        public string username = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string email = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 20 characters.")]
        public string password = string.Empty;

        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public SignupViewModel(IUserService userService) 
        {
            _userService = userService;
        }

        private void ClearAllFields()
        {
            Username = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }

        public async Task SignupButton()
        {
            
            if (HasErrors && !EmailRegex.IsMatch(Email))
            {
                var errors = string.Join("\n", GetErrors(null).Select(e => e.ErrorMessage));
                //await Application.Current.MainPage.DisplayAlert("Validation Error", errors, "OK");
                return;
            }

            var newUser = new User
            {
                Username = Username,
                Email = Email,
                Password = Password,
            };

            var success = await _userService.RegisterUserAsync(newUser);
            if (!success)
            {
                //await Application.Current.MainPage.DisplayAlert("Error", "Registration failed", "OK");
                return;
            }
            else
                ClearAllFields();



            //Gå exempelvis till login
        }
    }
}
