using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace MeTube.Client.ViewModels.SignupViewModels
{
    public partial class SignupViewModel : ObservableValidator
    {
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

        public SignupViewModel() 
        {
        }

        private bool ValidateFields()
        {

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Email) && EmailRegex.IsMatch(Email) && !string.IsNullOrEmpty(Password))
                return true;
            else
                return false;
        }

        private void ClearAllFields()
        {
            //Username = string.Empty;
            //Email = string.Empty;
            //Password = string.Empty;
        }

        [RelayCommand]
        public async Task SignupButton()
        {
            if (HasErrors)
            {
                var errors = string.Join("\n", GetErrors(null).Select(e => e.ErrorMessage));
                //await Application.Current.MainPage.DisplayAlert("Validation Error", errors, "OK");
                return;
            }

            //var success = await _customerService.RegisterCustomerAsync(customer);
            bool success = true;
            if (!success)
            {
                //await Application.Current.MainPage.DisplayAlert("Error", "Registration failed", "OK");
                return;
            }


            ClearAllFields();
        }
    }
}
