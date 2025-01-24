using CommunityToolkit.Mvvm.ComponentModel;

namespace MeTube.Client.ViewModels.LoginViewModels
{
    public partial class LoginViewModel : ObservableValidator
    {
        [ObservableProperty]
        public string username = string.Empty;

        [ObservableProperty]
        public string password = string.Empty;
        public LoginViewModel() { }

        public async Task LoginButton()
        {

        }
    }
}
