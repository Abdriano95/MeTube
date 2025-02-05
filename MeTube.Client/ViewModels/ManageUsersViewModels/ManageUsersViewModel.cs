using CommunityToolkit.Mvvm.ComponentModel;
using MeTube.Client.Models;
using MeTube.Client.Services;
using MeTube.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace MeTube.Client.ViewModels.ManageUsersViewModels
{
    public partial class ManageUsersViewModel : ObservableValidator
    {
        private readonly IUserService _userService;
        private readonly IJSRuntime _jsRuntime;

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

        [ObservableProperty]
        private bool showUserCard = false;

        [ObservableProperty]
        private User selectedUser = null;

        [ObservableProperty]
        private int chosenUserId;
        public ObservableCollection<User> AllUsers { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<User> FilteredUsers { get; set; } = new ObservableCollection<User>();

        public List<string> Roles = new() { "User", "Admin" };
        public ManageUsersViewModel(IUserService userService, IJSRuntime jsRuntime) 
        {
            _userService = userService;
            _jsRuntime = jsRuntime;
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

        public async Task EditUserButton(User user)
        {
            ShowUserCard = true;
            SelectedUser = user;
            ChosenUserId = await GetUserId(user);
        }

        public void CloseUserCard()
        {
            ShowUserCard = false;
            SelectedUser = null;
        }
        private async Task<int> GetUserId(User user)
        {
            var hasse = await _userService.GetUserIdByEmailAsync(user.Email);
            return hasse.Value;
        }

        public async Task DeleteUserButton(User user)
        {
            int userId = await GetUserId(user);
            bool securedelete = await _jsRuntime.InvokeAsync<bool>("confirm", $"You sure you want to delete this user?");
            if (securedelete)
            {
                bool response = await _userService.DeleteUserAsync(userId);
                if (response)
                    await _jsRuntime.InvokeVoidAsync("alert", "User succesfully deleted!");
                else
                    await _jsRuntime.InvokeVoidAsync("alert", "Unable to succesfully delete user!");
            }
        }

        private async Task<bool> CheckIfUserExist(User user)
        {
            Dictionary<string, string> response = await _userService.DoesUserExistAsync(user.Username, user.Email);
            string userExists = response.Keys.FirstOrDefault();
            string responseMessage = response[userExists];
            bool userDoExist = Convert.ToBoolean(userExists);
            await _jsRuntime.InvokeVoidAsync("alert", responseMessage);
            if (userDoExist)
                return true;
            else
                return false;
        }

        public async Task SaveChangesButton(User user)
        {
            var emailCount = AllUsers.Where(a => a.Email.Equals(user.Email));
            var usernameCount = AllUsers.Where(a => a.Username.Equals(user.Username));
            if (emailCount.Count() > 1 || usernameCount.Count() > 1)
            {
                var userExist = await CheckIfUserExist(user);
                if (userExist)
                    return;
            }

            UpdateUserDto dto = new UpdateUserDto
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
                Role = user.Role,
            };
            
            bool secureupdate = await _jsRuntime.InvokeAsync<bool>("confirm", "You sure you want to update this user?");
            if (secureupdate)
            {
                bool response = await _userService.UpdateUserAsync(ChosenUserId, dto);
                string message = string.Empty;
                if(response)
                { 
                    await _jsRuntime.InvokeVoidAsync("alert", "User succesfully saved!");
                    CloseUserCard();
                }
                else
                    await _jsRuntime.InvokeVoidAsync("alert", "Unable to succesfully update user!");
            }
        }
        public void SearchButton()
        {
            if (string.IsNullOrWhiteSpace(Search))
                ResetSearchedSongs();
            IEnumerable<User> result = FilteredUsers.Where(a => a.Username.ToLower().Contains(Search.ToLower()) || a.Email.ToLower().Contains(Search.ToLower()) || a.Password.ToLower().Contains(Search.ToLower()) || a.Role.ToLower().Contains(Search.ToLower())).Distinct();
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
