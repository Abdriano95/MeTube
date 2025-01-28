using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
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

        public async Task DeleteUserButton(User user)
        {
            int userId = await GetUserId(user);
            bool response = await _userService.DeleteUserAsync(userId);
            if (response)
            {
                string hasse = "User Deleted";
            }

        }

        public async Task SaveChangesButton(User user)
        {
            int userId = await GetUserId(user);
            UpdateUserDto dto = new UpdateUserDto
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
            };
            bool response = await _userService.UpdateUserAsync(userId, dto);
            if(response)
            {
                string hasse = "Det fungerar";
            }
        }
    }
}
