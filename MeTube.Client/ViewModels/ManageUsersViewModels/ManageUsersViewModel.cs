using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.ManageUsersViewModels
{
    public class ManageUsersViewModel : ObservableValidator
    {
        private readonly IUserService _userService;
        public ObservableCollection<User> AllUsers { get; set; } = new ObservableCollection<User>();
        public ManageUsersViewModel(IUserService userService) 
        {
            _userService = userService;
        }
        public async Task LoadUsers()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            AllUsers.Clear();
            foreach (var user in allUsers.OrderBy(a => a.Username))
                AllUsers.Add(user);
        }

        private async Task<int> GetUserId(User user)
        {
            var hasse = await _userService.GetUserIdByEmailAsync(user.Email);
            return hasse.Value;
        }

        public async Task DeleteUser(User user)
        {
            int userId = await GetUserId(user);
        }

        public async Task SaveAndUpdateUser(User user)
        {

        }
    }
}
