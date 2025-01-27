using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.ManageUsersViewModels
{
    public class ManageUsersViewModel : ObservableValidator
    {
        private readonly IUserService _userService;

        ObservableCollection<User> AllUsers { get; set; } = new ObservableCollection<User>();
        public ManageUsersViewModel(IUserService userService) 
        {
            _userService = userService;
        }

        private async void GetUsers()
        {
            //AllUsers
        }
        
    }
}
