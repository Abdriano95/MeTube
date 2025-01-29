using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.ManageUsersViewModels
{
    public partial class ManageUsersViewModel : ObservableValidator
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private string search = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string userRole = string.Empty;

        [ObservableProperty]
        public string selectedRole = string.Empty;
        public ObservableCollection<User> AllUsers { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<User> FilteredUsers { get; set; } = new ObservableCollection<User>();
        public ManageUsersViewModel(IUserService userService) 
        {
            _userService = userService;
        }
        public async Task LoadUsers()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            AllUsers.Clear();
            foreach (var user in allUsers.OrderBy(a => a.Username))
            {
                AllUsers.Add(user);
                FilteredUsers.Add(user);
            }
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
                Role = SelectedRole,
            };

            bool response = await _userService.UpdateUserAsync(userId, dto);
            if(response)
            {
                string hasse = "Det fungerar";
            }
        }
        public void SearchButton()
        {
            if (string.IsNullOrWhiteSpace(Search))
                ResetSearchedSongs();
            IEnumerable<User> result = FilteredUsers.Where(a => a.Username.ToLower().Contains(Search.ToLower()) || a.Email.ToLower().Contains(Search.ToLower()) || a.Password.ToLower().Contains(Search.ToLower())).Distinct();
            AllUsers.Clear();
            foreach (User user in result)
                AllUsers.Add(user);
        }

        private void ResetSearchedSongs()
        {
            AllUsers.Clear();
            foreach (User song in FilteredUsers.Distinct())
                AllUsers.Add(song);
            return;
        }
    }
}
