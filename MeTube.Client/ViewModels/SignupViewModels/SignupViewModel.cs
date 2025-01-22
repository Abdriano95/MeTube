using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace MeTube.Client.ViewModels.SignupViewModels
{
    public partial class SignupViewModel : ObservableObject
    {
        [ObservableProperty]
        string username = string.Empty;

        [ObservableProperty]
        string email = string.Empty;

        [ObservableProperty]
        string password = string.Empty;

        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public SignupViewModel() { }

        private bool ValidateFields()
        {
            if(!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Email) && EmailRegex.IsMatch(Email) && !string.IsNullOrEmpty(Password))
                return true;
            else
                return false;
        }

        [RelayCommand]
        public void SignupButton()
        { 
            bool isValid = ValidateFields();
            //Check if user exist, else create user

            //if(isValid && !userDontExist)
        }
    }
}
